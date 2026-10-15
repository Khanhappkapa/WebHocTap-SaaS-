// ============================================================
// File: Models/ViewModels/AdminCourseViewModel.cs
// Mô tả: ViewModel cho Admin quản lý khóa học (P7)
// ============================================================

namespace WebHocTap_SaaS_.Models.ViewModels
{
    /// <summary>
    /// Hiển thị 1 dòng/card trong danh sách Course (Admin)
    /// </summary>
    public class AdminCourseRowViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public CourseStatus Status { get; set; }
        public bool IsHidden { get; set; }
        public int EnrollmentCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// ViewModel tổng cho trang Admin/Course/Index
    /// </summary>
    public class AdminCourseIndexViewModel
    {
        public List<AdminCourseRowViewModel> Courses { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? StatusFilter { get; set; }
        public string? TeacherFilter { get; set; }
        public List<string> Teachers { get; set; } = new();

        public int TotalCount { get; set; }
        public int HiddenCount { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
    }
}