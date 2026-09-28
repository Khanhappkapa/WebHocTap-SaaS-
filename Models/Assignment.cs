// ============================================================
// File: Models/Assignment.cs
// Mô tả: Model bài tập
// CHECKPOINT: FK tới Course, có Deadline bắt buộc, MaxScore mặc định 10
//             Quan hệ 1-nhiều với Submission (Cascade Delete)
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Bài tập thuộc một khóa học
    /// Khi xóa Assignment sẽ xóa luôn các Submission liên quan (Cascade)
    /// </summary>
    public class Assignment
    {
        // --------------------------------------------------
        // CHECKPOINT: Thuộc tính cơ bản
        // --------------------------------------------------

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID khóa học (Foreign Key)
        /// </summary>
        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        /// <summary>
        /// Tiêu đề bài tập
        /// </summary>
        [Required(ErrorMessage = "Tiêu đề bài tập là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết bài tập (không bắt buộc)
        /// </summary>
        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        /// <summary>
        /// Hạn nộp bài (bắt buộc)
        /// </summary>
        [Required(ErrorMessage = "Hạn nộp bài là bắt buộc")]
        [Display(Name = "Hạn nộp")]
        public DateTime Deadline { get; set; }

        /// <summary>
        /// Điểm tối đa (mặc định = 10)
        /// </summary>
        [Display(Name = "Điểm tối đa")]
        public int MaxScore { get; set; } = 10;

        /// <summary>
        /// Ngày tạo bài tập
        /// </summary>
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// Khóa học chứa bài tập này
        /// </summary>
        public virtual Course Course { get; set; } = null!;

        /// <summary>
        /// Danh sách bài nộp của học sinh cho bài tập này
        /// CHECKPOINT: Cascade Delete - xóa Assignment sẽ xóa luôn Submissions
        /// </summary>
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
