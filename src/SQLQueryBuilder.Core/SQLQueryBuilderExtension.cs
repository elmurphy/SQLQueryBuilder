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
    public static class SQLQueryBuilderExtension
    {
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
        public static async Task<T> GetSingleAsync<T>(this SQLQueryBuilder<T> builder)
            where T : class, new()
        {
            var list = await GetListAsync(builder);
            return list.FirstOrDefault();
        }

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
                    catch (IndexOutOfRangeException) { /* Kolon bulunamadı, devam et */ }
                }
                results.Add(item);
            }
            return results;
        }
        #endregion
    }
}