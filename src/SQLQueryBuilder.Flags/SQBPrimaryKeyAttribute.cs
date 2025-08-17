using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Flags
{
    /// <summary>
    /// Attribute used to mark a property as the primary key of an entity.
    /// This attribute is essential for automatic JOIN generation and entity identification.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SQBPrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the SQBPrimaryKeyAttribute class.
        /// </summary>
        public SQBPrimaryKeyAttribute()
        {

        }
    }
}
