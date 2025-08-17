using System.Reflection;

namespace SQLQueryBuilder.Flags
{
    /// <summary>
    /// Attribute used to mark a property as a foreign key that references another entity.
    /// This attribute enables automatic JOIN generation in SQL queries.
    /// </summary>
    /// <typeparam name="T">The target entity type that this foreign key references</typeparam>
    [AttributeUsage(AttributeTargets.Property)]
    public class SQBForeignKeyAttribute<T> : Attribute
        where T : class
    {
        /// <summary>
        /// Gets the name of the table that this foreign key references.
        /// </summary>
        public string ForeignKeyTableName { get; }
        
        /// <summary>
        /// Gets the name of the primary key property in the referenced table.
        /// </summary>
        public string ForeignKeyPropertyName { get; }
        
        /// <summary>
        /// Initializes a new instance of the SQBForeignKeyAttribute class.
        /// Automatically discovers the referenced table name and primary key property from the target type.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the target type does not have a primary key defined</exception>
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
