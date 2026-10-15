// ============================================================
// File: Areas/Admin/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Admin - Phần 8
// CHECKPOINT: Đếm toàn hệ thống theo thiết kế, AsNoTracking(), DB-side COUNT
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;
            var vm = new AdminDashboardViewModel();

            // ==========================================
            // THỐNG KÊ NGƯỜI DÙNG (DB-side COUNT)
            // ==========================================
            vm.TotalUsers = await _context.Users
                .AsNoTracking()
                .CountAsync();

            vm.ActiveUsers = await _context.Users
                .AsNoTracking()
                .Where(u => u.IsActive)
                .CountAsync();

            vm.InactiveUsers = await _context.Users
                .AsNoTracking()
                .Where(u => !u.IsActive)
                .CountAsync();

            vm.LockedUsers = await _context.Users
                .AsNoTracking()
                .Where(u => u.LockoutEnd != null && u.LockoutEnd > now)
                .CountAsync();

            // ==========================================
            // THỐNG KÊ KHÓA HỌC (DB-side COUNT)
            // ==========================================
            vm.TotalCourses = await _context.Courses
                .AsNoTracking()
                .CountAsync();

            vm.PublishedCourses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.Status == CourseStatus.Published)
                .CountAsync();

            vm.DraftCourses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.Status == CourseStatus.Draft)
                .CountAsync();

            vm.ClosedCourses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.Status == CourseStatus.Closed)
                .CountAsync();

            vm.HiddenCourses = 0;

            // ==========================================
            // THỐNG KÊ HOẠT ĐỘNG (DB-side COUNT)
            // ==========================================
            vm.TotalEnrollments = await _context.Enrollments
                .AsNoTracking()
                .CountAsync();

            vm.TotalSubmissions = await _context.Submissions
                .AsNoTracking()
                .CountAsync();

            vm.UngradedSubmissions = await _context.Submissions
                .AsNoTracking()
                .Where(s => s.Score == null)
                .CountAsync();

            return View(vm);
        }
    }
}