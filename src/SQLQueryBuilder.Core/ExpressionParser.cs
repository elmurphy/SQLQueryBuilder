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
                if (unaryExpression.Operand is MemberExpression memberForUnary)
                {
                    conditions.Add(new SQBSqlWhere { ColumnName = memberForUnary.Member.Name, Operation = "=", Value = "0" });
                }
            }
            else if (expression is MemberExpression member)
            {
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

            if (CanBeEvaluatedAsValue(fieldSide))
            {
                (fieldSide, valueSide) = (valueSide, fieldSide);
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
            else if (valueObject is bool boolValue)
            {
                sqlWhere.Value = boolValue ? "1" : "0";
            }

            return sqlWhere;
        }

        private static (string ColumnName, string Template) ParseFieldSide(Expression expression)
        {
            if (expression is MemberExpression memberExp)
            {
                if (memberExp.Member is PropertyInfo && memberExp.Member.Name == "Length" && memberExp.Expression is MemberExpression parentMember)
                {
                    return (parentMember.Member.Name, "LEN({0})");
                }
                return (memberExp.Member.Name, "{0}");
            }
            throw new NotSupportedException($"Unsupported expression for a database field: {expression}");
        }

        /// <summary>
        /// Bir ifadenin C# tarafında bir değere dönüştürülüp dönüştürülemeyeceğini güvenli bir şekilde kontrol eder.
        /// Eğer ifade 'x' parametresine bağlıysa, bu bir veritabanı kolonudur.
        /// </summary>
        private static bool CanBeEvaluatedAsValue(Expression expression)
        {
            // Sabitler her zaman C# değeridir.
            if (expression.NodeType == ExpressionType.Constant)
            {
                return true;
            }

            // İfade ağacını gezerek içinde 'x' gibi bir parametre var mı diye kontrol et.
            // Varsa, bu bir kolon ifadesidir. Yoksa, C# tarafında hesaplanabilir bir değerdir.
            var visitor = new ParameterVisitor();
            visitor.Visit(expression);
            return !visitor.ContainsParameter;
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

        /// <summary>
        /// Bir ifade ağacında ParameterExpression olup olmadığını tespit etmek için kullanılan yardımcı sınıf.
        /// </summary>
        private class ParameterVisitor : ExpressionVisitor
        {
            public bool ContainsParameter { get; private set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                ContainsParameter = true;
                return base.VisitParameter(node);
            }
        }
    }
}