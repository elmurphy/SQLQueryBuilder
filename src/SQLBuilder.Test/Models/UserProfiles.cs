using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    /// <summary>
    /// Represents additional profile information for users.
    /// Maps to the "UserProfiles" table in the database.
    /// Has a one-to-one relationship with the User entity.
    /// </summary>
    [SQBTable("UserProfiles")]
    public class UserProfiles
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user profile.
        /// This corresponds to the User ID and serves as both primary key and foreign key.
        /// </summary>
        [SQBForeignKey<User>]
        [SQBPrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the profile was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who created this profile.
        /// </summary>
        [SQBForeignKey<User>]
        public int? CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the profile was last updated.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who last updated this profile.
        /// </summary>
        [SQBForeignKey<User>]
        public int? UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the profile is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether the profile is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the first name of the user.
        /// </summary>
        public string? FirstName { get; set; }
        
        /// <summary>
        /// Gets or sets the last name of the user.
        /// </summary>
        public string? LastName { get; set; }
        
        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        public string? PhoneNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the address of the user.
        /// </summary>
        public string? Address { get; set; }
    }
}
