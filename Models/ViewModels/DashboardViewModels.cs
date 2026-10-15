// ============================================================
// File: Models/ViewModels/DashboardViewModels.cs
// Mô tả: ViewModels cho Dashboard Client & Admin (P8)
// CHECKPOINT: 0 migration — chỉ đọc, số liệu tính toán từ DB
// ============================================================

namespace WebHocTap_SaaS_.Models.ViewModels
{
    /// <summary>
    /// ViewModel Dashboard Client (dùng chung cho Teacher và Student)
    /// </summary>
    public class ClientDashboardViewModel
    {
        public string Role { get; set; } = string.Empty; // "Teacher" | "Student"
        public bool IsTeacher => Role == "Teacher";
        public string FullName { get; set; } = "bạn";

        // Stat — Teacher (DB-side COUNT)
        public int CourseCount { get; set; }
        public int StudentCount { get; set; }
        public int AssignmentCount { get; set; }
        public int UngradedCount { get; set; }
        public int UpcomingSessionCount { get; set; }

        // Stat — Student (DB-side COUNT)
        public int EnrolledCount { get; set; }
        public int PendingSubmissionCount { get; set; }
        public int GradedCount { get; set; }
        public double AverageScore { get; set; } // 0 nếu chưa có điểm

        // Danh sách khóa học gần đây (cột trái)
        public List<CourseCardViewModel> Courses { get; set; } = new();

        // Widget to-do cross-course (cả 2 role, gộp và sắp xếp theo WhenUtc, Take 5)
        public List<ToDoItem> ToDo { get; set; } = new();
    }

    /// <summary>
    /// 1 mục trong widget việc cần làm cross-course
    /// </summary>
    public class ToDoItem
    {
        public ToDoKind Kind { get; set; } // Session | AssignmentDue | SubmissionToGrade
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime WhenUtc { get; set; } // StartTime (session) / Deadline (assignment)
        public string ActionUrl { get; set; } = string.Empty; // link tới Course/Details hoặc Assignment/Details
        public bool IsOverdueSoon { get; set; } // deadline < 24h → pill đỏ
    }

    public enum ToDoKind
    {
        Session,
        AssignmentDue,
        SubmissionToGrade
    }

    /// <summary>
    /// ViewModel Dashboard Admin (đếm toàn hệ thống)
    /// </summary>
    public class AdminDashboardViewModel
    {
        // Người dùng
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int LockedUsers { get; set; }

        // Khóa học
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int DraftCourses { get; set; }
        public int ClosedCourses { get; set; }
        public int HiddenCourses { get; set; }

        // Hoạt động
        public int TotalEnrollments { get; set; }
        public int TotalSubmissions { get; set; }
        public int UngradedSubmissions { get; set; }
    }
}