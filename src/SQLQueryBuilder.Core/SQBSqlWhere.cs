namespace SQLQueryBuilder.Core
{
    public enum LogicalOperator
    {
        AND,
        OR
    }

    public class SQBSqlWhere
    {
        // Basit koşullar için (örn: ColumnName = Value)
        public string ColumnName { get; set; }
        public string FunctionTemplate { get; set; } = "{0}";
        public string Operation { get; set; }
        public string Value { get; set; }

        // Gruplanmış koşullar için (örn: ( ... ) AND ( ... ))
        public LogicalOperator Operator { get; set; } = LogicalOperator.AND;
        public List<SQBSqlWhere> NestedConditions { get; set; }

        /// <summary>
        /// Bu düğümün bir grup olup olmadığını belirtir.
        /// </summary>
        public bool IsGroup => NestedConditions != null && NestedConditions.Any();

        public SQBSqlWhere()
        {
            NestedConditions = new List<SQBSqlWhere>();
        }
    }
}