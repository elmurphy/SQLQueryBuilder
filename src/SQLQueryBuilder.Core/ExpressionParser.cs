using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLQueryBuilder.Core
{
    public static class ExpressionParser
    {
        public static SQBSqlWhere Parse(Expression expression)
        {
            return ParseExpression(expression);
        }

        private static SQBSqlWhere ParseExpression(Expression expression)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                // AND veya OR ise, bu bir grup koşuludur
                if (binaryExpression.NodeType == ExpressionType.AndAlso || binaryExpression.NodeType == ExpressionType.OrElse)
                {
                    var group = new SQBSqlWhere
                    {
                        Operator = binaryExpression.NodeType == ExpressionType.AndAlso ? LogicalOperator.AND : LogicalOperator.OR
                    };
                    group.NestedConditions.Add(ParseExpression(binaryExpression.Left));
                    group.NestedConditions.Add(ParseExpression(binaryExpression.Right));
                    return group;
                }
                // Diğer operatörler (=, >, <, vb.) basit bir koşuldur
                return ParseSimpleBinaryExpression(binaryExpression);
            }

            if (expression is MethodCallExpression methodCall)
            {
                return ParseMethodCallExpression(methodCall);
            }

            if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Not)
            {
                if (unaryExpression.Operand is MemberExpression memberForUnary)
                {
                    var fieldParts = ParseFieldSide(memberForUnary);
                    return new SQBSqlWhere { ColumnName = fieldParts.ColumnName, Operation = "=", Value = "0" };
                }
            }

            if (expression is MemberExpression member)
            {
                var fieldParts = ParseFieldSide(member);
                return new SQBSqlWhere { ColumnName = fieldParts.ColumnName, Operation = "=", Value = "1" };
            }

            throw new NotSupportedException($"Unsupported expression type: {expression.NodeType}");
        }

        private static SQBSqlWhere ParseSimpleBinaryExpression(BinaryExpression binaryExpression)
        {
            Expression fieldSide = binaryExpression.Left;
            Expression valueSide = binaryExpression.Right;
            string op = GetSqlOperator(binaryExpression.NodeType);

            if (CanBeEvaluatedAsValue(fieldSide))
            {
                (fieldSide, valueSide) = (valueSide, fieldSide);
                op = FlipOperator(op);
            }

            var fieldParts = ParseFieldSide(fieldSide);
            var valueObject = GetValueFromExpression(valueSide);

            return new SQBSqlWhere
            {
                ColumnName = fieldParts.ColumnName,
                FunctionTemplate = fieldParts.Template,
                Operation = op,
                Value = FormatValue(valueObject)
            };
        }

        private static SQBSqlWhere ParseMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Object is not MemberExpression memberExp)
                throw new NotSupportedException("Method call must be on a member property.");

            var columnName = memberExp.Member.Name;
            var value = GetValueFromExpression(expression.Arguments[0]);
            var formattedValue = FormatValue(value);

            return expression.Method.Name switch
            {
                "Contains" => new SQBSqlWhere { ColumnName = columnName, Operation = "LIKE", Value = $"'%{value}%'" },
                "StartsWith" => new SQBSqlWhere { ColumnName = columnName, Operation = "LIKE", Value = $"'{value}%'" },
                "EndsWith" => new SQBSqlWhere { ColumnName = columnName, Operation = "LIKE", Value = $"'%{value}'" },
                _ => throw new NotSupportedException($"Unsupported string method: {expression.Method.Name}")
            };
        }

        private static (string ColumnName, string Template) ParseFieldSide(Expression expression)
        {
            if (expression is MemberExpression memberExp)
            {
                if (memberExp.Member is PropertyInfo prop && prop.Name == "Length" && memberExp.Expression is MemberExpression parentMember)
                {
                    return (parentMember.Member.Name, "LEN({0})");
                }
                return (memberExp.Member.Name, "{0}");
            }
            throw new NotSupportedException($"Unsupported expression for a database field: {expression}");
        }

        private static string FormatValue(object value)
        {
            if (value == null) return "NULL";
            if (value is decimal || value is double || value is float) return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value is string || value is DateTime || value is Guid) return $"'{value}'";
            if (value is bool boolValue) return boolValue ? "1" : "0";
            return value.ToString();
        }

        #region Helper Metotları
        private static object GetValueFromExpression(Expression expression)
        {
            if (expression is ConstantExpression constantExpression) return constantExpression.Value;
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        private static bool CanBeEvaluatedAsValue(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant) return true;
            var visitor = new ParameterVisitor();
            visitor.Visit(expression);
            return !visitor.ContainsParameter;
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
            public bool ContainsParameter { get; private set; }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                ContainsParameter = true;
                return base.VisitParameter(node);
            }
        }
        #endregion
    }
}