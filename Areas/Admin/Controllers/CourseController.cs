// ============================================================
// File: Areas/Admin/Controllers/CourseController.cs
// Mô tả: Admin quản lý khóa học (P7)
// CHECKPOINT: Kế thừa BaseAdminController → tự có [Area("Admin")] + [Authorize(Roles = "Admin")]
// Hành động: Index (danh sách), Close (đóng khóa), ToggleHidden (ẩn/hiện)
// KHÔNG hard-delete: chỉ đổi Status/IsHidden (soft-delete)
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Areas.Admin.Controllers
{
    public class CourseController : BaseAdminController
    {
        private readonly ApplicationDbContext _db;

        public CourseController(ApplicationDbContext db)
        {
            _db = db;
        }

        // --------------------------------------------------
        // GET: /Admin/Course
        // Danh sách toàn bộ khóa học, hỗ trợ search + filter status + filter teacher
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? status, string? teacher, int page = 1)
        {
            // Thống kê toàn hệ thống
            var totalCount = await _db.Courses.CountAsync();
            var hiddenCount = await _db.Courses.CountAsync(c => c.IsHidden);

            // Danh sách giáo viên để hiển thị filter
            var teachers = await _db.Courses
                .Include(c => c.Teacher)
                .Select(c => c.Teacher.FullName)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var query = _db.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .AsQueryable();

            // Tìm kiếm theo tên khóa học hoặc tên giáo viên
            if (!string.IsNullOrWhiteSpace(q))
            {
                var search = q.Trim().ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(search)
                                      || c.Teacher.FullName.ToLower().Contains(search));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CourseStatus>(status, out var s))
            {
                query = query.Where(c => c.Status == s);
            }

            // Lọc theo giáo viên
            if (!string.IsNullOrWhiteSpace(teacher))
            {
                query = query.Where(c => c.Teacher.FullName == teacher);
            }

            var courses = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            var vm = new AdminCourseIndexViewModel
            {
                SearchQuery = q,
                StatusFilter = status,
                TeacherFilter = teacher,
                Teachers = teachers,
                TotalCount = totalCount,
                HiddenCount = hiddenCount,
                CurrentPage = page < 1 ? 1 : page,
                PageSize = 10,
                Courses = courses.Select(c => new AdminCourseRowViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    TeacherName = c.Teacher.FullName,
                    Status = c.Status,
                    IsHidden = c.IsHidden,
                    EnrollmentCount = c.Enrollments.Count,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return View(vm);
        }

        // --------------------------------------------------
        // POST: /Admin/Course/Close/{id}
        // Đóng khóa học (Status = Closed)
        // CHECKPOINT: Chỉ đóng, KHÔNG xóa (soft-delete)
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction(nameof(Index));
            }

            if (course.Status == CourseStatus.Closed)
            {
                TempData["Error"] = "Khóa học đã đóng rồi.";
                return RedirectToAction(nameof(Index));
            }

            course.Status = CourseStatus.Closed;
            course.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã đóng khóa học “{course.Title}”.";
            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------
        // POST: /Admin/Course/ToggleHidden/{id}
        // Ẩn / hiện khóa học (IsHidden = !IsHidden)
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHidden(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null)
            {
                TempData["Error"] = "Không tìm thấy khóa học.";
                return RedirectToAction(nameof(Index));
            }

            course.IsHidden = !course.IsHidden;
            course.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = course.IsHidden
                ? $"Đã ẩn khóa học “{course.Title}”."
                : $"Đã hiện lại khóa học “{course.Title}”.";

            return RedirectToAction(nameof(Index));
        }
    }
}