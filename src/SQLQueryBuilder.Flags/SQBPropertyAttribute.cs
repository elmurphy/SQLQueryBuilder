namespace SQLQueryBuilder.Flags
{
    /// <summary>
    /// Attribute used to specify custom SQL field name and data type for an entity property.
    /// When not specified, the property name will be used as the column name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SQBPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets the SQL field name associated with the property.
        /// </summary>
        public readonly string SqlFieldName;
        
        /// <summary>
        /// Gets the SQL data type for the field, if specified.
        /// </summary>
        public readonly string? SqlDataType;

        /// <summary>
        /// Initializes a new instance of the SQBPropertyAttribute class with the specified field name and optional data type.
        /// </summary>
        /// <param name="sqlFieldName">The SQL field name for the property</param>
        /// <param name="sqlDataType">Optional SQL data type specification</param>
        public SQBPropertyAttribute(string sqlFieldName, string? sqlDataType = null)
        {
            this.SqlFieldName = sqlFieldName;
            this.SqlDataType = sqlDataType;
        }
    }
}
