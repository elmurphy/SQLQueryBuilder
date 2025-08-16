using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    public class OrderProducts
    {
        [SQBForeignKey<Order>]
        public int OrderId { get; set; }
        [SQBForeignKey<Product>]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
