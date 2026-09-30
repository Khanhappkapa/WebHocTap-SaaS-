using System.ComponentModel.DataAnnotations;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    public class SessionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề buổi học là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc")]
        [Display(Name = "Bắt đầu")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc")]
        [Display(Name = "Kết thúc")]
        public DateTime EndTime { get; set; }

        [Url(ErrorMessage = "Link họp phải là URL hợp lệ")]
        [StringLength(500)]
        [Display(Name = "Link họp (Zoom/Meet)")]
        public string? MeetingUrl { get; set; }

        [Required]
        public int CourseId { get; set; }
    }
}
