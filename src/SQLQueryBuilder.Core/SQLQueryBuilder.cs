using SQLQueryBuilder.Flags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SQLQueryBuilder.Core
{
    public class SQLQueryBuilder<T> where T : class, new()
    {
        internal Type MainEntityType { get; set; }
        internal QueryType queryType { get; set; }
        internal string TableName { get; set; }
        internal List<Expression> WhereExpressions { get; set; }
        internal List<SQBInclude> includes { get; set; }
        internal List<(string PropertyName, string Alias)> ColumnAndAlias { get; set; }
        internal long? TakeCount { get; set; }
        internal long? SkipCount { get; set; }
        internal List<(LambdaExpression Expression, bool Ascending)> OrderByClauses { get; set; }
        internal LambdaExpression GroupByExpression { get; set; }

        internal string mainTableAlias;
        internal readonly Dictionary<Type, string> typeToAliasMap = new();
        internal int tableAliasCounter = 0;

        internal SQLQueryBuilder()
        {
            WhereExpressions = new List<Expression>();
            includes = new List<SQBInclude>();
            ColumnAndAlias = new List<(string, string)>();
            OrderByClauses = new List<(LambdaExpression, bool)>();
        }

        public SQLQueryBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression != null)
            {
                WhereExpressions.Add(expression.Body);
            }
            return this;
        }

        public string BuildQuery()
        {
            if (MainEntityType == null) throw new InvalidOperationException("Main entity type was not set.");

            var queryBuilder = new StringBuilder();
            mainTableAlias = $"e{tableAliasCounter}";
            typeToAliasMap.Clear();
            typeToAliasMap[MainEntityType] = mainTableAlias;

            BuildSelectQuery(this, queryBuilder);

            return queryBuilder.ToString();
        }

        #region Builder Methods
        public SQLQueryBuilder<T> Take(long take) { this.TakeCount = take; return this; }
        public SQLQueryBuilder<T> Skip(long skip) { this.SkipCount = skip; return this; }
        public SQLQueryBuilder<T> GroupBy(Expression<Func<T, object>> groupBy) { this.GroupByExpression = groupBy; return this; }
        public SQLQueryBuilder<T> OrderByAscending(Expression<Func<T, object>> ascending) { this.OrderByClauses.Add((ascending, true)); return this; }
        public SQLQueryBuilder<T> OrderByDescending(Expression<Func<T, object>> descending) { this.OrderByClauses.Add((descending, false)); return this; }
        public SQLQueryBuilder<T> ThenBy(Expression<Func<T, object>> thenBy) { this.OrderByClauses.Add((thenBy, true)); return this; }
        public SQLQueryBuilder<T> ThenByDescending(Expression<Func<T, object>> thenByDescending) { this.OrderByClauses.Add((thenByDescending, false)); return this; }
        public SQLQueryBuilder<T> Page(long pageIndex, long pageCount) { this.Take(pageCount); this.Skip(pageCount * pageIndex); return this; }
        internal SQLQueryBuilder<T> SetQueryType(QueryType qt) { this.queryType = qt; return this; }
        internal SQLQueryBuilder<T> SetTableName()
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<SQBTableAttribute>();
            this.TableName = tableAttr?.TableName ?? type.Name;
            return this;
        }
        #endregion

        private static void BuildSelectQuery(SQLQueryBuilder<T> builder, StringBuilder stringBuilder)
        {
            var columnsBuilder = new StringBuilder();

            // First add main table columns
            builder.ColumnAndAlias.AddRange(AppendColumnsWithAlias(columnsBuilder, builder.MainEntityType, builder.mainTableAlias));

            // Then build join clause and add joined table columns
            string joinClause = BuildJoinClause(builder, columnsBuilder);

            stringBuilder.Append("SELECT");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"    {columnsBuilder}");
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append($"FROM [{builder.TableName}] AS [{builder.mainTableAlias}]");
            stringBuilder.Append(joinClause);

            if (builder.WhereExpressions.Any())
            {
                var whereConditions = builder.WhereExpressions.Select(expr => ExpressionParser.Parse(builder, expr)).ToList();
                var combinedCondition = whereConditions.Count == 1
                    ? whereConditions.First()
                    : new SQBSqlWhere { Operator = LogicalOperator.AND, NestedConditions = whereConditions };

                string conditions = BuildWhereClauseRecursive(combinedCondition, true);
                if (!string.IsNullOrEmpty(conditions))
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append($"WHERE {conditions}");
                }
            }

            if (builder.GroupByExpression != null)
            {
                var body = builder.GroupByExpression.Body;
                var parsedMembers = ExpressionParser.ParseMembers(builder, body);
                var groupByColumns = parsedMembers.Select(m => $"[{m.TableAlias}].[{m.ColumnName}]");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append($"GROUP BY {string.Join(", ", groupByColumns)}");
            }

            if (builder.OrderByClauses.Any())
            {
                var orderByParts = new List<string>();
                foreach (var (expression, ascending) in builder.OrderByClauses)
                {
                    var parsedMembers = ExpressionParser.ParseMembers(builder, expression.Body);
                    string dir = ascending ? "ASC" : "DESC";
                    orderByParts.AddRange(parsedMembers.Select(m => $"[{m.TableAlias}].[{m.ColumnName}] {dir}"));
                }
                if (orderByParts.Any())
                {
                    stringBuilder.Append(Environment.NewLine);
                    stringBuilder.Append($"ORDER BY {string.Join(", ", orderByParts)}");
                }
            }

            if (builder.SkipCount.HasValue || builder.TakeCount.HasValue)
            {
                if (!builder.OrderByClauses.Any()) throw new InvalidOperationException("SKIP or TAKE requires an ORDER BY clause.");
                stringBuilder.Append(Environment.NewLine);
                stringBuilder.Append($"OFFSET {builder.SkipCount ?? 0} ROWS");
                if (builder.TakeCount.HasValue)
                {
                    stringBuilder.Append($" FETCH NEXT {builder.TakeCount.Value} ROWS ONLY");
                }
            }
            stringBuilder.Append(";");
        }

        private static string BuildWhereClauseRecursive(SQBSqlWhere condition, bool isTopLevel = false)
        {
            if (condition == null) return string.Empty;

            if (!condition.IsGroup)
            {
                return $"[{condition.TableAlias}].[{condition.ColumnName}] {condition.Operation} {condition.Value}";
            }
            else
            {
                var clauses = condition.NestedConditions.Select(c => BuildWhereClauseRecursive(c, false)).Where(s => !string.IsNullOrEmpty(s));
                if (!clauses.Any()) return string.Empty;

                var joined = string.Join($" {condition.Operator} ", clauses);
                if (isTopLevel && clauses.Count() == 1)
                {
                    return joined;
                }
                return $"({joined})";
            }
        }

        private static string BuildJoinClause(SQLQueryBuilder<T> builder, StringBuilder columnsBuilder)
        {
            var joinsBuilder = new StringBuilder();

            foreach (var include in builder.includes)
            {
                builder.tableAliasCounter++;
                string joinedTableAlias = $"e{builder.tableAliasCounter}";

                if (!builder.typeToAliasMap.TryGetValue(include.TypeFrom, out var fromTableAlias))
                {
                    throw new InvalidOperationException($"Cannot join from type '{include.TypeFrom.Name}'. Ensure it was included before.");
                }

                var fkPropInfo = include.TypeFrom.GetProperty(include.FieldName);
                var fkAttr = GetForeignKeyAttr(fkPropInfo);
                var joinedType = GetJoinedType(fkAttr);

                if (!builder.typeToAliasMap.ContainsKey(joinedType))
                {
                    builder.typeToAliasMap[joinedType] = joinedTableAlias;
                }
                else // ThenInclude ile aynı tip tekrar join'leniyorsa, yeni alias'ı ata
                {
                    builder.typeToAliasMap[joinedType] = joinedTableAlias;
                }

                builder.ColumnAndAlias.AddRange(AppendColumnsWithAlias(columnsBuilder, joinedType, joinedTableAlias));

                var joinedTableAttr = joinedType.GetCustomAttribute<SQBTableAttribute>();
                var joinedTableName = joinedTableAttr?.TableName ?? joinedType.Name;
                var joinedPk = joinedType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<SQBPrimaryKeyAttribute>() != null)?.Name ?? "Id";

                joinsBuilder.Append(Environment.NewLine);
                joinsBuilder.Append($"LEFT JOIN [{joinedTableName}] AS [{joinedTableAlias}] ON [{fromTableAlias}].[{fkPropInfo.Name}] = [{joinedTableAlias}].[{joinedPk}]");
            }
            return joinsBuilder.ToString();
        }

        private static List<(string, string)> AppendColumnsWithAlias(StringBuilder columnsBuilder, Type entityType, string tableAlias)
        {
            var aliases = new List<(string, string)>();
            var properties = entityType.GetProperties()
                .Where(p => p.PropertyType.IsPrimitive || p.PropertyType.IsValueType || p.PropertyType == typeof(string));

            foreach (var prop in properties)
            {
                if (columnsBuilder.Length > 0)
                {
                    columnsBuilder.Append(", ");
                }
                var aliasName = $"{tableAlias}_{prop.Name}";
                columnsBuilder.Append($"[{tableAlias}].[{prop.Name}] AS [{aliasName}]");
                aliases.Add((prop.Name, aliasName));
            }
            return aliases;
        }

        private static object GetForeignKeyAttr(PropertyInfo prop) => prop.GetCustomAttributes(false).FirstOrDefault(attr => attr.GetType().IsGenericType && attr.GetType().GetGenericTypeDefinition() == typeof(SQBForeignKeyAttribute<>));
        private static Type GetJoinedType(object attr)
        {
            if (attr == null)
                throw new InvalidOperationException($"Property must have SQBForeignKeyAttribute to be used in joins.");
            return attr.GetType().GetGenericArguments()[0];
        }
    }
}