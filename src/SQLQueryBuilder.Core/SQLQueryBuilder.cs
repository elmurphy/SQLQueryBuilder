using SQLQueryBuilder.Flags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// A fluent SQL query builder for generating SELECT statements with support for WHERE, JOIN, ORDER BY, GROUP BY, and pagination.
    /// </summary>
    /// <typeparam name="T">The main entity type for the query</typeparam>
    public class SQLQueryBuilder<T> where T : class, new()
    {
        /// <summary>
        /// Gets or sets the main entity type for the query.
        /// </summary>
        internal Type MainEntityType { get; set; }
        
        /// <summary>
        /// Gets or sets the type of SQL query being built (Select, Insert, Update, Delete).
        /// </summary>
        internal QueryType queryType { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the main table for the query.
        /// </summary>
        internal string TableName { get; set; }
        
        /// <summary>
        /// Gets or sets the list of WHERE condition expressions.
        /// </summary>
        internal List<Expression> WhereExpressions { get; set; }
        
        /// <summary>
        /// Gets or sets the list of tables to include via LEFT JOIN operations.
        /// </summary>
        internal List<SQBInclude> includes { get; set; }
        
        /// <summary>
        /// Gets or sets the list of column names and their corresponding aliases in the SELECT clause.
        /// </summary>
        internal List<(string PropertyName, string Alias)> ColumnAndAlias { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum number of rows to return (FETCH NEXT clause).
        /// </summary>
        internal long? TakeCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of rows to skip (OFFSET clause).
        /// </summary>
        internal long? SkipCount { get; set; }
        
        /// <summary>
        /// Gets or sets the list of ORDER BY clauses with their sort direction.
        /// </summary>
        internal List<(LambdaExpression Expression, bool Ascending)> OrderByClauses { get; set; }
        
        /// <summary>
        /// Gets or sets the GROUP BY expression.
        /// </summary>
        internal LambdaExpression GroupByExpression { get; set; }

        /// <summary>
        /// The alias used for the main table in the query.
        /// </summary>
        internal string mainTableAlias;
        
        /// <summary>
        /// Dictionary mapping entity types to their corresponding table aliases.
        /// </summary>
        internal readonly Dictionary<Type, string> typeToAliasMap = new();
        
        /// <summary>
        /// Counter for generating unique table aliases (e0, e1, e2, etc.).
        /// </summary>
        internal int tableAliasCounter = 0;

        /// <summary>
        /// Initializes a new instance of the SQLQueryBuilder class.
        /// </summary>
        internal SQLQueryBuilder()
        {
            WhereExpressions = new List<Expression>();
            includes = new List<SQBInclude>();
            ColumnAndAlias = new List<(string, string)>();
            OrderByClauses = new List<(LambdaExpression, bool)>();
        }

        /// <summary>
        /// Adds a WHERE condition to the query using a lambda expression.
        /// </summary>
        /// <param name="expression">Lambda expression that defines the WHERE condition</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression != null)
            {
                WhereExpressions.Add(expression.Body);
            }
            return this;
        }

        /// <summary>
        /// Builds and returns the complete SQL query string based on the configured conditions, joins, and clauses.
        /// </summary>
        /// <returns>A complete SQL SELECT statement as a string</returns>
        /// <exception cref="InvalidOperationException">Thrown when the main entity type is not set</exception>
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
        /// <summary>
        /// Limits the number of rows returned by the query (equivalent to FETCH NEXT clause).
        /// </summary>
        /// <param name="take">Maximum number of rows to return</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> Take(long take) { this.TakeCount = take; return this; }
        
        /// <summary>
        /// Skips the specified number of rows before returning results (equivalent to OFFSET clause).
        /// </summary>
        /// <param name="skip">Number of rows to skip</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> Skip(long skip) { this.SkipCount = skip; return this; }
        
        /// <summary>
        /// Adds a GROUP BY clause to the query using the specified expression.
        /// </summary>
        /// <param name="groupBy">Expression that selects the properties to group by</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> GroupBy(Expression<Func<T, object>> groupBy) { this.GroupByExpression = groupBy; return this; }
        
        /// <summary>
        /// Adds an ORDER BY clause with ascending sort direction.
        /// </summary>
        /// <param name="ascending">Expression that selects the property to sort by</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> OrderByAscending(Expression<Func<T, object>> ascending) { this.OrderByClauses.Add((ascending, true)); return this; }
        
        /// <summary>
        /// Adds an ORDER BY clause with descending sort direction.
        /// </summary>
        /// <param name="descending">Expression that selects the property to sort by</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> OrderByDescending(Expression<Func<T, object>> descending) { this.OrderByClauses.Add((descending, false)); return this; }
        
        /// <summary>
        /// Adds an additional ORDER BY clause with ascending sort direction (used after OrderByAscending or OrderByDescending).
        /// </summary>
        /// <param name="thenBy">Expression that selects the property to sort by</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> ThenBy(Expression<Func<T, object>> thenBy) { this.OrderByClauses.Add((thenBy, true)); return this; }
        
        /// <summary>
        /// Adds an additional ORDER BY clause with descending sort direction (used after OrderByAscending or OrderByDescending).
        /// </summary>
        /// <param name="thenByDescending">Expression that selects the property to sort by</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> ThenByDescending(Expression<Func<T, object>> thenByDescending) { this.OrderByClauses.Add((thenByDescending, false)); return this; }
        
        /// <summary>
        /// Implements pagination by setting both Skip and Take values based on page index and page size.
        /// </summary>
        /// <param name="pageIndex">Zero-based page index</param>
        /// <param name="pageCount">Number of items per page</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public SQLQueryBuilder<T> Page(long pageIndex, long pageCount) { this.Take(pageCount); this.Skip(pageCount * pageIndex); return this; }
        
        /// <summary>
        /// Sets the type of SQL query to be built (internal use only).
        /// </summary>
        /// <param name="qt">The query type to set</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        internal SQLQueryBuilder<T> SetQueryType(QueryType qt) { this.queryType = qt; return this; }
        
        /// <summary>
        /// Sets the table name based on the entity type and SQBTableAttribute if present (internal use only).
        /// </summary>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
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
                else // If the same type is joined again via ThenInclude, assign new alias
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