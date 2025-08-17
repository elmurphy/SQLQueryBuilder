namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Enumeration of supported database types for SQL query generation.
    /// </summary>
    public enum DatabaseType
    {
        /// <summary>
        /// No specific database type specified.
        /// </summary>
        None,
        
        /// <summary>
        /// Microsoft SQL Server database.
        /// </summary>
        MSSQL,
        
        /// <summary>
        /// PostgreSQL database.
        /// </summary>
        PostgreSQL
    }
}
