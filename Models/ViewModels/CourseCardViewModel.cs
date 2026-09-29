using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    public class CourseCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public CourseStatus Status { get; set; }
        public int StudentCount { get; set; }
        public bool IsEnrolled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
