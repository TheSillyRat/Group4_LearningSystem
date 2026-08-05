using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [ForeignKey("Role")]
        public int RoleId { get; set; }

        public Role? Role { get; set; }

        public ICollection<Enrollment>? Enrollments { get; set; }

        public ICollection<Course>? Courses { get; set; }

        public ICollection<Assignment>? Assignments { get; set; }

        public ICollection<ForumPost>? ForumPosts { get; set; }

        public ICollection<ForumReply>? ForumReplies { get; set; }
    }
}