using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Submission")]
    public class Submission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubmissionId { get; set; }

        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public DateTime SubmitDate { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? FilePath { get; set; }

        public double? Score { get; set; }

        [StringLength(1000)]
        public string? Feedback { get; set; }

        public Assignment? Assignment { get; set; }

        public User? Student { get; set; }
    }
}