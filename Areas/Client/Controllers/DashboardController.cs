// ============================================================
// File: Areas/Client/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Client (Teacher + Student) - Phần 8
// CHECKPOINT: DB-side COUNT, AsNoTracking(), IDOR-locked ToDo widget, 0 migration
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    public class DashboardController : BaseClientController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

            var userId = user.Id;
            var isTeacher = user.UserRole == "Teacher";
            var role = isTeacher ? "Teacher" : "Student";
            var now = DateTime.UtcNow;

            var vm = new ClientDashboardViewModel
            {
                Role = role,
                FullName = user.FullName ?? "bạn"
            };

            if (isTeacher)
            {
                // ==========================================
                // STAT - TEACHER (DB-side COUNT độc lập)
                // ==========================================
                vm.CourseCount = await _context.Courses
                    .AsNoTracking()
                    .Where(c => c.TeacherId == userId)
                    .CountAsync();

                vm.StudentCount = await _context.Enrollments
                    .AsNoTracking()
                    .Where(e => e.Course.TeacherId == userId)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .CountAsync();

                vm.AssignmentCount = await _context.Assignments
                    .AsNoTracking()
                    .Where(a => a.Course.TeacherId == userId)
                    .CountAsync();

                vm.UngradedCount = await _context.Submissions
                    .AsNoTracking()
                    .Where(s => s.Score == null && s.Assignment.Course.TeacherId == userId)
                    .CountAsync();

                vm.UpcomingSessionCount = await _context.Sessions
                    .AsNoTracking()
                    .Where(s => s.Course.TeacherId == userId && s.StartTime > now)
                    .CountAsync();

                // Khóa học gần đây của giáo viên
                vm.Courses = await _context.Courses
                    .AsNoTracking()
                    .Where(c => c.TeacherId == userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(6)
                    .Select(c => new CourseCardViewModel
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Description = c.Description,
                        TeacherName = user.FullName ?? "",
                        Status = c.Status,
                        StudentCount = c.Enrollments.Count,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();
            }
            else
            {
                // ==========================================
                // STAT - STUDENT (DB-side COUNT độc lập)
                // ==========================================
                vm.EnrolledCount = await _context.Enrollments
                    .AsNoTracking()
                    .Where(e => e.StudentId == userId)
                    .CountAsync();

                vm.PendingSubmissionCount = await _context.Assignments
                    .AsNoTracking()
                    .Where(a => a.Course.Enrollments.Any(e => e.StudentId == userId)
                             && a.Deadline > now
                             && !_context.Submissions.Any(s => s.AssignmentId == a.Id && s.StudentId == userId))
                    .CountAsync();

                vm.GradedCount = await _context.Submissions
                    .AsNoTracking()
                    .Where(s => s.StudentId == userId && s.Score != null)
                    .CountAsync();

                // Tính điểm trung bình (guard divide-by-zero)
                var gradedQuery = _context.Submissions
                    .AsNoTracking()
                    .Where(s => s.StudentId == userId && s.Score != null);

                if (await gradedQuery.AnyAsync())
                {
                    var avg = await gradedQuery.AverageAsync(s => s.Score!.Value);
                    vm.AverageScore = Math.Round(avg, 1);
                }
                else
                {
                    vm.AverageScore = 0.0;
                }

                // Khóa học sinh đã đăng ký
                vm.Courses = await _context.Enrollments
                    .AsNoTracking()
                    .Where(e => e.StudentId == userId)
                    .OrderByDescending(e => e.EnrolledAt)
                    .Take(6)
                    .Select(e => new CourseCardViewModel
                    {
                        Id = e.Course.Id,
                        Title = e.Course.Title,
                        Description = e.Course.Description,
                        TeacherName = e.Course.Teacher.FullName ?? "",
                        Status = e.Course.Status,
                        StudentCount = e.Course.Enrollments.Count,
                        CreatedAt = e.Course.CreatedAt
                    })
                    .ToListAsync();
            }

            // ==========================================
            // WIDGET TO-DO CROSS-COURSE (SIẾT IDOR THEO userId)
            // ==========================================
            // 1. Buổi học sắp tới (cả 2 role, gán URL chi tiết khóa học)
            var sessionQuery = _context.Sessions
                .AsNoTracking()
                .Where(s => s.StartTime > now)
                .Where(s => isTeacher
                    ? s.Course.TeacherId == userId
                    : s.Course.Enrollments.Any(e => e.StudentId == userId));

            var upcomingSessions = await sessionQuery
                .OrderBy(s => s.StartTime)
                .Take(3)
                .Select(s => new ToDoItem
                {
                    Kind = ToDoKind.Session,
                    Title = s.Title,
                    CourseTitle = s.Course.Title,
                    WhenUtc = DateTime.SpecifyKind(s.StartTime, DateTimeKind.Utc),
                    IsOverdueSoon = false,
                    ActionUrl = Url.Action("Details", "Course", new { area = "Client", id = s.CourseId }) ?? ""
                })
                .ToListAsync();

            var todoList = new List<ToDoItem>(upcomingSessions);

            if (isTeacher)
            {
                // 2. Bài tập có bài nộp chưa chấm (CHỈ giáo viên sở hữu khóa học)
                var toGradeSubmissions = await _context.Submissions
                    .AsNoTracking()
                    .Where(s => s.Score == null)
                    .Where(s => s.Assignment.Course.TeacherId == userId) // IDOR lock
                    .OrderBy(s => s.Assignment.Deadline)
                    .Take(3)
                    .Select(s => new ToDoItem
                    {
                        Kind = ToDoKind.SubmissionToGrade,
                        Title = s.Assignment.Title,
                        CourseTitle = s.Assignment.Course.Title,
                        WhenUtc = DateTime.SpecifyKind(s.Assignment.Deadline, DateTimeKind.Utc),
                        IsOverdueSoon = s.Assignment.Deadline < now.AddHours(24),
                        ActionUrl = Url.Action("Details", "Assignment", new { area = "Client", id = s.AssignmentId }) ?? ""
                    })
                    .ToListAsync();

                todoList.AddRange(toGradeSubmissions);
            }
            else
            {
                // 3. Bài tập sắp tới hạn chưa nộp (CHỈ học viên đã đăng ký khóa)
                var dueAssignments = await _context.Assignments
                    .AsNoTracking()
                    .Where(a => a.Deadline > now)
                    .Where(a => a.Course.Enrollments.Any(e => e.StudentId == userId)) // IDOR lock
                    .Where(a => !_context.Submissions.Any(s => s.AssignmentId == a.Id && s.StudentId == userId))
                    .OrderBy(a => a.Deadline)
                    .Take(3)
                    .Select(a => new ToDoItem
                    {
                        Kind = ToDoKind.AssignmentDue,
                        Title = a.Title,
                        CourseTitle = a.Course.Title,
                        WhenUtc = DateTime.SpecifyKind(a.Deadline, DateTimeKind.Utc),
                        IsOverdueSoon = a.Deadline < now.AddHours(24),
                        ActionUrl = Url.Action("Details", "Assignment", new { area = "Client", id = a.Id }) ?? ""
                    })
                    .ToListAsync();

                todoList.AddRange(dueAssignments);
            }

            // Gộp và sắp xếp theo thời gian sớm nhất, lấy tối đa 5 việc
            vm.ToDo = todoList.OrderBy(t => t.WhenUtc).Take(5).ToList();

            return View(vm);
        }
    }
}