using System;
using System.Collections.Generic;
using System.Linq;

namespace WebHocTap_SaaS_.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang danh sách Bài tập (Assignment/Index)
    /// Hỗ trợ cả Giáo viên (quản lý bài tập theo khóa, phân loại bài nộp) và Học viên
    /// </summary>
    public class AssignmentIndexViewModel
    {
        public bool IsTeacher { get; set; }

        // Dành cho Giáo viên
        public List<TeacherAssignmentItemViewModel> TeacherAssignments { get; set; } = new();

        public int TotalAssignments => TeacherAssignments.Count;
        public int TotalSubmissions => TeacherAssignments.Sum(a => a.SubmittedCount);
        public int TotalUngraded => TeacherAssignments.Sum(a => a.UngradedSubmissions.Count);
        public int TotalGraded => TeacherAssignments.Sum(a => a.GradedSubmissions.Count);

        // Dành cho Học viên
        public List<StudentAssignmentItemViewModel> StudentAssignments { get; set; } = new();
    }

    public class TeacherAssignmentItemViewModel
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int MaxScore { get; set; }
        public DateTime CreatedAt { get; set; }

        public int TotalStudents { get; set; }
        public int SubmittedCount { get; set; }

        public bool IsOverdue => DateTime.UtcNow > Deadline;
        public bool IsDueSoon => !IsOverdue && (Deadline - DateTime.UtcNow).TotalHours <= 24;

        // 1. Những bài chưa chấm hiện lên trên
        public List<SubmissionItemViewModel> UngradedSubmissions { get; set; } = new();

        // 2. Những bài chấm rồi xuống dưới
        public List<SubmissionItemViewModel> GradedSubmissions { get; set; } = new();

        // 3. Còn lại là học sinh chưa nộp
        public List<UnsubmittedStudentViewModel> UnsubmittedStudents { get; set; } = new();
    }

    public class SubmissionItemViewModel
    {
        public int SubmissionId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string? FileName { get; set; }
        public double? Score { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
    }

    public class UnsubmittedStudentViewModel
    {
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
    }

    public class StudentAssignmentItemViewModel
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int MaxScore { get; set; }
        public bool HasSubmitted { get; set; }
        public double? Score { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? Feedback { get; set; }
        public bool IsOverdue => DateTime.UtcNow > Deadline;
        public bool IsDueSoon => !IsOverdue && (Deadline - DateTime.UtcNow).TotalHours <= 24;
    }
}
