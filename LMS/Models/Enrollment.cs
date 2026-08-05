using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Enrollment")]
    public class Enrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrollmentId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public DateTime EnrollmentDate { get; set; }

        public double? Progress { get; set; }

        public bool Attendance { get; set; }

        public User? Student { get; set; }

        public Course? Course { get; set; }
    }
}