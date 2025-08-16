namespace SQLQueryBuilder.Flags
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SQBTableAttribute : Attribute
    {
        public readonly string TableName;

        public SQBTableAttribute(string tableName)
        {
            this.TableName = tableName;
        }
    }
}
