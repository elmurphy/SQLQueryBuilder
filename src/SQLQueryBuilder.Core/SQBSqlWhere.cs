using System.Collections.Generic;
using System.Linq;

namespace SQLQueryBuilder.Core
{
    public enum LogicalOperator
    {
        AND,
        OR
    }

    public class SQBSqlWhere
    {
        public string ColumnName { get; set; }
        public string TableAlias { get; set; }
        public string Operation { get; set; }
        public string Value { get; set; }

        public LogicalOperator Operator { get; set; } = LogicalOperator.AND;
        public List<SQBSqlWhere> NestedConditions { get; set; }

        public bool IsGroup => NestedConditions != null && NestedConditions.Any();

        public SQBSqlWhere()
        {
            NestedConditions = new List<SQBSqlWhere>();
        }
    }
}