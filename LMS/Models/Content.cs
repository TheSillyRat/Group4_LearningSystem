using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Models
{
    [Table("Content")]
    public class Content
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string FileUrl { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        [ForeignKey("Module")]
        public int ModuleId { get; set; }

        public Module? Module { get; set; }
    }
}