using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    /// <summary>
    /// Represents a user entity in the system.
    /// Maps to the "Users" table in the database.
    /// </summary>
    [SQBTable("Users")]
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// Also serves as a foreign key reference to UserProfiles.
        /// </summary>
        [SQBForeignKey<UserProfiles>]
        [SQBPrimaryKey]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user account was created.
        /// </summary>
        public DateTime CreatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who created this user account.
        /// </summary>
        [SQBForeignKey<User>]
        public int? CreatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the user account was last updated.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the user who last updated this user account.
        /// </summary>
        [SQBForeignKey<User>]
        public int? UpdatedBy { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the user account is active.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Gets or sets a value indicating whether the user account is marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Gets or sets the username for the user account.
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Gets or sets the hashed password for the user account.
        /// </summary>
        public string PasswordHash { get; set; }
        
        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }
    }
}
