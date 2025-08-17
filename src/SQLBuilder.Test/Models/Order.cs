using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    /// <summary>
    /// Represents an order entity in the system.
    /// Maps to the "Orders" table in the database.
    /// </summary>
    [SQBTable("Orders")]
    public class Order
    {
        /// <summary>
        /// Gets or sets the unique identifier for the order.
        /// </summary>
        [SQBPrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the order was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who created this order.
        /// </summary>
        [SQBForeignKey<User>]
        public int? CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the order was last updated.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who last updated this order.
        /// </summary>
        [SQBForeignKey<User>]
        public int? UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the order is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether the order is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the total amount of the order.
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Gets or sets the status of the order.
        /// </summary>
        public decimal Status { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who placed this order.
        /// </summary>
        [SQBForeignKey<User>]
        public int UserId { get; set; }
    }
}
