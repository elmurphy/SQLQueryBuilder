using SQLQueryBuilder.Flags;

namespace SQLBuilder.Test.Models
{
    [SQBTable("UserProfiles")]
    public class UserProfiles
    {
        [SQBForeignKey<User>]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
