using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Core
{
    /// <summary>
    /// Enumeration of SQL query types that can be built by the SQLQueryBuilder.
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// No specific query type specified.
        /// </summary>
        None,
        
        /// <summary>
        /// SELECT query for retrieving data.
        /// </summary>
        Select,
        
        /// <summary>
        /// INSERT query for adding new data.
        /// </summary>
        Insert,
        
        /// <summary>
        /// UPDATE query for modifying existing data.
        /// </summary>
        Update,
        
        /// <summary>
        /// DELETE query for removing data.
        /// </summary>
        Delete
    }
}
