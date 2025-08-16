namespace SQLQueryBuilder.Flags
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SQBPropertyAttribute : Attribute
    {
        public readonly string SqlFieldName;
        public readonly string? SqlDataType;

        public SQBPropertyAttribute(string sqlFieldName, string? sqlDataType = null)
        {
            this.SqlFieldName = sqlFieldName;
            this.SqlDataType = sqlDataType;
        }
    }
}
