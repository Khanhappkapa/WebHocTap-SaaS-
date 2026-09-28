// ============================================================
// File: Models/ViewModels/RegisterViewModel.cs
// Mô tả: ViewModel cho trang đăng ký
// CHECKPOINT: CHỈ cho phép chọn "Teacher" hoặc "Student"
//             Server chặn cứng nếu UserRole == "Admin"
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhập lại mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;

        /// <summary>
        /// CHỈ chấp nhận "Teacher" hoặc "Student".
        /// Server sẽ chặn cứng nếu giá trị là "Admin" (chống leo thang đặc quyền).
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        [Display(Name = "Vai trò")]
        public string UserRole { get; set; } = "Student";
    }
}
