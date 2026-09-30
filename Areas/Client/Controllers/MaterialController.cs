using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;
using WebHocTap_SaaS_.Utils;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    public class MaterialController : BaseClientController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public MaterialController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================================
        // Helper: Kiểm tra ownership
        // ==========================================================
        private async Task<Course?> GetOwnedCourseAsync(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            return await _context.Courses.FirstOrDefaultAsync(
                c => c.Id == courseId && c.TeacherId == userId);
        }

        // --------------------------------------------------
        // FORM TẠO TÀI LIỆU
        // --------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(int courseId)
        {
            var course = await GetOwnedCourseAsync(courseId);
            if (course == null) return Forbid(); // IDOR

            ViewBag.CourseTitle = course.Title;
            return View(new MaterialCreateViewModel { CourseId = courseId });
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(MaterialCreateViewModel model)
        {
            var course = await GetOwnedCourseAsync(model.CourseId);
            if (course == null) return Forbid(); // IDOR

            var material = new Material
            {
                CourseId = model.CourseId,
                Title = model.Title,
                FileType = model.FileType,
                UploadedAt = DateTime.UtcNow
            };

            if (model.FileType == FileType.Link)
            {
                // Link: bắt buộc có URL
                if (string.IsNullOrWhiteSpace(model.FileUrl))
                {
                    ModelState.AddModelError("FileUrl", "Vui lòng nhập URL khi chọn loại Link.");
                }
                else
                {
                    material.FileUrl = model.FileUrl;
                }
            }
            else
            {
                // Upload file: validate bằng FileValidation utility
                var (ok, error) = FileValidation.Validate(model.UploadFile);
                if (!ok)
                {
                    ModelState.AddModelError("UploadFile", error);
                }
                else
                {
                    var ext = Path.GetExtension(model.UploadFile!.FileName).ToLowerInvariant();
                    material.FileName = Guid.NewGuid().ToString() + ext; // Guid rename
                    material.ContentType = model.UploadFile.ContentType;

                    // Đọc file vào byte[] lưu DB (bytea)
                    using var ms = new MemoryStream();
                    await model.UploadFile.CopyToAsync(ms);
                    material.FileData = ms.ToArray();
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CourseTitle = course.Title;
                return View(model);
            }

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Course", new { id = model.CourseId });
        }

        // --------------------------------------------------
        // XÓA TÀI LIỆU
        // --------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var material = await _context.Materials
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();
            if (material.Course.TeacherId != userId) return Forbid(); // IDOR

            int courseId = material.CourseId;
            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Course", new { id = courseId });
        }

        // --------------------------------------------------
        // TẢI XUỐNG TÀI LIỆU (Kiểm tra quyền: teacher owner HOẶC student đã enroll)
        // CHECKPOINT: KHÔNG có URL công khai — bắt buộc qua action này
        // --------------------------------------------------
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var userId = _userManager.GetUserId(User);
            var material = await _context.Materials
                .AsNoTracking()
                .Include(m => m.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (material == null) return NotFound();

            // Kiểm tra quyền: teacher chủ khóa HOẶC student đã enroll
            bool isOwner = material.Course.TeacherId == userId;
            bool isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == material.CourseId && e.StudentId == userId);

            if (!isOwner && !isEnrolled)
                return Forbid();

            // Nếu FileType == Link → redirect tới URL
            if (material.FileType == FileType.Link && !string.IsNullOrEmpty(material.FileUrl))
            {
                return Redirect(material.FileUrl);
            }

            // Nếu có file data → stream về
            if (material.FileData != null && material.FileData.Length > 0)
            {
                return File(material.FileData, 
                    material.ContentType ?? "application/octet-stream", 
                    material.FileName ?? "download");
            }

            return NotFound();
        }
    }
}
