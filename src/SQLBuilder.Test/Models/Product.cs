using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    /// <summary>
    /// Represents a product entity in the system.
    /// Maps to the "Products" table in the database.
    /// </summary>
    [SQBTable("Products")]
    public class Product

    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        [SQBPrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the product was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who created this product.
        /// </summary>
        [SQBForeignKey<User>]
        public int? CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the product was last updated.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who last updated this product.
        /// </summary>
        [SQBForeignKey<User>]
        public int? UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the product is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether the product is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the price of the product.
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Gets or sets the current stock count for the product.
        /// </summary>
        public int StockCount { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the category this product belongs to.
        /// </summary>
        [SQBForeignKey<Category>]
        public int CategoryId { get; set; }
    }
}
