using Microsoft.Data.SqlClient;
using SQLQueryBuilder.Flags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Core
{
    public static class SQLQueryBuilderExtension
    {
        #region Private Methods

        private static MemberExpression GetMemberExpression<T>(Expression<Func<T, object>> expression)
        {
            var body = expression.Body;
            if (body is UnaryExpression unaryExpression)
            {
                body = unaryExpression.Operand;
            }

            if (body is MemberExpression memberExpression)
            {
                return memberExpression;
            }

            throw new ArgumentException("The expression must be a member access expression.", nameof(expression));
        }

        #endregion

        #region Select Additional Methods

        /// <summary>
        /// Includes a foreign key relationship in the SQL query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="builder"></param>
        /// <param name="foreignKeySelector">Select your foreign key here</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static SQLQueryBuilder<T> Include<T, V>(this SQLQueryBuilder<T> builder, Expression<Func<T, object>> foreignKeySelector)
            where T : class, new()
            where V : class, new()
        {
            Type type = typeof(T);
            SQBTableAttribute? tableAttribute = type.GetCustomAttribute<SQBTableAttribute>();
            var tableName = tableAttribute != null ? tableAttribute.TableName : type.Name;


            var body = foreignKeySelector.Body;
            if (body is UnaryExpression unaryExpression)
            {
                body = unaryExpression.Operand;
            }

            if (body is MemberExpression memberExpression)
            {
                if (memberExpression.Member is PropertyInfo propertyInfo)
                {
                    var foreignKeyAttribute = propertyInfo.GetCustomAttribute<SQBForeignKeyAttribute<V>>();
                    if (foreignKeyAttribute != null)
                    {
                        var tableFrom = string.Empty;
                        if (builder.tableIndexes.Any())
                        {
                            tableFrom = builder.tableIndexes.Last();
                        }
                        else
                        {
                            tableFrom = tableName;
                        }

                        builder.includes.Add(new SQBInclude
                        {
                            TableName = foreignKeyAttribute.ForeignKeyTableName,
                            FieldName = propertyInfo.Name,
                            TableFrom = builder.TableName
                        });

                        builder.tableIndexes.Add(foreignKeyAttribute.ForeignKeyTableName);
                    }
                }
                else
                {
                    throw new ArgumentException("Seçilen üye bir özellik (property) olmalıdır.", nameof(foreignKeySelector));
                }
            }
            else
            {
                throw new ArgumentException("İfade bir özellik seçimi olmalıdır (örn: x => x.CategoryId).", nameof(foreignKeySelector));
            }

            return builder;
        }

        /// <summary>
        /// Includes a foreign key relationship in the SQL query, allowing for chaining with ThenInclude.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="builder"></param>
        /// <param name="foreignKeySelector">Select your foreign key here</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static SQLQueryBuilder<T> ThenInclude<T, V>(this SQLQueryBuilder<T> builder, Expression<Func<T, object>> foreignKeySelector)
            where T : class, new()
            where V : class, new()
        {
            if (!builder.includes.Any())
            {
                throw new InvalidOperationException("ThenInclude must be called after an Include method.");
            }

            Type fromType = typeof(T);
            SQBTableAttribute? fromTableAttribute = fromType.GetCustomAttribute<SQBTableAttribute>();
            var fromTableName = fromTableAttribute != null ? fromTableAttribute.TableName : fromType.Name;

            var memberExpression = GetMemberExpression(foreignKeySelector);
            var propertyName = memberExpression.Member.Name;

            if (!(memberExpression.Member is PropertyInfo propertyInfo))
            {
                throw new ArgumentException("The selected member must be a property.", nameof(foreignKeySelector));
            }

            var foreignKeyAttribute = propertyInfo.GetCustomAttribute<SQBForeignKeyAttribute<V>>();
            if (foreignKeyAttribute == null)
            {
                throw new InvalidOperationException($"The property {propertyName} does not have an SQBForeignKeyAttribute.");
            }

            builder.includes.Add(new SQBInclude
            {
                TableName = foreignKeyAttribute.ForeignKeyTableName,
                FieldName = propertyName,
                TableFrom = fromTableName
            });

            return builder;
        }

        #endregion


        #region Get

        /// <summary>
        /// Constructs a SQLQueryBuilder for the specified type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">Apply where query here. Just like EF</param>
        /// <returns></returns>
        public static SQLQueryBuilder<T> ConstructBuilder<T>(Expression<Func<T, bool>>? expression = null)
            where T : class, new()
        {
            var builder = new SQLQueryBuilder<T>();
            builder.MainEntityType = typeof(T);
            builder.SetQueryType(QueryType.Select);
            builder.SetTableName();
            builder.SetProperties();

            if (expression != null)
            {
                builder.whereConditions.AddRange(ExpressionParser.Parse(expression.Body));
            }

            return builder;
        }
        #endregion

        #region Operations
        public static SQLQueryBuilder<T> QueryInsert<T>(T value)
            where T : class, new()
        {
            throw new NotImplementedException("Single insert operation is not implemented yet. Use QueryInsert(List<T> values) for bulk insert.");
        }
        public static SQLQueryBuilder<T> QueryUpdate<T>(T value)
            where T : class, new()
        {
            throw new NotImplementedException("Single update operation is not implemented yet. Use QueryUpdate(List<T> values) for bulk update.");
        }
        public static SQLQueryBuilder<T> QueryDelete<T>(T value)
            where T : class, new()
        {
            throw new NotImplementedException("Single delete operation is not implemented yet. Use QueryDelete(List<T> values) for bulk delete.");
        }
        #endregion

        #region Bulk Operations
        public static SQLQueryBuilder<T> QueryInsert<T>(List<T> values)
            where T : class, new()
        {
            throw new NotImplementedException("Bulk insert operation is not implemented yet.");
        }

        public static SQLQueryBuilder<T> QueryUpdate<T>(List<T> values)
            where T : class, new()
        {
            throw new NotImplementedException("Bulk update operation is not implemented yet.");
        }

        public static SQLQueryBuilder<T> QueryDelete<T>(List<T> values)
            where T : class, new()
        {
            throw new NotImplementedException("Bulk delete operation is not implemented yet.");
        }
        #endregion
        #region Sql Execution Methods

        /// <summary>
        /// Get a single record from the database based on the SQLQueryBuilder configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static async Task<T?> GetSingleAsync<T>(this SQLQueryBuilder<T> builder)
            where T : class, new()
        {
            List<T> results = new List<T>();
            string sqlQuery = builder.BuildQuery();

            using (SqlConnection connection = new SqlConnection("Server=localhost,1433;Database=SqlSample;User Id=sa;Password=As.1234567890;TrustServerCertificate=True;"))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        results.AddRange(reader.MapToList<T>(builder));
                    }
                }
            }

            return results.FirstOrDefault();
        }

        /// <summary>
        /// Get a list of records from the database based on the SQLQueryBuilder configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static async Task<List<T>> GetListAsync<T>(this SQLQueryBuilder<T> builder)
            where T : class, new()
        {
            List<T> results = new List<T>();
            string sqlQuery = builder.BuildQuery();

            using (SqlConnection connection = new SqlConnection("Server=localhost,1433;Database=SqlSample;User Id=sa;Password=As.1234567890;TrustServerCertificate=True;"))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        results.AddRange(reader.MapToList<T>(builder));
                    }
                }
            }

            return results;
        }
        internal static List<T> MapToList<T>(this SqlDataReader reader, SQLQueryBuilder<T> builder) where T : class, new()
        {
            var results = new List<T>();

            PropertyInfo[] properties = typeof(T).GetProperties();
            while (reader.Read())
            {
                var item = new T();
                foreach (var property in properties)
                {
                    try
                    {
                        var propertyAlias = builder.ColumnAndAlias.FirstOrDefault(x => x.Item1 == property.Name).Item2 ?? property.Name;
                        int ordinal = reader.GetOrdinal(propertyAlias);
                        if (!reader.IsDBNull(ordinal))
                        {
                            object value = reader.GetValue(ordinal);
                            property.SetValue(item, value);
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        continue;
                    }
                }
                results.Add(item);
            }

            return results;
        }
        #endregion
    }
}