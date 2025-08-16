using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    [SQBTable("Products")]
    public class Product

    {
        [SQBPrimaryKey]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        [SQBForeignKey<User>]
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        [SQBForeignKey<User>]
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockCount { get; set; }
        [SQBForeignKey<Category>]
        public int CategoryId { get; set; }
    }
}
