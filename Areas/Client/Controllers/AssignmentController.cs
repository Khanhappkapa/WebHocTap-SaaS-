using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;
using WebHocTap_SaaS_.Utils;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    public class AssignmentController : BaseClientController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AssignmentController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --------------------------------------------------
        // TẠO BÀI TẬP (GET) — Teacher only
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Create(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == userId);
            if (course == null) return Forbid();

            var vm = new AssignmentCreateViewModel
            {
                CourseId = courseId,
                CourseName = course.Title,
                Deadline = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Utc)
            };
            return View("CreateEdit", vm);
        }

        // --------------------------------------------------
        // TẠO BÀI TẬP (POST) — PRG
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int courseId, AssignmentCreateViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.TeacherId == userId);
            if (course == null) return Forbid();

            vm.CourseId = courseId;
            vm.CourseName = course.Title;

            if (!ModelState.IsValid)
                return View("CreateEdit", vm);

            var assignment = new Assignment
            {
                CourseId = courseId,
                Title = vm.Title,
                Description = vm.Description,
                Deadline = DateTime.SpecifyKind(vm.Deadline, DateTimeKind.Utc),
                MaxScore = vm.MaxScore,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo bài tập thành công!";
            return RedirectToAction("Details", new { id = assignment.Id });
        }

        // --------------------------------------------------
        // SỬA BÀI TẬP (GET) — Teacher only
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments.AsNoTracking()
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id && a.Course.TeacherId == userId);
            if (assignment == null) return Forbid();

            var vm = new AssignmentCreateViewModel
            {
                Id = assignment.Id,
                CourseId = assignment.CourseId,
                CourseName = assignment.Course.Title,
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                MaxScore = assignment.MaxScore
            };
            return View("CreateEdit", vm);
        }

        // --------------------------------------------------
        // SỬA BÀI TẬP (POST) — PRG
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssignmentCreateViewModel vm)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id && a.Course.TeacherId == userId);
            if (assignment == null) return Forbid();

            vm.Id = id;
            vm.CourseId = assignment.CourseId;
            vm.CourseName = assignment.Course.Title;

            if (!ModelState.IsValid)
                return View("CreateEdit", vm);

            assignment.Title = vm.Title;
            assignment.Description = vm.Description;
            assignment.Deadline = DateTime.SpecifyKind(vm.Deadline, DateTimeKind.Utc);
            assignment.MaxScore = vm.MaxScore;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật bài tập!";
            return RedirectToAction("Details", new { id });
        }

        // --------------------------------------------------
        // XEM CHI TIẾT BÀI TẬP — 5 nhánh theo role/trạng thái
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var assignment = await _context.Assignments
                .AsNoTracking()
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null) return NotFound();

            bool isTeacher = User.IsInRole("Teacher");
            bool isOwner = isTeacher && assignment.Course.TeacherId == userId;

            bool isEnrolled = false;
            if (!isTeacher)
            {
                isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.CourseId == assignment.CourseId && e.StudentId == userId);
            }

            // Nhánh 1 — chưa enroll: redirect về Course/Details
            if (!isOwner && !isEnrolled)
                return RedirectToAction("Details", "Course", new { id = assignment.CourseId });

            var model = new AssignmentDetailsViewModel
            {
                Assignment = assignment,
                IsOwner = isOwner,
                IsEnrolled = isEnrolled,
                TotalStudents = await _context.Enrollments.CountAsync(e => e.CourseId == assignment.CourseId)
            };

            if (isOwner)
            {
                // Teacher: load MỌI submissions + student info
                model.Submissions = await _context.Submissions
                    .AsNoTracking()
                    .Include(s => s.Student)
                    .Where(s => s.AssignmentId == id)
                    .OrderByDescending(s => s.SubmittedAt)
                    .ToListAsync();
                model.SubmittedCount = model.Submissions.Count;
            }
            else
            {
                // Student: load bài nộp của chính mình
                model.Submission = await _context.Submissions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == userId);
                model.SubmittedCount = await _context.Submissions
                    .CountAsync(s => s.AssignmentId == id);
            }

            return View(model);
        }

        // --------------------------------------------------
        // NỘP BÀI — Student enrolled, PRG + FileValidation + unique
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, IFormFile? File, string? StudentNote)
        {
            var userId = _userManager.GetUserId(User);

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (assignment == null) return NotFound();

            // Kiểm quyền: enrolled
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.CourseId == assignment.CourseId && e.StudentId == userId);
            if (!isEnrolled) return Forbid();

            // Kiểm deadline
            if (DateTime.UtcNow > assignment.Deadline)
            {
                TempData["Error"] = "Đã quá hạn nộp bài!";
                return RedirectToAction("Details", new { id });
            }

            // Kiểm trùng (lớp 2)
            var existing = await _context.Submissions
                .AnyAsync(s => s.AssignmentId == id && s.StudentId == userId);
            if (existing)
            {
                TempData["Error"] = "Bạn đã nộp bài rồi!";
                return RedirectToAction("Details", new { id });
            }

            // Validate file (tái dùng FileValidation P5)
            var (ok, error) = FileValidation.Validate(File);
            if (!ok)
            {
                TempData["Error"] = error;
                return RedirectToAction("Details", new { id });
            }

            // Đọc file vào bytea
            byte[] fileData;
            using (var ms = new MemoryStream())
            {
                await File!.CopyToAsync(ms);
                fileData = ms.ToArray();
            }

            var submission = new Submission
            {
                AssignmentId = id,
                StudentId = userId!,
                Content = StudentNote,
                FileName = Guid.NewGuid().ToString("N") + Path.GetExtension(File.FileName).ToLowerInvariant(),
                ContentType = File.ContentType,
                FileData = fileData,
                SubmittedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };

            try
            {
                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Nộp bài thành công!";
            }
            catch (DbUpdateException)
            {
                // Lớp 3: unique constraint tại DB
                TempData["Error"] = "Bạn đã nộp bài rồi!";
            }

            return RedirectToAction("Details", new { id });
        }

        // --------------------------------------------------
        // CHẤM ĐIỂM — Teacher, AJAX + fallback PRG
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(GradeViewModel vm)
        {
            var userId = _userManager.GetUserId(User);

            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(s => s.Id == vm.SubmissionId);

            if (submission == null) return NotFound();

            // IDOR check: chỉ teacher của khóa mới có quyền chấm
            if (submission.Assignment.Course.TeacherId != userId)
                return Forbid();

            // Validate score
            if (vm.Score < 0 || vm.Score > submission.Assignment.MaxScore)
            {
                var errorMsg = $"Điểm phải từ 0 đến {submission.Assignment.MaxScore}";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { ok = false, error = errorMsg });
                TempData["Error"] = errorMsg;
                return RedirectToAction("Details", new { id = submission.AssignmentId });
            }

            submission.Score = vm.Score;
            submission.Feedback = vm.Feedback;
            submission.GradedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            await _context.SaveChangesAsync();

            // AJAX response
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { ok = true, score = vm.Score, status = "Đã chấm" });

            // Fallback PRG
            TempData["Success"] = "Đã lưu điểm!";
            return RedirectToAction("Details", new { id = submission.AssignmentId });
        }

        // --------------------------------------------------
        // TẢI BÀI NỘP — kiểm quyền kép (owner + student bài mình)
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var userId = _userManager.GetUserId(User);

            var submission = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (submission == null || submission.FileData == null)
                return NotFound();

            bool isOwner = submission.Assignment.Course.TeacherId == userId;
            bool isOwnSubmission = submission.StudentId == userId;

            if (!isOwner && !isOwnSubmission)
                return Forbid();

            return File(submission.FileData, submission.ContentType ?? "application/octet-stream", submission.FileName);
        }

        // --------------------------------------------------
        // XÓA BÀI TẬP — Teacher only, PRG
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id && a.Course.TeacherId == userId);

            if (assignment == null) return Forbid();

            var courseId = assignment.CourseId;
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa bài tập!";
            return RedirectToAction("Details", "Course", new { id = courseId });
        }
    }
}
