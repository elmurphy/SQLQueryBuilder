using SQLQueryBuilder.Flags;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SQLQueryBuilder.Core
{
    public class SQLQueryBuilder<T>
        where T : class, new()
    {
        internal Type MainEntityType { get; set; }
        internal QueryType queryType { get; set; }
        internal string TableName { get; set; }
        internal List<SQBProperty> SQBProperties { get; set; }
        internal List<SQBSqlWhere> whereConditions { get; set; }
        internal List<SQBInclude> includes { get; set; }
        internal List<string> tableIndexes { get; set; }
        internal List<(string, string)> ColumnAndAlias { get; set; }
        internal long? TakeCount { get; set; }
        internal long? SkipCount { get; set; }
        internal bool IsDistinct { get; set; }
        internal bool IsForUpdate { get; set; }

        internal bool? Asc { get; set; }
        internal Expression<Func<T, object>> OrderByExpression { get; set; }

        internal Expression<Func<T, object>> GroupByExpression { get; set; }


        internal SQLQueryBuilder()
        {
            SQBProperties = new List<SQBProperty>();
            whereConditions = new List<SQBSqlWhere>();
            includes = new List<SQBInclude>();
            tableIndexes = new List<string>();
            ColumnAndAlias = new List<(string, string)>();
        }

        public ISQBLImplementation sqlImplementation(DatabaseType databaseType = DatabaseType.MSSQL)
        {
            switch (databaseType)
            {
                case DatabaseType.MSSQL:
                    return new SQBMSSQLImplementation();
                case DatabaseType.PostgreSQL:
                    throw new NotImplementedException("PostgreSQL implementation is not yet available.");
                default:
                    throw new InvalidOperationException("Unsupported database type.");
            }
        }

        /// <summary>
        /// Build query string based on the provided configuration. (It's not required, you might want to use GetSingleAsync/GetListAsync)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string BuildQuery()
        {
            if (MainEntityType == null)
            {
                throw new InvalidOperationException("Main entity type was not set for the query builder.");
            }

            var queryBuilder = new StringBuilder();
            int tableAliasCounter = 0;

            switch (queryType)
            {
                case QueryType.Select:
                    BuildSelectQuery(this, queryBuilder, tableAliasCounter);
                    break;

                default:
                    throw new InvalidOperationException("Unsupported query type.");
            }
            return queryBuilder.ToString();
        }

        /// <summary>
        /// Takes the type of the main entity to be queried.
        /// </summary>
        /// <param name="take"></param>
        /// <returns></returns>
        public SQLQueryBuilder<T> Take(long take) { this.TakeCount = take; return this; }
        /// <summary>
        /// Skips the specified number of records.
        /// </summary>
        /// <param name="skip"></param>
        /// <returns></returns>
        public SQLQueryBuilder<T> Skip(long skip) { this.SkipCount = skip; return this; }
        /// <summary>
        /// Groups the query results by the specified column.
        /// </summary>
        /// <param name="groupBy"></param>
        /// <returns></returns>
        public SQLQueryBuilder<T> GroupBy(Expression<Func<T, object>> groupBy) { this.GroupByExpression = groupBy; return this; }
        /// <summary>
        /// Sorts the query results by the specified column in ascending or descending order.
        /// </summary>
        /// <param name="ascending"></param>
        /// <returns></returns>
        public SQLQueryBuilder<T> OrderByAscending(Expression<Func<T, object>> ascending) { this.OrderByExpression = ascending; this.Asc = true; return this; }
        /// <summary>
        /// Sorts the query results by the specified column in descending order.
        /// </summary>
        /// <param name="descending"></param>
        /// <returns></returns>
        public SQLQueryBuilder<T> OrderByDescending(Expression<Func<T, object>> descending) { this.OrderByExpression = descending; this.Asc = false; return this; }

        /// <summary>
        /// Page query results.
        /// </summary>
        /// <param name="pageIndex">Must be greater than -1</param>
        /// <param name="pageCount">Must be positive</param>
        /// <returns></returns>
        public SQLQueryBuilder<T> Page([Range(0, long.MaxValue)] long pageIndex, [Range(1, long.MaxValue)] long pageCount) { this.Take(pageCount); this.Skip(pageCount * (pageIndex + 1)); return this; }

        internal SQLQueryBuilder<T> SetQueryType(QueryType queryType)
        {
            this.queryType = queryType;
            return this;
        }
        internal SQLQueryBuilder<T> SetTableName()
        {
            Type type = typeof(T);
            SQBTableAttribute? tableAttribute = type.GetCustomAttribute<SQBTableAttribute>();
            var tableName = tableAttribute != null ? tableAttribute.TableName : type.Name;
            this.TableName = tableName;
            return this;
        }
        internal SQLQueryBuilder<T> SetProperties()
        {
            Type type = typeof(T);
            List<PropertyInfo> properties = type.GetProperties()
                .ToList();
            foreach (var property in properties)
            {
                string propertyName = property.GetCustomAttribute<SQBPropertyAttribute>()?.SqlFieldName ?? property.Name;
                Type propertyType = property.PropertyType;
                SQBProperty sqbProperty = new SQBProperty
                {
                    Name = propertyName,
                    Type = this.sqlImplementation().GetSqlType(propertyType)
                };
                this.SQBProperties.Add(sqbProperty);
            }
            return this;
        }

        internal static string GetLeftJoinSql(string joinedTableName, string joinedTableAlias, string mainTableAlias, string fkColumnName, string joinedPrimaryKey)
        {
            return $" LEFT JOIN [{joinedTableName}] AS [{joinedTableAlias}] ON [{mainTableAlias}].[{fkColumnName}] = [{joinedTableAlias}].[{joinedPrimaryKey!}]";
        }

        internal static string GetJoinedTableName(Type joinedType, SQBTableAttribute? joinedTableAttr)
        {
            return joinedTableAttr?.TableName ?? joinedType.Name;
        }

        internal static SQBTableAttribute? GetJoinedTableAttr(Type joinedType)
        {
            return (SQBTableAttribute)Attribute.GetCustomAttribute(joinedType, typeof(SQBTableAttribute));
        }

        internal static Type GetJoinedType(object foreignKeyAttr)
        {
            return foreignKeyAttr.GetType().GetGenericArguments()[0];
        }

        internal static object? GetForeignKeyAttr(PropertyInfo foreignKeyProp)
        {
            return foreignKeyProp.GetCustomAttributes(false)
                                    .FirstOrDefault(attr => attr.GetType().IsGenericType && attr.GetType().GetGenericTypeDefinition() == typeof(SQBForeignKeyAttribute<>));
        }

        internal static void BuildSelectQuery(SQLQueryBuilder<T> builder, StringBuilder stringBuilder, int tableAliasCounter)
        {
            string mainTableAlias = $"e{tableAliasCounter}";
            var columnsBuilder = new StringBuilder();

            builder.ColumnAndAlias = AppendColumnsWithAlias(builder, columnsBuilder, builder.MainEntityType, mainTableAlias);

            string joinClause = BuildJoinClause(builder, ref tableAliasCounter, mainTableAlias, columnsBuilder);

            var selectTemplate = builder.sqlImplementation().SelectQueryTemplate();
            stringBuilder.AppendFormat(selectTemplate, columnsBuilder.ToString(), $"[{builder.TableName}] AS [{mainTableAlias}]");
            stringBuilder.Append(joinClause);

            if (builder.whereConditions != null && builder.whereConditions.Any())
            {
                string conditions = GetConditions(builder, mainTableAlias);

                var whereTemplate = builder.sqlImplementation().WhereClauseTemplate();
                stringBuilder.Append(" ");
                stringBuilder.AppendFormat(whereTemplate, conditions);
            }
            if (builder.GroupByExpression != null)
            {
                string groupByColumn = builder.GroupByExpression.Body.ToString().Split('.').Last();
                var groupByTemplate = builder.sqlImplementation().GroupByClauseTemplate();
                stringBuilder.Append(" ");
                stringBuilder.AppendFormat(groupByTemplate, $"[{mainTableAlias}].[{groupByColumn}]");
            }

            if (builder.OrderByExpression != null)
            {
                string orderByColumn = builder.OrderByExpression.Body.ToString().Split('.').Last();
                string orderByDirection = builder.Asc.HasValue && builder.Asc.Value ? "ASC" : "DESC";
                var orderByTemplate = builder.sqlImplementation().OrderByClauseTemplate();
                stringBuilder.Append(" ");
                stringBuilder.AppendFormat(orderByTemplate, $"[{mainTableAlias}].[{orderByColumn}] {orderByDirection}");
            }

            if (builder.TakeCount.HasValue)
            {
                stringBuilder.Append($"OFFSET {builder.TakeCount.Value} ROWS");
                stringBuilder.Append($" FETCH NEXT {builder.SkipCount ?? 0} ROWS ONLY");
            }

            stringBuilder.Append(";");
        }

        internal static string GetConditions(SQLQueryBuilder<T> builder, string mainTableAlias)
        {
            return string.Join(" AND ", builder.whereConditions.Select(x =>
            {
                string aliasedColumn = $"[{mainTableAlias}].[{x.ColumnName}]";
                string fieldPart = string.Format(x.FunctionTemplate, aliasedColumn);
                return $"{fieldPart} {x.Operation} {x.Value}";
            }));
        }

        internal static string BuildJoinClause(SQLQueryBuilder<T> builder, ref int tableAliasCounter, string mainTableAlias, StringBuilder columnsBuilder)
        {
            var joinsBuilder = new StringBuilder();
            var aliasToTypeMap = new Dictionary<string, Type> { { mainTableAlias, builder.MainEntityType } };
            var nameToAliasMap = new Dictionary<string, string> { { builder.TableName, mainTableAlias } };

            if (builder.includes != null && builder.includes.Any())
            {
                foreach (var include in builder.includes)
                {
                    tableAliasCounter++;
                    string joinedTableAlias = $"e{tableAliasCounter}";

                    if (!nameToAliasMap.TryGetValue(include.TableFrom, out var fromTableAlias))
                    {
                        throw new InvalidOperationException($"Cannot join from table '{include.TableFrom}' as it has not been included in the query. Ensure your Include/ThenInclude calls are in the correct order.");
                    }
                    var fromType = aliasToTypeMap[fromTableAlias];

                    var foreignKeyProp = fromType.GetProperty(include.FieldName);
                    if (foreignKeyProp == null)
                    {
                        throw new InvalidOperationException($"Property '{include.FieldName}' not found on type '{fromType.Name}'.");
                    }

                    var foreignKeyAttr = GetForeignKeyAttr(foreignKeyProp);
                    if (foreignKeyAttr == null)
                    {
                        throw new InvalidOperationException($"Property '{include.FieldName}' on type '{fromType.Name}' does not have an SQBForeignKey attribute.");
                    }

                    var joinedType = GetJoinedType(foreignKeyAttr);

                    AppendColumnsWithAlias(builder, columnsBuilder, joinedType, joinedTableAlias);

                    var joinedTableAttr = GetJoinedTableAttr(joinedType);
                    var joinedTableName = GetJoinedTableName(joinedType, joinedTableAttr);
                    var joinedPrimaryKey = joinedType.GetProperties().FirstOrDefault(p => p.GetCustomAttribute<SQBPrimaryKeyAttribute>() != null)?.Name;

                    var fkColumnAttr = foreignKeyProp.GetCustomAttribute<SQBPropertyAttribute>();
                    var fkColumnName = fkColumnAttr?.SqlFieldName ?? foreignKeyProp.Name;

                    joinsBuilder.Append(GetLeftJoinSql(joinedTableName, joinedTableAlias, fromTableAlias, fkColumnName, joinedPrimaryKey));

                    aliasToTypeMap.Add(joinedTableAlias, joinedType);
                    nameToAliasMap[joinedTableName] = joinedTableAlias;
                }
            }

            return joinsBuilder.ToString();
        }

        internal static List<(string, string)> AppendColumnsWithAlias(SQLQueryBuilder<T> builder, StringBuilder columnsBuilder, Type entityType, string tableAlias)
        {
            List<(string, string)> ColumnAndAlias = new List<(string, string)>();
            var properties = entityType.GetProperties();
            foreach (var prop in properties)
            {
                var columnAttr = prop.GetCustomAttribute<SQBPropertyAttribute>();
                var columnName = columnAttr?.SqlFieldName ?? prop.Name;

                if (columnsBuilder.Length > 0)
                {
                    columnsBuilder.Append(", ");
                }
                columnsBuilder.Append($"[{tableAlias}].[{columnName}] AS [{tableAlias}_{prop.Name}]");
                ColumnAndAlias.Add((columnName, $"{tableAlias}_{prop.Name}"));
            }
            return ColumnAndAlias;
        }
    }
}