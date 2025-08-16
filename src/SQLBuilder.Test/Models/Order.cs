using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    [SQBTable("Orders")]
    public class Order
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

        public decimal TotalAmount { get; set; }
        public decimal Status { get; set; }
        [SQBForeignKey<User>]
        public int UserId { get; set; }
    }
}
