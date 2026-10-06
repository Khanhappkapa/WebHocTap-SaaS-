using System.ComponentModel.DataAnnotations;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho form Tạo / Sửa bài tập (P6)
    /// </summary>
    public class AssignmentCreateViewModel
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }

        /// <summary>Create vs Edit</summary>
        public bool IsEdit => Id.HasValue;

        [Required(ErrorMessage = "Vui lòng không để trống")]
        [StringLength(200, ErrorMessage = "Tiêu đề tối đa 200 ký tự")]
        [Display(Name = "Tiêu đề bài tập")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự")]
        [Display(Name = "Nội dung đề bài")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Hạn nộp là bắt buộc")]
        [Display(Name = "Hạn nộp")]
        public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required(ErrorMessage = "Điểm tối đa là bắt buộc")]
        [Range(1, 10000, ErrorMessage = "Điểm tối đa phải >= 1")]
        [Display(Name = "Điểm tối đa")]
        public int MaxScore { get; set; } = 100;
    }
}
