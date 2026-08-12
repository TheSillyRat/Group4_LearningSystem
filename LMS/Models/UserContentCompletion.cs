using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("UserContentCompletion")]
    public class UserContentCompletion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserContentCompletionId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Content")]
        public int ContentId { get; set; }

        public DateTime CompletedAt { get; set; }

        public User? Student { get; set; }

        public Content? Content { get; set; }
    }
}
