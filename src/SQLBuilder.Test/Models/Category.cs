using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    [SQBTable("Categories")]
    public class Category
    {
        [SQBPrimaryKey]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public string Name { get; set; }
        public string? Description { get; set; }

        [SQBForeignKey<Category>]
        public int ParentCategoryId { get; set; }
    }
}
