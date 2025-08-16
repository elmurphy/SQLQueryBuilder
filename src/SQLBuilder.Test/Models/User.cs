using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    [SQBTable("Users")]
    public class User
    {
        [SQBForeignKey<UserProfiles>]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
    }
}
