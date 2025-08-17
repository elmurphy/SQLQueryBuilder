using Microsoft.Data.SqlClient;
using SQLQueryBuilder.Flags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Extension methods for SQLQueryBuilder to provide fluent API functionality for building SQL queries.
    /// </summary>
    public static class SQLQueryBuilderExtension
    {
        /// <summary>
        /// Includes a related entity in the query by creating a LEFT JOIN based on foreign key relationships.
        /// </summary>
        /// <typeparam name="T">The main entity type</typeparam>
        /// <typeparam name="V">The related entity type to include</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance</param>
        /// <param name="foreignKeySelector">Expression that selects the foreign key property</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public static SQLQueryBuilder<T> Include<T, V>(this SQLQueryBuilder<T> builder, Expression<Func<T, object>> foreignKeySelector)
            where T : class, new() where V : class, new()
        {
            var member = GetMemberExpression(foreignKeySelector.Body);
            builder.includes.Add(new SQBInclude
            {
                TypeFrom = typeof(T),
                FieldName = member.Member.Name
            });
            return builder;
        }

        /// <summary>
        /// Includes a related entity that is related to a previously included entity, creating a chain of JOINs.
        /// </summary>
        /// <typeparam name="T">The main entity type</typeparam>
        /// <typeparam name="V">The entity type that was previously included</typeparam>
        /// <typeparam name="K">The new entity type to include</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance</param>
        /// <param name="foreignKeySelector">Expression that selects the foreign key property from the previously included entity</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        /// <exception cref="InvalidOperationException">Thrown when ThenInclude is called before any Include method</exception>
        public static SQLQueryBuilder<T> ThenInclude<T, V, K>(this SQLQueryBuilder<T> builder, Expression<Func<V, object>> foreignKeySelector)
            where T : class, new() where V : class, new() where K : class, new()
        {
            if (!builder.includes.Any())
            {
                throw new InvalidOperationException("ThenInclude must be called after an Include method.");
            }
            var member = GetMemberExpression(foreignKeySelector.Body);
            builder.includes.Add(new SQBInclude
            {
                TypeFrom = typeof(V),
                FieldName = member.Member.Name,
            });
            return builder;
        }

        /// <summary>
        /// Adds ascending order by clause for a property from a joined table.
        /// </summary>
        /// <typeparam name="T">The main entity type</typeparam>
        /// <typeparam name="V">The joined entity type</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance</param>
        /// <param name="keySelector">Expression that selects the property to order by</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public static SQLQueryBuilder<T> IncludeOrderByAscending<T, V>(this SQLQueryBuilder<T> builder, Expression<Func<V, object>> keySelector)
            where T : class, new() where V : class, new()
        {
            builder.OrderByClauses.Add((keySelector, true));
            return builder;
        }

        /// <summary>
        /// Adds descending order by clause for a property from a joined table.
        /// </summary>
        /// <typeparam name="T">The main entity type</typeparam>
        /// <typeparam name="V">The joined entity type</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance</param>
        /// <param name="keySelector">Expression that selects the property to order by</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public static SQLQueryBuilder<T> IncludeOrderByDescending<T, V>(this SQLQueryBuilder<T> builder, Expression<Func<V, object>> keySelector)
            where T : class, new() where V : class, new()
        {
            builder.OrderByClauses.Add((keySelector, false));
            return builder;
        }

        /// <summary>
        /// Adds a WHERE condition for a property from a joined table.
        /// </summary>
        /// <typeparam name="T">The main entity type</typeparam>
        /// <typeparam name="V">The joined entity type</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance</param>
        /// <param name="whereExpression">Expression that defines the WHERE condition</param>
        /// <returns>The SQLQueryBuilder instance for method chaining</returns>
        public static SQLQueryBuilder<T> IncludeWhere<T, V>(this SQLQueryBuilder<T> builder, Expression<Func<V, bool>> whereExpression)
            where T : class, new() where V : class, new()
        {
            if (whereExpression != null)
            {
                builder.WhereExpressions.Add(whereExpression.Body);
            }
            return builder;
        }

        /// <summary>
        /// Constructs a new SQLQueryBuilder instance with the specified entity type and optional initial WHERE condition.
        /// </summary>
        /// <typeparam name="T">The entity type to build queries for</typeparam>
        /// <param name="expression">Optional initial WHERE condition expression</param>
        /// <returns>A new SQLQueryBuilder instance configured for SELECT queries</returns>
        public static SQLQueryBuilder<T> ConstructBuilder<T>(Expression<Func<T, bool>>? expression = null)
            where T : class, new()
        {
            var builder = new SQLQueryBuilder<T>();
            builder.SetQueryType(QueryType.Select);
            builder.SetTableName();
            builder.MainEntityType = typeof(T);

            if (expression != null)
            {
                builder.Where(expression);
            }

            return builder;
        }

        #region Helper
        /// <summary>
        /// Extracts a MemberExpression from various expression types, handling conversions and unary expressions.
        /// </summary>
        /// <param name="expression">The expression to extract the member from</param>
        /// <returns>The extracted MemberExpression</returns>
        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is UnaryExpression unary)
            {
                return (MemberExpression)unary.Operand;
            }
            return (MemberExpression)expression;
        }
        #endregion

        #region Sql Execution Methods
        /// <summary>
        /// Executes the SQL query asynchronously and returns the first result or null if no results are found.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance</param>
        /// <returns>The first entity from the query results, or null if no results found</returns>
        public static async Task<T> GetSingleAsync<T>(this SQLQueryBuilder<T> builder)
            where T : class, new()
        {
            var list = await GetListAsync(builder);
            return list.FirstOrDefault();
        }

        /// <summary>
        /// Executes the SQL query asynchronously and returns a list of all matching entities.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="builder">The SQLQueryBuilder instance</param>
        /// <returns>A list of entities matching the query criteria</returns>
        public static async Task<List<T>> GetListAsync<T>(this SQLQueryBuilder<T> builder)
            where T : class, new()
        {
            List<T> results = new List<T>();
            string sqlQuery = builder.BuildQuery();

            using (var connection = new SqlConnection("Server=localhost,1433;Database=SqlSample;User Id=sa;Password=As.1234567890;TrustServerCertificate=True;"))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sqlQuery, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    results.AddRange(reader.MapToList(builder));
                }
            }
            return results;
        }

        /// <summary>
        /// Maps SqlDataReader results to a list of strongly-typed entities using the column aliases from the query builder.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="reader">The SqlDataReader containing the query results</param>
        /// <param name="builder">The SQLQueryBuilder instance that contains column alias mappings</param>
        /// <returns>A list of entities populated from the data reader</returns>
        internal static List<T> MapToList<T>(this SqlDataReader reader, SQLQueryBuilder<T> builder) where T : class, new()
        {
            var results = new List<T>();
            var properties = typeof(T).GetProperties();

            while (reader.Read())
            {
                var item = new T();
                foreach (var property in properties)
                {
                    try
                    {
                        var aliasInfo = builder.ColumnAndAlias.FirstOrDefault(x => x.PropertyName == property.Name);
                        if (aliasInfo != default)
                        {
                            int ordinal = reader.GetOrdinal(aliasInfo.Alias);
                            if (!reader.IsDBNull(ordinal))
                            {
                                object value = reader.GetValue(ordinal);
                                property.SetValue(item, value);
                            }
                        }
                    }
                    catch (IndexOutOfRangeException) { /* Column not found, continue */ }
                }
                results.Add(item);
            }
            return results;
        }
        #endregion
    }
}