// ============================================================
// File: Models/Session.cs
// Mô tả: Model buổi học trực tuyến
// CHECKPOINT: FK tới Course, có StartTime/EndTime bắt buộc
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Buổi học trực tuyến thuộc một khóa học
    /// </summary>
    public class Session
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
        /// Tiêu đề buổi học
        /// </summary>
        [Required(ErrorMessage = "Tiêu đề buổi học là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả nội dung buổi học (không bắt buộc)
        /// </summary>
        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        /// <summary>
        /// Thời gian bắt đầu buổi học
        /// </summary>
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        [Display(Name = "Bắt đầu")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc buổi học
        /// </summary>
        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        [Display(Name = "Kết thúc")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Đường dẫn họp trực tuyến (Zoom, Google Meet, v.v.)
        /// </summary>
        [StringLength(500, ErrorMessage = "URL tối đa 500 ký tự")]
        [Display(Name = "Link họp")]
        public string? MeetingUrl { get; set; }

        /// <summary>
        /// Ngày tạo buổi học
        /// </summary>
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// Khóa học chứa buổi học này
        /// </summary>
        public virtual Course Course { get; set; } = null!;
    }
}
