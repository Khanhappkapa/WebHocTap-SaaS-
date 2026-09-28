// ============================================================
// File: Models/Course.cs
// Mô tả: Model khóa học (lớp học trực tuyến)
// CHECKPOINT: Quan hệ 1-nhiều với Teacher(AppUser), Enrollments, Sessions, Materials, Assignments
// ============================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Khóa học / Lớp học trực tuyến
    /// Mỗi khóa học thuộc về một Teacher (AppUser)
    /// </summary>
    public class Course
    {
        // --------------------------------------------------
        // CHECKPOINT: Thuộc tính cơ bản
        // --------------------------------------------------

        /// <summary>
        /// Khóa chính, tự tăng
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Tên khóa học
        /// </summary>
        [Required(ErrorMessage = "Tên khóa học là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên khóa học tối đa 200 ký tự")]
        [Display(Name = "Tên khóa học")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả khóa học (không bắt buộc)
        /// </summary>
        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        /// <summary>
        /// ID của giáo viên (Foreign Key tới AppUser)
        /// Kiểu string vì IdentityUser.Id là string (GUID)
        /// </summary>
        [Required(ErrorMessage = "Giáo viên là bắt buộc")]
        [ForeignKey("Teacher")]
        public string TeacherId { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái khóa học: Draft / Published / Closed
        /// CHECKPOINT: Sử dụng enum CourseStatus thay vì string để type-safe
        /// </summary>
        [Required]
        [Display(Name = "Trạng thái")]
        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        /// <summary>
        /// Ngày tạo khóa học
        /// </summary>
        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày cập nhật gần nhất (null nếu chưa cập nhật)
        /// </summary>
        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedAt { get; set; }

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// Giáo viên phụ trách khóa học
        /// Quan hệ: Nhiều Course → 1 Teacher (AppUser)
        /// </summary>
        public virtual AppUser Teacher { get; set; } = null!;

        /// <summary>
        /// Danh sách học sinh đăng ký khóa học
        /// </summary>
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        /// <summary>
        /// Danh sách buổi học trực tuyến
        /// </summary>
        public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

        /// <summary>
        /// Danh sách tài liệu học tập
        /// </summary>
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();

        /// <summary>
        /// Danh sách bài tập
        /// </summary>
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
