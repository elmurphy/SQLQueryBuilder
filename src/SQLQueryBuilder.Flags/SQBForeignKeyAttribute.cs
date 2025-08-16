using System.Reflection;

namespace SQLQueryBuilder.Flags
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SQBForeignKeyAttribute<T> : Attribute
        where T : class
    {
        public string ForeignKeyTableName { get; }
        public string ForeignKeyPropertyName { get; }
        public SQBForeignKeyAttribute()
        {
            var type = typeof(T);

            var primaryKeyProperty = type
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(SQBPrimaryKeyAttribute), false).Any());

            if (primaryKeyProperty == null)
            {
                throw new InvalidOperationException($"Type {type.Name} does not have a primary key defined.");
            }
            ForeignKeyPropertyName = primaryKeyProperty.Name;

            SQBTableAttribute? tableAttribute = type.GetCustomAttribute<SQBTableAttribute>();
            var tableName = tableAttribute != null ? tableAttribute.TableName : type.Name;
            ForeignKeyTableName = tableName;
        }

    }
}
