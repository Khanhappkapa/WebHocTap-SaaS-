namespace WebHocTap_SaaS_.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Chi tiết Bài tập (P6)
    /// Phục vụ 5 nhánh hiển thị: locked / student-upload / student-awaiting / student-graded / teacher-grading
    /// </summary>
    public class AssignmentDetailsViewModel
    {
        public Assignment Assignment { get; set; } = null!;
        public bool IsOwner { get; set; }
        public bool IsEnrolled { get; set; }

        /// <summary>Submission của chính học viên (null = chưa nộp). Chỉ load khi là Student.</summary>
        public Submission? Submission { get; set; }

        /// <summary>Toàn bộ submissions (Teacher grading view). Chỉ load khi IsOwner.</summary>
        public List<Submission> Submissions { get; set; } = new();

        /// <summary>Số bài đã nộp</summary>
        public int SubmittedCount { get; set; }

        /// <summary>Tổng số HV trong khóa</summary>
        public int TotalStudents { get; set; }

        /// <summary>Số bài chưa chấm (Score == null)</summary>
        public int UngradedCount => Submissions.Count(s => s.Score == null);
    }

    /// <summary>
    /// ViewModel cho form chấm điểm (AJAX + fallback PRG)
    /// </summary>
    public class GradeViewModel
    {
        public int SubmissionId { get; set; }
        public double Score { get; set; }
        public string? Feedback { get; set; }
    }
}
