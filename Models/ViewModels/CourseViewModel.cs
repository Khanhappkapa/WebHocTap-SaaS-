using System.ComponentModel.DataAnnotations;
using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    public class CourseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên khóa học là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên khóa học tối đa 200 ký tự")]
        [Display(Name = "Tên khóa học")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Display(Name = "Trạng thái")]
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
    }
}
