using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLQueryBuilder.Core
{
    public record ParsedMember(string TableAlias, string ColumnName);

    public static class ExpressionParser
    {
        public static SQBSqlWhere Parse<T>(SQLQueryBuilder<T> builder, Expression expression) where T : class, new()
        {
            var visitor = new AliasExpressionVisitor(builder);
            return visitor.Translate(expression);
        }

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

        #region Helper Metotları
        private static object GetValueFromExpression(Expression expression)
        {
            if (expression is ConstantExpression constantExpression) return constantExpression.Value;
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        private static bool CanBeEvaluatedAsValue(Expression expression)
        {
            return !new ParameterVisitor().ContainsParameter(expression);
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "NULL";
            if (value is decimal || value is double || value is float) return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value is string || value is DateTime || value is Guid) return $"'{value}'";
            if (value is bool boolValue) return boolValue ? "1" : "0";
            return value.ToString();
        }

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

        private static string FlipOperator(string op) => op switch
        {
            ">" => "<",
            ">=" => "<=",
            "<" => ">",
            "<=" => ">=",
            _ => op
        };

        private class ParameterVisitor : ExpressionVisitor
        {
            private bool _containsParameter;
            public bool ContainsParameter(Expression node)
            {
                _containsParameter = false;
                Visit(node);
                return _containsParameter;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                _containsParameter = true;
                return base.VisitParameter(node);
            }
        }
        #endregion
    }
}