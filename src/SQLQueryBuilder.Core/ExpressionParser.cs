using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLQueryBuilder.Core
{
    public static class ExpressionParser
    {
        public static List<SQBSqlWhere> Parse(Expression expression)
        {
            var conditions = new List<SQBSqlWhere>();
            VisitExpression(expression, conditions);
            return conditions;
        }

        private static void VisitExpression(Expression expression, List<SQBSqlWhere> conditions)
        {
            if (expression is BinaryExpression binaryExpression)
            {
                if (binaryExpression.NodeType == ExpressionType.AndAlso)
                {
                    VisitExpression(binaryExpression.Left, conditions);
                    VisitExpression(binaryExpression.Right, conditions);
                }
                else
                {
                    conditions.Add(ParseBinaryExpression(binaryExpression));
                }
            }
            else if (expression is MethodCallExpression methodCall)
            {
                conditions.Add(ParseMethodCallExpression(methodCall));
            }
            else if (expression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Not)
            {
                // `!x.IsActive` gibi ifadeleri `x.IsActive == false` olarak ele al
                if (unaryExpression.Operand is MemberExpression memberForUnary)
                {
                    conditions.Add(new SQBSqlWhere { ColumnName = memberForUnary.Member.Name, Operation = "=", Value = "0" });
                }
            }
            else if (expression is MemberExpression member)
            {
                // `x.IsActive` gibi ifadeleri `x.IsActive == true` olarak ele al
                conditions.Add(new SQBSqlWhere { ColumnName = member.Member.Name, Operation = "=", Value = "1" });
            }
            else
            {
                throw new NotSupportedException($"Unsupported expression type: {expression.NodeType}");
            }
        }

        private static SQBSqlWhere ParseMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Object is not MemberExpression memberExp)
                throw new NotSupportedException("Method call must be on a member property.");

            var columnName = memberExp.Member.Name;
            var value = GetValueFromExpression(expression.Arguments.First());

            return expression.Method.Name switch
            {
                "Contains" => new SQBSqlWhere { ColumnName = columnName, Operation = "LIKE", Value = $"'%{value}%'" },
                "StartsWith" => new SQBSqlWhere { ColumnName = columnName, Operation = "LIKE", Value = $"'{value}%'" },
                "EndsWith" => new SQBSqlWhere { ColumnName = columnName, Operation = "LIKE", Value = $"'%{value}'" },
                _ => throw new NotSupportedException($"Unsupported string method: {expression.Method.Name}")
            };
        }

        private static SQBSqlWhere ParseBinaryExpression(BinaryExpression binaryExpression)
        {
            Expression fieldSide = binaryExpression.Left;
            Expression valueSide = binaryExpression.Right;
            string op = GetSqlOperator(binaryExpression.NodeType);

            // Değer (sabit, değişken) sol taraftaysa, ifadeleri değiştir ve operatörü ters çevir.
            if (CanBeEvaluatedAsValue(fieldSide))
            {
                (fieldSide, valueSide) = (valueSide, fieldSide); // Değiştir
                op = FlipOperator(op);
            }

            var fieldParts = ParseFieldSide(fieldSide);
            var valueObject = GetValueFromExpression(valueSide);

            var sqlWhere = new SQBSqlWhere
            {
                ColumnName = fieldParts.ColumnName,
                FunctionTemplate = fieldParts.Template,
                Operation = op,
                Value = valueObject?.ToString() ?? "NULL"
            };

            if (valueObject is string || valueObject is DateTime || valueObject is Guid)
            {
                sqlWhere.Value = $"'{sqlWhere.Value}'";
            }

            return sqlWhere;
        }

        private static (string ColumnName, string Template) ParseFieldSide(Expression expression)
        {
            if (expression is MemberExpression memberExp)
            {
                // x.Name.Length durumunu yakala
                if (memberExp.Member is PropertyInfo && memberExp.Member.Name == "Length" && memberExp.Expression is MemberExpression parentMember)
                {
                    return (parentMember.Member.Name, "LEN({0})");
                }
                // x.Name gibi normal durum
                return (memberExp.Member.Name, "{0}");
            }
            throw new NotSupportedException($"Unsupported expression for a database field: {expression}");
        }

        private static bool CanBeEvaluatedAsValue(Expression expression)
        {
            // Eğer ifadenin ağacında bir parametre (örn: 'x') yoksa, bu C# tarafında hesaplanabilir bir değerdir.
            return !expression.ToString().Contains(((ParameterExpression)((dynamic)expression).Expression)?.Name + ".");
        }

        private static string FlipOperator(string op)
        {
            return op switch { ">" => "<", ">=" => "<=", "<" => ">", "<=" => ">=", _ => op };
        }

        private static object GetValueFromExpression(Expression expression)
        {
            if (expression is ConstantExpression constantExpression)
            {
                return constantExpression.Value;
            }
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        private static string GetSqlOperator(ExpressionType type)
        {
            return type switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                _ => throw new NotSupportedException($"Unsupported operator: {type}"),
            };
        }
    }
}