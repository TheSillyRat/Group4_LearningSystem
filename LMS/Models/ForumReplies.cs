using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("ForumReplies")]
    public class ForumReply
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReplyId { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        // Lưu thời gian chỉnh sửa bình luận
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("ForumPost")]
        public int PostId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public ForumPost? ForumPost { get; set; }

        public User? User { get; set; }
    }
}