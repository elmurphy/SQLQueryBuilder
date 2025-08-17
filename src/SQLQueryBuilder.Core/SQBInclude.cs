namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Represents an include specification for JOIN operations in SQL queries.
    /// </summary>
    public class SQBInclude
    {
        /// <summary>
        /// Gets or sets the entity type that contains the foreign key property.
        /// </summary>
        public Type TypeFrom { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the foreign key property to join on.
        /// </summary>
        public string FieldName { get; set; }
    }
}
