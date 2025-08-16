using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Core
{
    public class SQBMSSQLImplementation : ISQBLImplementation
    {
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

        public string GetSqlType(Type propertyType)
        {
            Type type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (_typeMapping.TryGetValue(type, out var sql_type))
            {
                return sql_type;
            }

            throw new ArgumentException($"Desteklenmeyen C# Tipi: {propertyType.Name}");
        }

        public string WhereClauseTemplate()
        {
            return "WHERE {0}";
        }

        public string GroupByClauseTemplate()
        {
            return "GROUP BY {0}";
        }

        public string OrderByClauseTemplate()
        {
            return "ORDER BY {0}";
        }

        public string SelectQueryTemplate()
        {
            return $"SELECT {{0}} FROM {{1}}";
        }

        public string InsertQueryTemplate()
        {
            return "INSERT INTO {0} ({1}) VALUES ({2})";
        }

        public string UpdateQueryTemplate()
        {
            return "UPDATE {0} SET {1}";
        }

        public string DeleteQueryTemplate()
        {
            return "DELETE FROM {0}";
        }
    }
}
