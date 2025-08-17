using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    /// <summary>
    /// Represents a product category entity with hierarchical structure support.
    /// Maps to the "Categories" table in the database.
    /// </summary>
    [SQBTable("Categories")]
    public class Category
    {
        /// <summary>
        /// Gets or sets the unique identifier for the category.
        /// </summary>
        [SQBPrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the category was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who created this category.
        /// </summary>
        [SQBForeignKey<User>]
        public int? CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the category was last updated.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who last updated this category.
        /// </summary>
        [SQBForeignKey<User>]
        public int? UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the category is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether the category is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the optional description of the category.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent category for hierarchical structure.
        /// </summary>
        [SQBForeignKey<Category>]
        public int ParentCategoryId { get; set; }
    }
}
