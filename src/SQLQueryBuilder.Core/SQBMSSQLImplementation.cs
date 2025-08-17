using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Microsoft SQL Server specific implementation of ISQBLImplementation.
    /// Provides SQL Server compatible query templates and type mappings.
    /// </summary>
    public class SQBMSSQLImplementation : ISQBLImplementation
    {
        /// <summary>
        /// Dictionary mapping .NET types to their corresponding SQL Server data types.
        /// </summary>
        private readonly Dictionary<Type, string> _typeMapping = new Dictionary<Type, string>
        {
            { typeof(int), "INT" },
            { typeof(long), "BIGINT" },
            { typeof(short), "SMALLINT" },
            { typeof(byte), "TINYINT" },
            { typeof(string), "NVARCHAR(MAX)" },
            { typeof(DateTime), "DATETIME2" },
            { typeof(bool), "BIT" },
            { typeof(decimal), "DECIMAL(18, 2)" },
            { typeof(float), "FLOAT" },
            { typeof(double), "FLOAT" },
            { typeof(Guid), "UNIQUEIDENTIFIER" }
        };

        /// <summary>
        /// Maps a .NET type to its corresponding SQL Server data type.
        /// </summary>
        /// <param name="propertyType">The .NET property type to map</param>
        /// <returns>The SQL Server data type string</returns>
        /// <exception cref="ArgumentException">Thrown when the property type is not supported</exception>
        public string GetSqlType(Type propertyType)
        {
            Type type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (_typeMapping.TryGetValue(type, out var sql_type))
            {
                return sql_type;
            }

            throw new ArgumentException($"Unsupported C# type: {propertyType.Name}");
        }

        /// <summary>
        /// Gets the WHERE clause template for SQL Server.
        /// </summary>
        /// <returns>A WHERE clause template with placeholder for conditions</returns>
        public string WhereClauseTemplate()
        {
            return "WHERE {0}";
        }

        /// <summary>
        /// Gets the GROUP BY clause template for SQL Server.
        /// </summary>
        /// <returns>A GROUP BY clause template with placeholder for columns</returns>
        public string GroupByClauseTemplate()
        {
            return "GROUP BY {0}";
        }

        /// <summary>
        /// Gets the ORDER BY clause template for SQL Server.
        /// </summary>
        /// <returns>An ORDER BY clause template with placeholder for columns</returns>
        public string OrderByClauseTemplate()
        {
            return "ORDER BY {0}";
        }

        /// <summary>
        /// Gets the SELECT query template for SQL Server.
        /// </summary>
        /// <returns>A SELECT query template with placeholders for columns and table</returns>
        public string SelectQueryTemplate()
        {
            return $"SELECT {{0}} FROM {{1}}";
        }

        /// <summary>
        /// Gets the INSERT query template for SQL Server.
        /// </summary>
        /// <returns>An INSERT query template with placeholders for table, columns, and values</returns>
        public string InsertQueryTemplate()
        {
            return "INSERT INTO {0} ({1}) VALUES ({2})";
        }

        /// <summary>
        /// Gets the UPDATE query template for SQL Server.
        /// </summary>
        /// <returns>An UPDATE query template with placeholders for table and SET clause</returns>
        public string UpdateQueryTemplate()
        {
            return "UPDATE {0} SET {1}";
        }

        /// <summary>
        /// Gets the DELETE query template for SQL Server.
        /// </summary>
        /// <returns>A DELETE query template with placeholder for table</returns>
        public string DeleteQueryTemplate()
        {
            return "DELETE FROM {0}";
        }
    }
}
