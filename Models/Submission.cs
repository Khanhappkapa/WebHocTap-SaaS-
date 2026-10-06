// ============================================================
// File: Models/Submission.cs
// Mô tả: Model bài nộp của học sinh
// CHECKPOINT: Unique constraint trên (AssignmentId + StudentId) - cấu hình trong DbContext
//             Mỗi Student chỉ nộp 1 lần cho 1 Assignment
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Bài nộp của học sinh cho một bài tập
    /// Ràng buộc: Một student chỉ nộp 1 lần cho 1 assignment
    /// Unique (AssignmentId + StudentId) được cấu hình qua Fluent API trong DbContext
    /// </summary>
    public class Submission
    {
        // --------------------------------------------------
        // CHECKPOINT: Thuộc tính cơ bản
        // --------------------------------------------------

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// ID bài tập (Foreign Key)
        /// </summary>
        [Required]
        [ForeignKey("Assignment")]
        public int AssignmentId { get; set; }

        /// <summary>
        /// ID học sinh (Foreign Key tới AppUser)
        /// </summary>
        [Required]
        [ForeignKey("Student")]
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung bài nộp dạng text (không bắt buộc)
        /// </summary>
        [StringLength(5000, ErrorMessage = "Nội dung tối đa 5000 ký tự")]
        [Display(Name = "Nội dung")]
        public string? Content { get; set; }

        /// <summary>
        /// Đường dẫn tệp đính kèm (không bắt buộc)
        /// </summary>
        [StringLength(500, ErrorMessage = "URL tối đa 500 ký tự")]
        [Display(Name = "Tệp đính kèm")]
        public string? FileUrl { get; set; }

        /// <summary>
        /// Tên file gốc (dùng để trả về header Content-Disposition khi download)
        /// </summary>
        [StringLength(255)]
        public string? FileName { get; set; }

        /// <summary>
        /// MIME type thật lúc upload (ví dụ: application/pdf)
        /// </summary>
        [StringLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Nội dung file, lưu DB (bytea). Production thật đổi sang S3 + signed URL.
        /// </summary>
        public byte[]? FileData { get; set; }

        /// <summary>
        /// Thời điểm nộp bài
        /// </summary>
        [Display(Name = "Ngày nộp")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Điểm số (null = chưa chấm)
        /// </summary>
        [Display(Name = "Điểm")]
        public double? Score { get; set; }

        /// <summary>
        /// Nhận xét của giáo viên (không bắt buộc)
        /// </summary>
        [StringLength(2000, ErrorMessage = "Nhận xét tối đa 2000 ký tự")]
        [Display(Name = "Nhận xét")]
        public string? Feedback { get; set; }

        /// <summary>
        /// Thời điểm chấm điểm (null = chưa chấm)
        /// </summary>
        [Display(Name = "Ngày chấm")]
        public DateTime? GradedAt { get; set; }

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// Bài tập tương ứng
        /// </summary>
        public virtual Assignment Assignment { get; set; } = null!;

        /// <summary>
        /// Học sinh nộp bài
        /// </summary>
        public virtual AppUser Student { get; set; } = null!;
    }
}
