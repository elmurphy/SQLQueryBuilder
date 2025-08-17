using System.Collections.Generic;
using System.Linq;

namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Enumeration of logical operators used to combine WHERE conditions.
    /// </summary>
    public enum LogicalOperator
    {
        /// <summary>
        /// Logical AND operator for combining conditions.
        /// </summary>
        AND,
        
        /// <summary>
        /// Logical OR operator for combining conditions.
        /// </summary>
        OR
    }

    /// <summary>
    /// Represents a SQL WHERE condition that can be either a simple condition or a group of nested conditions.
    /// </summary>
    public class SQBSqlWhere
    {
        /// <summary>
        /// Gets or sets the database column name for simple conditions.
        /// </summary>
        public string ColumnName { get; set; }
        
        /// <summary>
        /// Gets or sets the table alias used in the SQL query for simple conditions.
        /// </summary>
        public string TableAlias { get; set; }
        
        /// <summary>
        /// Gets or sets the comparison operation (=, !=, >, <, etc.) for simple conditions.
        /// </summary>
        public string Operation { get; set; }
        
        /// <summary>
        /// Gets or sets the value to compare against for simple conditions.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the logical operator (AND/OR) used to combine nested conditions in groups.
        /// </summary>
        public LogicalOperator Operator { get; set; } = LogicalOperator.AND;
        
        /// <summary>
        /// Gets or sets the list of nested WHERE conditions for grouped conditions.
        /// </summary>
        public List<SQBSqlWhere> NestedConditions { get; set; }

        /// <summary>
        /// Gets a value indicating whether this condition represents a group of nested conditions.
        /// </summary>
        public bool IsGroup => NestedConditions != null && NestedConditions.Any();

        /// <summary>
        /// Initializes a new instance of the SQBSqlWhere class.
        /// </summary>
        public SQBSqlWhere()
        {
            NestedConditions = new List<SQBSqlWhere>();
        }
    }
}
