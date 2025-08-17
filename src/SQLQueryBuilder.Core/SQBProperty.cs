namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Represents a database property with its name and type information.
    /// Used internally for property mapping and SQL generation.
    /// </summary>
    internal class SQBProperty
    {
        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the SQL type of the property.
        /// </summary>
        public string Type { get; set; }
    }
}
