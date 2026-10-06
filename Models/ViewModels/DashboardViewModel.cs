using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    public class DashboardViewModel
    {
        public string FullName { get; set; } = "bạn";
        public bool IsTeacher { get; set; }

        // Courses dùng chung cho cả Teacher (owned) và Student (enrolled)
        public List<CourseCardViewModel> Courses { get; set; } = new();

        // Todo items (upcoming sessions + pending assignments)
        public List<TodoItemViewModel> TodoItems { get; set; } = new();
    }

    public class TodoItemViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-calendar-event";
    }
}
