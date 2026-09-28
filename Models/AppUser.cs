// ============================================================
// File: Models/AppUser.cs
// Mô tả: Model người dùng mở rộng từ IdentityUser
// CHECKPOINT: Kế thừa IdentityUser + thêm FullName, UserRole, IsActive
// ============================================================

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebHocTap_SaaS_.Models
{
    /// <summary>
    /// Người dùng hệ thống, kế thừa IdentityUser (có sẵn: Id, UserName, Email, PasswordHash, ...)
    /// Thêm các thuộc tính riêng: FullName, UserRole, IsActive
    /// </summary>
    public class AppUser : IdentityUser
    {
        // --------------------------------------------------
        // CHECKPOINT: Thuộc tính mở rộng
        // --------------------------------------------------

        /// <summary>
        /// Họ và tên đầy đủ của người dùng
        /// </summary>
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò người dùng: "Admin", "Teacher", "Student"
        /// Lưu ý: Đây là thuộc tính phụ để hiển thị nhanh, 
        /// vai trò chính thức vẫn được quản lý qua ASP.NET Core Identity Roles
        /// </summary>
        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        [StringLength(20, ErrorMessage = "Vai trò tối đa 20 ký tự")]
        [Display(Name = "Vai trò")]
        public string UserRole { get; set; } = "Student";

        /// <summary>
        /// Trạng thái hoạt động của tài khoản (true = đang hoạt động)
        /// </summary>
        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        // --------------------------------------------------
        // CHECKPOINT: Navigation Properties
        // --------------------------------------------------

        /// <summary>
        /// Danh sách các khóa học mà Teacher này dạy
        /// Chỉ có ý nghĩa khi UserRole = "Teacher"
        /// </summary>
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

        /// <summary>
        /// Danh sách các đăng ký khóa học (Enrollment) của Student
        /// Chỉ có ý nghĩa khi UserRole = "Student"
        /// </summary>
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        /// <summary>
        /// Danh sách các bài nộp (Submission) của Student
        /// Chỉ có ý nghĩa khi UserRole = "Student"
        /// </summary>
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
