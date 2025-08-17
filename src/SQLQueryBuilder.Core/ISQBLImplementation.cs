
namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Interface that defines the contract for database-specific SQL query template implementations.
    /// Provides methods for generating SQL query templates for different database systems.
    /// </summary>
    public interface ISQBLImplementation
    {
        /// <summary>
        /// Gets the DELETE query template for the specific database implementation.
        /// </summary>
        /// <returns>A DELETE query template string with placeholders</returns>
        string DeleteQueryTemplate();
        
        /// <summary>
        /// Maps a .NET type to its corresponding SQL data type for the specific database.
        /// </summary>
        /// <param name="propertyType">The .NET property type to map</param>
        /// <returns>The SQL data type string for the database</returns>
        string GetSqlType(Type propertyType);
        
        /// <summary>
        /// Gets the INSERT query template for the specific database implementation.
        /// </summary>
        /// <returns>An INSERT query template string with placeholders</returns>
        string InsertQueryTemplate();
        
        /// <summary>
        /// Gets the GROUP BY clause template for the specific database implementation.
        /// </summary>
        /// <returns>A GROUP BY clause template string with placeholders</returns>
        string GroupByClauseTemplate();
        
        /// <summary>
        /// Gets the ORDER BY clause template for the specific database implementation.
        /// </summary>
        /// <returns>An ORDER BY clause template string with placeholders</returns>
        string OrderByClauseTemplate();
        
        /// <summary>
        /// Gets the SELECT query template for the specific database implementation.
        /// </summary>
        /// <returns>A SELECT query template string with placeholders</returns>
        string SelectQueryTemplate();
        
        /// <summary>
        /// Gets the UPDATE query template for the specific database implementation.
        /// </summary>
        /// <returns>An UPDATE query template string with placeholders</returns>
        string UpdateQueryTemplate();
        
        /// <summary>
        /// Gets the WHERE clause template for the specific database implementation.
        /// </summary>
        /// <returns>A WHERE clause template string with placeholders</returns>
        string WhereClauseTemplate();
    }
}
