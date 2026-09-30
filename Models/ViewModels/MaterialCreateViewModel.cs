using System.ComponentModel.DataAnnotations;
using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    public class MaterialCreateViewModel
    {
        [Required(ErrorMessage = "Tiêu đề tài liệu là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chọn loại tài liệu")]
        [Display(Name = "Loại tài liệu")]
        public FileType FileType { get; set; }

        [Url(ErrorMessage = "URL phải hợp lệ")]
        [StringLength(500)]
        [Display(Name = "Đường dẫn (nếu là Link)")]
        public string? FileUrl { get; set; }

        [Display(Name = "File tải lên")]
        public IFormFile? UploadFile { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
