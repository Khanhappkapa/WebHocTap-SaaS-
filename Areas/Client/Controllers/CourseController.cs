using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    public class CourseController : BaseClientController
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --------------------------------------------------
        // CATALOG (Dành cho Student)
        // Hỗ trợ AJAX phân trang và tìm kiếm (không reload trang)
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            int pageSize = 6;
            
            // Xây dựng query: Chỉ lấy course Published
            var query = _context.Courses
                .AsNoTracking()
                .Include(c => c.Teacher)
                .Where(c => c.Status == CourseStatus.Published)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                // Tìm kiếm theo Title không phân biệt hoa thường (ILIKE trong Postgres)
                query = query.Where(c => c.Title.ToLower().Contains(search.ToLower()));
            }

            // Đếm tổng số để tính phân trang
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Lấy danh sách khóa đã enroll của học sinh này (chỉ ID)
            var enrolledCourseIds = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == userId!)
                .Select(e => e.CourseId)
                .ToListAsync();

            // Pagination query
            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    TeacherName = c.Teacher.FullName,
                    Status = c.Status,
                    StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id), // Count from Enrollments
                    IsEnrolled = enrolledCourseIds.Contains(c.Id),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            var model = new CourseListViewModel
            {
                Items = courses,
                Search = search ?? "",
                Page = page,
                TotalPages = totalPages,
                HasPrevious = page > 1,
                HasNext = page < totalPages
            };

            // CHECKPOINT: AJAX DETECTION
            // Nếu header báo fetch qua AJAX, trả về PartialView
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax)
            {
                return PartialView("_CourseCards", model);
            }

            return View(model);
        }

        // --------------------------------------------------
        // KHÓA HỌC CỦA TÔI (Student)
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);

            var courses = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == userId)
                .Include(e => e.Course)
                .ThenInclude(c => c.Teacher)
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => new CourseCardViewModel
                {
                    Id = e.Course.Id,
                    Title = e.Course.Title,
                    Description = e.Course.Description,
                    TeacherName = e.Course.Teacher.FullName,
                    Status = e.Course.Status,
                    IsEnrolled = true,
                    CreatedAt = e.Course.CreatedAt
                })
                .ToListAsync();

            return View(courses);
        }

        // --------------------------------------------------
        // ĐĂNG KÝ KHÓA HỌC (AJAX POST cho Student)
        // --------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken] // Tùy chọn (nếu filter chưa global, nhưng giờ là global)
        public async Task<IActionResult> Enroll(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Kiểm tra course tồn tại và phải là Published
            // Lấy từ DB luôn không cần AsNoTracking vì không thay đổi Course, nhưng thay đổi Enrollment
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null || course.Status != CourseStatus.Published)
            {
                return Json(new { success = false, message = "Khóa học không khả dụng." });
            }

            // Kiểm tra lớp bảo vệ 1 (UX check)
            bool isEnrolled = await _context.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.CourseId == id && e.StudentId == userId);
            
            if (isEnrolled)
            {
                return Json(new { success = false, message = "Bạn đã đăng ký khóa học này trước đó." });
            }

            var enrollment = new Enrollment
            {
                CourseId = id,
                StudentId = userId!,
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);

            try
            {
                await _context.SaveChangesAsync();
                int studentCount = await _context.Enrollments.CountAsync(e => e.CourseId == id);
                return Json(new { success = true, message = "Đăng ký thành công!", data = studentCount });
            }
            catch (DbUpdateException)
            {
                // Lớp bảo vệ 2: Database Unique Constraint Race Condition
                return Json(new { success = false, message = "Lỗi xử lý đăng ký (có thể do đăng ký trùng lặp đồng thời)." });
            }
        }

        // --------------------------------------------------
        // XEM CHI TIẾT KHÓA HỌC (PHẦN 5: Mở rộng với Sessions + Materials)
        // CHECKPOINT: IDOR Visibility Check
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);

            var course = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            bool isTeacher = User.IsInRole("Teacher");
            bool isOwner = isTeacher && course.TeacherId == userId;
            bool isEnrolled = false;

            // Xử lý quyền xem (IDOR Protection)
            if (isTeacher)
            {
                if (course.TeacherId != userId)
                    return Forbid();
            }
            else
            {
                isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.CourseId == id && e.StudentId == userId);

                if (course.Status != CourseStatus.Published && !isEnrolled)
                {
                    return Forbid();
                }
            }

            // Load sessions + materials nếu có quyền xem nội dung
            var sessions = new List<Models.Session>();
            var materials = new List<Models.Material>();

            if (isOwner || isEnrolled)
            {
                sessions = await _context.Sessions
                    .AsNoTracking()
                    .Where(s => s.CourseId == id)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();

                materials = await _context.Materials
                    .AsNoTracking()
                    .Where(m => m.CourseId == id)
                    .OrderByDescending(m => m.UploadedAt)
                    .ToListAsync();
            }

            var model = new CourseDetailsViewModel
            {
                Course = course,
                Sessions = sessions,
                Materials = materials,
                IsEnrolled = isEnrolled,
                IsOwner = isOwner,
                StudentCount = await _context.Enrollments.CountAsync(e => e.CourseId == id)
            };

            return View(model);
        }

        // ==========================================================
        // KHU VỰC DÀNH CHO TEACHER (CRUD KHÓA HỌC)
        // ==========================================================

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Manage()
        {
            var userId = _userManager.GetUserId(User);

            // Bắt buộc lọc theo TeacherId == userId (Chống IDOR)
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.TeacherId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CourseCardViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    StudentCount = _context.Enrollments.Count(e => e.CourseId == c.Id)
                })
                .ToListAsync();

            return View(courses);
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public IActionResult Create()
        {
            return View("CreateEdit", new CourseViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(CourseViewModel model)
        {
            if (!ModelState.IsValid)
                return View("CreateEdit", model);

            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                Status = model.Status,
                TeacherId = _userManager.GetUserId(User)!, // Gán cứng TeacherId là current user
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // CHECKPOINT: IDOR Protection — Edit Ownership check
            if (course.TeacherId != userId) return Forbid();

            var model = new CourseViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Status = course.Status
            };

            return View("CreateEdit", model);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id, CourseViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View("CreateEdit", model);

            var userId = _userManager.GetUserId(User);
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // CHECKPOINT: IDOR Protection — Edit Ownership check (POST)
            if (course.TeacherId != userId) return Forbid();

            course.Title = model.Title;
            course.Description = model.Description;
            course.Status = model.Status;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // CHECKPOINT: IDOR Protection — Delete Ownership check
            if (course.TeacherId != userId) return Forbid();

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }
    }
}
