
namespace SQLQueryBuilder.Core
{
    public interface ISQBLImplementation
    {
        string DeleteQueryTemplate();
        string GetSqlType(Type propertyType);
        string InsertQueryTemplate();
        string GroupByClauseTemplate();
        string OrderByClauseTemplate();
        string SelectQueryTemplate();
        string UpdateQueryTemplate();
        string WhereClauseTemplate();
    }
}