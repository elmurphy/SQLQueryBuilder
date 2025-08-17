using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Represents a parsed member expression with its table alias and column name.
    /// </summary>
    /// <param name="TableAlias">The table alias used in the SQL query</param>
    /// <param name="ColumnName">The column name from the entity property</param>
    public record ParsedMember(string TableAlias, string ColumnName);

    /// <summary>
    /// Static class that provides expression parsing functionality for converting LINQ expressions to SQL WHERE conditions.
    /// </summary>
    public static class ExpressionParser
    {
        /// <summary>
        /// Parses a LINQ expression into a SQL WHERE condition structure.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance containing type mappings</param>
        /// <param name="expression">The LINQ expression to parse</param>
        /// <returns>A SQBSqlWhere object representing the SQL condition</returns>
        public static SQBSqlWhere Parse<T>(SQLQueryBuilder<T> builder, Expression expression) where T : class, new()
        {
            var visitor = new AliasExpressionVisitor(builder);
            return visitor.Translate(expression);
        }

        /// <summary>
        /// Parses a LINQ expression to extract member information (table aliases and column names).
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance containing type mappings</param>
        /// <param name="expression">The LINQ expression to parse</param>
        /// <returns>A list of ParsedMember objects containing table aliases and column names</returns>
        public static List<ParsedMember> ParseMembers<T>(SQLQueryBuilder<T> builder, Expression expression) where T : class, new()
        {
            var visitor = new AliasExpressionVisitor(builder);
            visitor.Translate(expression);
            return visitor.ParsedMembers;
        }

        private class AliasExpressionVisitor : ExpressionVisitor
        {
            private readonly object _builder;
            private readonly List<ParsedMember> _parsedMembers = new();
            private SQBSqlWhere _currentWhere;

            public List<ParsedMember> ParsedMembers => _parsedMembers;

            public AliasExpressionVisitor(object builder)
            {
                _builder = builder;
            }

            public SQBSqlWhere Translate(Expression expression)
            {
                Visit(expression);
                return _currentWhere;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (TryGetTableAlias(node, out var tableAlias, out var memberName))
                {
                    if (!_parsedMembers.Any(m => m.TableAlias == tableAlias && m.ColumnName == memberName))
                    {
                        _parsedMembers.Add(new ParsedMember(tableAlias, memberName));
                    }

                    if (node.Type == typeof(bool))
                    {
                        _currentWhere = new SQBSqlWhere { ColumnName = memberName, TableAlias = tableAlias, Operation = "=", Value = "1" };
                    }
                }
                else if (!CanBeEvaluatedAsValue(node))
                {
                    return base.VisitMember(node);
                }

                return node;
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse)
                {
                    Visit(node.Left);
                    var left = _currentWhere;

                    _currentWhere = null;

                    Visit(node.Right);
                    var right = _currentWhere;

                    _currentWhere = new SQBSqlWhere
                    {
                        Operator = node.NodeType == ExpressionType.AndAlso ? LogicalOperator.AND : LogicalOperator.OR,
                        NestedConditions = new List<SQBSqlWhere> { left, right }
                    };
                }
                else
                {
                    _currentWhere = ParseSimpleBinaryExpression(node);
                }
                return node;
            }

            protected override Expression VisitNew(NewExpression node)
            {
                foreach (var arg in node.Arguments)
                {
                    Visit(arg);
                }
                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (TryGetTableAlias(node.Object, out var tableAlias, out var memberName))
                {
                    var value = GetValueFromExpression(node.Arguments[0]);

                    _currentWhere = node.Method.Name switch
                    {
                        "Contains" => new SQBSqlWhere { ColumnName = memberName, TableAlias = tableAlias, Operation = "LIKE", Value = $"'%{value}%'" },
                        "StartsWith" => new SQBSqlWhere { ColumnName = memberName, TableAlias = tableAlias, Operation = "LIKE", Value = $"'{value}%'" },
                        "EndsWith" => new SQBSqlWhere { ColumnName = memberName, TableAlias = tableAlias, Operation = "LIKE", Value = $"'%{value}'" },
                        _ => throw new NotSupportedException($"Unsupported string method: {node.Method.Name}")
                    };
                }
                else
                {
                    throw new NotSupportedException($"Method call must be on a member property: {node.Object}");
                }

                return node;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (node.NodeType == ExpressionType.Not && node.Operand is MemberExpression member)
                {
                    if (TryGetTableAlias(member, out var tableAlias, out var memberName))
                    {
                        _currentWhere = new SQBSqlWhere { ColumnName = memberName, TableAlias = tableAlias, Operation = "=", Value = "0" };
                    }
                }
                else if (node.NodeType == ExpressionType.Convert)
                {
                    return Visit(node.Operand);
                }

                return node;
            }

            private SQBSqlWhere ParseSimpleBinaryExpression(BinaryExpression binaryExpression)
            {
                var fieldSide = binaryExpression.Left;
                var valueSide = binaryExpression.Right;
                var op = GetSqlOperator(binaryExpression.NodeType);

                if (CanBeEvaluatedAsValue(fieldSide))
                {
                    (fieldSide, valueSide) = (valueSide, fieldSide);
                    op = FlipOperator(op);
                }

                if (!TryGetTableAlias(fieldSide, out var tableAlias, out var memberName))
                {
                    throw new NotSupportedException($"Unsupported expression for a database field: {fieldSide}");
                }

                var valueObject = GetValueFromExpression(valueSide);

                return new SQBSqlWhere
                {
                    ColumnName = memberName,
                    TableAlias = tableAlias,
                    Operation = op,
                    Value = FormatValue(valueObject)
                };
            }

            private bool TryGetTableAlias(Expression expression, out string tableAlias, out string memberName)
            {
                tableAlias = null;
                memberName = null;

                if (expression is not MemberExpression memberExp) return false;

                memberName = memberExp.Member.Name;
                var typeToAliasMap = (Dictionary<Type, string>)_builder.GetType().GetField("typeToAliasMap", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_builder);

                if (memberExp.Expression is ParameterExpression paramExp)
                {
                    if (typeToAliasMap.TryGetValue(paramExp.Type, out var alias))
                    {
                        tableAlias = alias;
                        return true;
                    }
                }

                if (memberExp.Expression is MemberExpression innerMemberExp)
                {
                    var propertyInfo = innerMemberExp.Member as PropertyInfo;
                    if (propertyInfo != null && typeToAliasMap.TryGetValue(propertyInfo.PropertyType, out var alias))
                    {
                        tableAlias = alias;
                        return true;
                    }
                }

                return false;
            }
        }

        #region Helper Methods
        /// <summary>
        /// Extracts the actual value from an expression by evaluating constants or compiling and invoking lambda expressions.
        /// </summary>
        /// <param name="expression">The expression to evaluate</param>
        /// <returns>The actual value of the expression</returns>
        private static object GetValueFromExpression(Expression expression)
        {
            if (expression is ConstantExpression constantExpression) return constantExpression.Value;
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        /// <summary>
        /// Determines whether an expression can be evaluated as a constant value (doesn't contain parameter references).
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <returns>True if the expression can be evaluated as a value, false if it contains parameter references</returns>
        private static bool CanBeEvaluatedAsValue(Expression expression)
        {
            return !new ParameterVisitor().ContainsParameter(expression);
        }

        /// <summary>
        /// Formats a .NET value into its SQL string representation with proper quoting and null handling.
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>A string representation suitable for use in SQL queries</returns>
        private static string FormatValue(object value)
        {
            if (value == null) return "NULL";
            if (value is decimal || value is double || value is float) return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value is string || value is DateTime || value is Guid) return $"'{value}'";
            if (value is bool boolValue) return boolValue ? "1" : "0";
            return value.ToString();
        }

        /// <summary>
        /// Converts a LINQ expression type to its corresponding SQL operator string.
        /// </summary>
        /// <param name="type">The expression type to convert</param>
        /// <returns>The SQL operator string</returns>
        /// <exception cref="NotSupportedException">Thrown when the expression type is not supported</exception>
        private static string GetSqlOperator(ExpressionType type) => type switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            _ => throw new NotSupportedException($"Unsupported operator: {type}"),
        };

        /// <summary>
        /// Flips comparison operators when operands are swapped in binary expressions.
        /// </summary>
        /// <param name="op">The original operator to flip</param>
        /// <returns>The flipped operator, or the original if no flipping is needed</returns>
        private static string FlipOperator(string op) => op switch
        {
            ">" => "<",
            ">=" => "<=",
            "<" => ">",
            "<=" => ">=",
            _ => op
        };

        /// <summary>
        /// Expression visitor that checks whether an expression tree contains any parameter expressions.
        /// </summary>
        private class ParameterVisitor : ExpressionVisitor
        {
            private bool _containsParameter;
            
            /// <summary>
            /// Checks if the given expression node contains any parameter expressions.
            /// </summary>
            /// <param name="node">The expression to check</param>
            /// <returns>True if the expression contains parameter references, false otherwise</returns>
            public bool ContainsParameter(Expression node)
            {
                _containsParameter = false;
                Visit(node);
                return _containsParameter;
            }
            
            /// <summary>
            /// Visits parameter expressions and sets the flag indicating parameter presence.
            /// </summary>
            /// <param name="node">The parameter expression node</param>
            /// <returns>The visited expression</returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                _containsParameter = true;
                return base.VisitParameter(node);
            }
        }
        #endregion
    }
}