// ============================================================
// File: Areas/Admin/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Admin
// CHECKPOINT: Kế thừa BaseAdminController → tự động có [Area("Admin")] + [Authorize(Roles="Admin")]
// CHECKPOINT: Truy vấn DB thật, truyền ViewBag stats
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;

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
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.CourseCount = await _context.Courses.CountAsync();
            ViewBag.AssignmentCount = await _context.Assignments.CountAsync();
            ViewBag.SubmissionCount = await _context.Submissions.CountAsync();
            return View();
        }
    }
}
