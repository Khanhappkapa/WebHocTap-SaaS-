// ============================================================
// File: Areas/Client/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Client (Teacher + Student)
// CHECKPOINT: Kế thừa BaseClientController → tự động có [Area("Client")] + [Authorize(Roles="Teacher,Student")]
// CHECKPOINT: Truy vấn DB thật, truyền DashboardViewModel cho View
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

            var isTeacher = user.UserRole == "Teacher";

            var vm = new DashboardViewModel
            {
                FullName = user.FullName ?? "bạn",
                IsTeacher = isTeacher
            };

            if (isTeacher)
            {
                // Teacher: lấy các khóa do mình sở hữu
                vm.Courses = await _context.Courses
                    .Where(c => c.TeacherId == user.Id)
                    .OrderByDescending(c => c.CreatedAt)
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

                // Todo: bài tập chưa chấm
                var ungradedAssignments = await _context.Assignments
                    .Where(a => a.Course.TeacherId == user.Id)
                    .Select(a => new
                    {
                        a.Title,
                        CourseName = a.Course.Title,
                        UngradedCount = a.Submissions.Count(s => s.Score == null)
                    })
                    .Where(a => a.UngradedCount > 0)
                    .Take(5)
                    .ToListAsync();

                foreach (var a in ungradedAssignments)
                {
                    vm.TodoItems.Add(new TodoItemViewModel
                    {
                        Title = $"Chấm bài: {a.Title}",
                        Meta = $"{a.UngradedCount} bài chờ · {a.CourseName}",
                        Icon = "bi-pencil-square"
                    });
                }
            }
            else
            {
                // Student: lấy các khóa đã enroll
                vm.Courses = await _context.Enrollments
                    .Where(e => e.StudentId == user.Id)
                    .OrderByDescending(e => e.EnrolledAt)
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

                // Todo: bài tập sắp đến hạn chưa nộp
                var pendingAssignments = await _context.Assignments
                    .Where(a => a.Course.Enrollments.Any(e => e.StudentId == user.Id))
                    .Where(a => a.Deadline > DateTime.UtcNow)
                    .Where(a => !a.Submissions.Any(s => s.StudentId == user.Id))
                    .OrderBy(a => a.Deadline)
                    .Take(5)
                    .Select(a => new
                    {
                        a.Title,
                        a.Deadline,
                        CourseName = a.Course.Title
                    })
                    .ToListAsync();

                foreach (var a in pendingAssignments)
                {
                    var remainHours = (a.Deadline - DateTime.UtcNow).TotalHours;
                    var meta = remainHours < 24
                        ? $"Sắp hết hạn · {a.CourseName}"
                        : $"Hạn: {a.Deadline:dd/MM} · {a.CourseName}";

                    vm.TodoItems.Add(new TodoItemViewModel
                    {
                        Title = $"Nộp bài: {a.Title}",
                        Meta = meta,
                        Icon = remainHours < 24 ? "bi-exclamation-triangle" : "bi-file-earmark-text"
                    });
                }
            }

            return View(vm);
        }
    }
}
