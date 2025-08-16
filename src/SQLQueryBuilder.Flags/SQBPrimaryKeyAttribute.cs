using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLQueryBuilder.Flags
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SQBPrimaryKeyAttribute : Attribute
    {
        public SQBPrimaryKeyAttribute()
        {

        }
    }
}
