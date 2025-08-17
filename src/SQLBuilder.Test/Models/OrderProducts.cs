using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    /// <summary>
    /// Represents the many-to-many relationship between orders and products.
    /// This is a junction table entity that stores order line items.
    /// </summary>
    public class OrderProducts
    {
        /// <summary>
        /// Gets or sets the ID of the order this line item belongs to.
        /// </summary>
        [SQBForeignKey<Order>]
        public int OrderId { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the product in this line item.
        /// </summary>
        [SQBForeignKey<Product>]
        public int ProductId { get; set; }
        
        /// <summary>
        /// Gets or sets the quantity of the product in this order line.
        /// </summary>
        public int Quantity { get; set; }
        
        /// <summary>
        /// Gets or sets the unit price of the product at the time of the order.
        /// </summary>
        public decimal UnitPrice { get; set; }
    }
}
