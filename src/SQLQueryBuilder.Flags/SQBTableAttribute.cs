namespace SQLQueryBuilder.Flags
{
    /// <summary>
    /// Attribute used to specify the database table name for an entity class.
    /// When not specified, the class name will be used as the table name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SQBTableAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the database table associated with the entity.
        /// </summary>
        public readonly string TableName;

        /// <summary>
        /// Initializes a new instance of the SQBTableAttribute class with the specified table name.
        /// </summary>
        /// <param name="tableName">The name of the database table</param>
        public SQBTableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }
}
