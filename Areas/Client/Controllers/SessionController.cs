using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    public class SessionController : BaseClientController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SessionController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================================
        // Helper: Kiểm tra ownership — teacher hiện tại có sở hữu course không
        // ==========================================================
        private async Task<Course?> GetOwnedCourseAsync(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Courses.FirstOrDefaultAsync(
                c => c.Id == courseId && c.TeacherId == userId);
        }

        // --------------------------------------------------
        // TẠO BUỔI HỌC
        // --------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(int courseId)
        {
            var course = await GetOwnedCourseAsync(courseId);
            if (course == null) return Forbid(); // IDOR Protection

            var model = new SessionViewModel { CourseId = courseId };
            ViewBag.CourseTitle = course.Title;
            return View("CreateEdit", model);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(SessionViewModel model)
        {
            var course = await GetOwnedCourseAsync(model.CourseId);
            if (course == null) return Forbid(); // IDOR Protection

            // Server-side validation: EndTime phải sau StartTime
            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CourseTitle = course.Title;
                return View("CreateEdit", model);
            }

            var session = new Session
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                MeetingUrl = model.MeetingUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            // PRG Pattern: redirect về Details khóa học
            return RedirectToAction("Details", "Course", new { id = model.CourseId });
        }

        // --------------------------------------------------
        // SỬA BUỔI HỌC
        // --------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var session = await _context.Sessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();
            if (session.Course.TeacherId != userId) return Forbid(); // IDOR

            var model = new SessionViewModel
            {
                Id = session.Id,
                CourseId = session.CourseId,
                Title = session.Title,
                Description = session.Description,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                MeetingUrl = session.MeetingUrl
            };

            ViewBag.CourseTitle = session.Course.Title;
            return View("CreateEdit", model);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, SessionViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var session = await _context.Sessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();
            if (session.Course.TeacherId != userId) return Forbid(); // IDOR

            if (model.EndTime <= model.StartTime)
            {
                ModelState.AddModelError("EndTime", "Thời gian kết thúc phải sau thời gian bắt đầu.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CourseTitle = session.Course.Title;
                return View("CreateEdit", model);
            }

            session.Title = model.Title;
            session.Description = model.Description;
            session.StartTime = model.StartTime;
            session.EndTime = model.EndTime;
            session.MeetingUrl = model.MeetingUrl;

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Course", new { id = session.CourseId });
        }

        // --------------------------------------------------
        // XÓA BUỔI HỌC
        // --------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var session = await _context.Sessions
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (session == null) return NotFound();
            if (session.Course.TeacherId != userId) return Forbid(); // IDOR

            int courseId = session.CourseId;
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Course", new { id = courseId });
        }
    }
}
