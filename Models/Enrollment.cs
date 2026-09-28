// ============================================================
// File: Models/Enrollment.cs
// Mô tả: Model đăng ký khóa học (quan hệ nhiều-nhiều giữa Student và Course)
// CHECKPOINT: Unique constraint trên (CourseId + StudentId) - cấu hình trong DbContext
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Bản ghi đăng ký khóa học
    /// Một Student chỉ được đăng ký 1 Course duy nhất 1 lần
    /// Ràng buộc Unique (CourseId + StudentId) được cấu hình qua Fluent API trong DbContext
    /// </summary>
    public class Enrollment
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
        /// ID học sinh (Foreign Key tới AppUser)
        /// </summary>
        [Required]
        [ForeignKey("Student")]
        public string StudentId { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm đăng ký
        /// </summary>
        [Display(Name = "Ngày đăng ký")]
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// Khóa học được đăng ký
        /// </summary>
        public virtual Course Course { get; set; } = null!;

        /// <summary>
        /// Học sinh đăng ký
        /// </summary>
        public virtual AppUser Student { get; set; } = null!;
    }
}
