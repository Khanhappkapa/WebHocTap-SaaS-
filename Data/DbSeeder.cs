// ============================================================
// File: Data/DbSeeder.cs
// Mô tả: Seed 3 roles + tài khoản demo + ma trận dữ liệu mẫu P9 (E1-E13)
// CHECKPOINT: IDEMPOTENT — chạy nhiều lần không trùng lặp, không vỡ Unique Index
//   - Tự chữa lành: Luôn đảm bảo tài khoản demo đúng trạng thái, đúng mật khẩu
//   - Phủ đủ ma trận trạng thái edge E1-E13 theo Part 9 Spec
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Seed 3 Roles
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Người dùng cốt lõi (E13, E1, E12, E2, E3, E8, E9, E10, E11)
            await SeedOrResetUserAsync(userManager, "admin@class.com", "Admin@123", "Ngọc Linh", "Admin");
            await SeedOrResetUserAsync(userManager, "teacher@class.com", "Teacher@123", "Minh Anh", "Teacher");
            await SeedOrResetUserAsync(userManager, "teacher_b@class.com", "Teacher@123", "Thầy Bình", "Teacher");
            await SeedOrResetUserAsync(userManager, "student@class.com", "Student@123", "Gia Hân", "Student");
            await SeedOrResetUserAsync(userManager, "student_awaiting@class.com", "Student@123", "Bảo Ngọc", "Student");
            await SeedOrResetUserAsync(userManager, "student_lockout@class.com", "Student@123", "Hải Đăng", "Student");
            await SeedOrResetUserAsync(userManager, "student_inactive@class.com", "Student@123", "Tuấn Kiệt", "Student");
            await SeedOrResetUserAsync(userManager, "student_not_enrolled@class.com", "Student@123", "Thanh Trúc", "Student");
            await SeedOrResetUserAsync(userManager, "new_student@class.com", "Student@123", "Học viên Mới", "Student");

            // 3. Cấu hình trạng thái đặc thù cho E8 (Lockout) & E9 (Inactive)
            var now = DateTime.UtcNow;

            var lockoutUser = await userManager.FindByEmailAsync("student_lockout@class.com");
            if (lockoutUser != null)
            {
                lockoutUser.IsActive = true;
                lockoutUser.LockoutEnabled = true;
                lockoutUser.LockoutEnd = DateTime.SpecifyKind(now.AddMinutes(15), DateTimeKind.Utc);
                lockoutUser.AccessFailedCount = 5;
                await userManager.UpdateAsync(lockoutUser);
            }

            var inactiveUser = await userManager.FindByEmailAsync("student_inactive@class.com");
            if (inactiveUser != null)
            {
                inactiveUser.IsActive = false;
                inactiveUser.LockoutEnd = null;
                inactiveUser.AccessFailedCount = 0;
                await userManager.UpdateAsync(inactiveUser);
            }

            // 4. Seed Khóa học, Buổi học, Tài liệu, Bài tập và Bài nộp (E1-E7, E10, E12)
            await SeedPart9EntitiesAsync(context, userManager, now);
        }

        private static async Task SeedOrResetUserAsync(
            UserManager<AppUser> userManager,
            string email, string password, string fullName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    UserRole = role,
                    IsActive = true,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, password);
            }
            else
            {
                // Tự chữa lành: Đồng bộ role và reset password về chuẩn demo
                user.FullName = fullName;
                user.UserRole = role;

                var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, resetToken, password);
                await userManager.UpdateAsync(user);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        private static async Task SeedPart9EntitiesAsync(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            DateTime now)
        {
            var teacherA = await userManager.FindByEmailAsync("teacher@class.com");
            var teacherB = await userManager.FindByEmailAsync("teacher_b@class.com");
            var student = await userManager.FindByEmailAsync("student@class.com");
            var studentAwaiting = await userManager.FindByEmailAsync("student_awaiting@class.com");

            if (teacherA == null || teacherB == null || student == null || studentAwaiting == null) return;

            // ========================================================
            // E1, E2, E3, E4, E5: KHÓA HỌC CHÍNH CỦA CÔ MINH ANH (PUBLISHED)
            // ========================================================
            var courseA = await context.Courses.FirstOrDefaultAsync(c => c.TeacherId == teacherA.Id && c.Title == "Nhập môn Lập trình Web");
            if (courseA == null)
            {
                courseA = new Course
                {
                    TeacherId = teacherA.Id,
                    Title = "Nhập môn Lập trình Web",
                    Description = "Khóa học nền tảng về phát triển ứng dụng Web với ASP.NET Core MVC và C#.",
                    Status = CourseStatus.Published,
                    IsHidden = false,
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-10), DateTimeKind.Utc)
                };
                context.Courses.Add(courseA);
                await context.SaveChangesAsync();
            }
            else
            {
                courseA.Status = CourseStatus.Published;
                courseA.IsHidden = false;
                await context.SaveChangesAsync();
            }

            // Ghi danh: student (Gia Hân) và studentAwaiting (Bảo Ngọc)
            if (!await context.Enrollments.AnyAsync(e => e.CourseId == courseA.Id && e.StudentId == student.Id))
            {
                context.Enrollments.Add(new Enrollment
                {
                    CourseId = courseA.Id,
                    StudentId = student.Id,
                    EnrolledAt = DateTime.SpecifyKind(now.AddDays(-5), DateTimeKind.Utc)
                });
            }
            if (!await context.Enrollments.AnyAsync(e => e.CourseId == courseA.Id && e.StudentId == studentAwaiting.Id))
            {
                context.Enrollments.Add(new Enrollment
                {
                    CourseId = courseA.Id,
                    StudentId = studentAwaiting.Id,
                    EnrolledAt = DateTime.SpecifyKind(now.AddDays(-4), DateTimeKind.Utc)
                });
            }
            await context.SaveChangesAsync();

            // E1: Buổi học sắp tới (StartTime = now + 2h)
            var sessionE1 = await context.Sessions.FirstOrDefaultAsync(s => s.CourseId == courseA.Id && s.Title == "Buổi học: Lập trình MVC cơ bản");
            if (sessionE1 == null)
            {
                sessionE1 = new Session
                {
                    CourseId = courseA.Id,
                    Title = "Buổi học: Lập trình MVC cơ bản",
                    Description = "Tìm hiểu mô hình Model-View-Controller và xử lý routing trong ASP.NET Core MVC.",
                    StartTime = DateTime.SpecifyKind(now.AddHours(2), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(now.AddHours(4), DateTimeKind.Utc),
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-2), DateTimeKind.Utc)
                };
                context.Sessions.Add(sessionE1);
            }
            else
            {
                sessionE1.StartTime = DateTime.SpecifyKind(now.AddHours(2), DateTimeKind.Utc);
                sessionE1.EndTime = DateTime.SpecifyKind(now.AddHours(4), DateTimeKind.Utc);
            }

            // E1: Tài liệu học tập PDF (bytea lưu trữ DB an toàn qua redeploy)
            var materialE1 = await context.Materials.FirstOrDefaultAsync(m => m.CourseId == courseA.Id && m.Title == "Giáo trình Lập trình Web ASP.NET Core MVC");
            if (materialE1 == null)
            {
                var dummyPdfBytes = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 Bulb.edu Demo Curriculum Document");
                context.Materials.Add(new Material
                {
                    CourseId = courseA.Id,
                    Title = "Giáo trình Lập trình Web ASP.NET Core MVC",
                    FileType = FileType.Pdf,
                    FileName = "GiaoTrinh_ASPNETCore_MVC.pdf",
                    ContentType = "application/pdf",
                    FileData = dummyPdfBytes,
                    UploadedAt = DateTime.SpecifyKind(now.AddDays(-3), DateTimeKind.Utc)
                });
            }
            await context.SaveChangesAsync();

            // E2 & E3: Bài tập 1 (Hạn nộp now + 12h, < 24h)
            // - student (Gia Hân): CHƯA NỘP -> hiện trong ToDo kèm pill đỏ "Sắp hết hạn" (E2)
            // - studentAwaiting (Bảo Ngọc): ĐÃ NỘP, Score = null -> Chờ chấm (E3)
            var assign1 = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseA.Id && a.Title == "Bài tập 1: Xây dựng Layout Razor");
            if (assign1 == null)
            {
                assign1 = new Assignment
                {
                    CourseId = courseA.Id,
                    Title = "Bài tập 1: Xây dựng Layout Razor",
                    Description = "Thiết kế layout responsive với Bootstrap 5 và hệ thống phác thảo nét vẽ tay Bulb.edu.",
                    Deadline = DateTime.SpecifyKind(now.AddHours(12), DateTimeKind.Utc),
                    MaxScore = 10,
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-1), DateTimeKind.Utc)
                };
                context.Assignments.Add(assign1);
                await context.SaveChangesAsync();
            }
            else
            {
                assign1.Deadline = DateTime.SpecifyKind(now.AddHours(12), DateTimeKind.Utc);
                await context.SaveChangesAsync();
            }

            // Đảm bảo Gia Hân (student) chưa nộp assign1 để test E2
            var subGiaHanAssign1 = await context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assign1.Id && s.StudentId == student.Id);
            if (subGiaHanAssign1 != null)
            {
                context.Submissions.Remove(subGiaHanAssign1);
                await context.SaveChangesAsync();
            }

            // E3: Bảo Ngọc nộp assign1, Score = null (Awaiting grading)
            var subAwaiting = await context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assign1.Id && s.StudentId == studentAwaiting.Id);
            if (subAwaiting == null)
            {
                context.Submissions.Add(new Submission
                {
                    AssignmentId = assign1.Id,
                    StudentId = studentAwaiting.Id,
                    FileName = "LayoutRazor_BaoNgoc.zip",
                    ContentType = "application/zip",
                    FileData = System.Text.Encoding.UTF8.GetBytes("Demo submission content"),
                    SubmittedAt = DateTime.SpecifyKind(now.AddHours(-2), DateTimeKind.Utc),
                    Score = null // Chờ chấm!
                });
                await context.SaveChangesAsync();
            }

            // E4: Bài tập 2 (Đã chấm điểm wow - Score = 85/100 + Lời phê FZ Caveat)
            var assign2 = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseA.Id && a.Title == "Bài tập 2: Cơ sở dữ liệu Entity Framework");
            if (assign2 == null)
            {
                assign2 = new Assignment
                {
                    CourseId = courseA.Id,
                    Title = "Bài tập 2: Cơ sở dữ liệu Entity Framework",
                    Description = "Thực hành cấu hình DbContext, truy vấn LINQ và quản lý quan hệ FK trong PostgreSQL.",
                    Deadline = DateTime.SpecifyKind(now.AddDays(4), DateTimeKind.Utc),
                    MaxScore = 100,
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-3), DateTimeKind.Utc)
                };
                context.Assignments.Add(assign2);
                await context.SaveChangesAsync();
            }
            else
            {
                assign2.MaxScore = 100;
                assign2.Deadline = DateTime.SpecifyKind(now.AddDays(4), DateTimeKind.Utc);
                await context.SaveChangesAsync();
            }

            var sub2Graded = await context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assign2.Id && s.StudentId == student.Id);
            if (sub2Graded == null)
            {
                context.Submissions.Add(new Submission
                {
                    AssignmentId = assign2.Id,
                    StudentId = student.Id,
                    FileName = "EntityFramework_GiaHan.pdf",
                    ContentType = "application/pdf",
                    FileData = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 Assignment Solution"),
                    SubmittedAt = DateTime.SpecifyKind(now.AddDays(-1), DateTimeKind.Utc),
                    Score = 85,
                    Feedback = "Bài làm rất xuất sắc! Bố cục giao diện chuẩn, truy vấn LINQ tối ưu và xử lý quan hệ chuẩn mực.",
                    GradedAt = DateTime.SpecifyKind(now.AddHours(-3), DateTimeKind.Utc)
                });
                await context.SaveChangesAsync();
            }
            else
            {
                sub2Graded.Score = 85;
                sub2Graded.Feedback = "Bài làm rất xuất sắc! Bố cục giao diện chuẩn, truy vấn LINQ tối ưu và xử lý quan hệ chuẩn mực.";
                sub2Graded.GradedAt = DateTime.SpecifyKind(now.AddHours(-3), DateTimeKind.Utc);
                await context.SaveChangesAsync();
            }

            // E5: Bài tập QUÁ HẠN (Deadline = now - 2 ngày, Gia Hân chưa nộp)
            var assignOverdue = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseA.Id && a.Title == "Bài tập 0: Khởi tạo Project & Git");
            if (assignOverdue == null)
            {
                assignOverdue = new Assignment
                {
                    CourseId = courseA.Id,
                    Title = "Bài tập 0: Khởi tạo Project & Git",
                    Description = "Cấu hình repository Git và khởi tạo cấu trúc thư mục chuẩn.",
                    Deadline = DateTime.SpecifyKind(now.AddDays(-2), DateTimeKind.Utc),
                    MaxScore = 10,
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-6), DateTimeKind.Utc)
                };
                context.Assignments.Add(assignOverdue);
                await context.SaveChangesAsync();
            }
            else
            {
                assignOverdue.Deadline = DateTime.SpecifyKind(now.AddDays(-2), DateTimeKind.Utc);
                await context.SaveChangesAsync();
            }

            // Đảm bảo Gia Hân chưa nộp bài tập quá hạn
            var subOverdue = await context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assignOverdue.Id && s.StudentId == student.Id);
            if (subOverdue != null)
            {
                context.Submissions.Remove(subOverdue);
                await context.SaveChangesAsync();
            }

            // ========================================================
            // E6: KHÓA HỌC DRAFT (BẢN NHÁP - KHÔNG HIỆN Ở CATALOG)
            // ========================================================
            var courseDraft = await context.Courses.FirstOrDefaultAsync(c => c.TeacherId == teacherA.Id && c.Title == "Lập trình Web nâng cao với Microservices");
            if (courseDraft == null)
            {
                courseDraft = new Course
                {
                    TeacherId = teacherA.Id,
                    Title = "Lập trình Web nâng cao với Microservices",
                    Description = "Kiến trúc Microservices, RabbitMQ và Docker container hóa ứng dụng .NET 8.",
                    Status = CourseStatus.Draft,
                    IsHidden = false,
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-3), DateTimeKind.Utc)
                };
                context.Courses.Add(courseDraft);
                await context.SaveChangesAsync();
            }
            else
            {
                courseDraft.Status = CourseStatus.Draft;
                courseDraft.IsHidden = false;
                await context.SaveChangesAsync();
            }

            // ========================================================
            // E7: KHÓA HỌC CLOSED & HIDDEN (ĐÓNG & BỊ ẨN BỞI ADMIN)
            // ========================================================
            var courseClosed = await context.Courses.FirstOrDefaultAsync(c => c.TeacherId == teacherA.Id && c.Title == "Lập trình C# cơ bản khóa 2025");
            if (courseClosed == null)
            {
                courseClosed = new Course
                {
                    TeacherId = teacherA.Id,
                    Title = "Lập trình C# cơ bản khóa 2025",
                    Description = "Khóa học đã hoàn tất chu kỳ giảng dạy và được đưa vào kho lưu trữ nội bộ.",
                    Status = CourseStatus.Closed,
                    IsHidden = true,
                    CreatedAt = DateTime.SpecifyKind(now.AddMonths(-3), DateTimeKind.Utc)
                };
                context.Courses.Add(courseClosed);
                await context.SaveChangesAsync();
            }
            else
            {
                courseClosed.Status = CourseStatus.Closed;
                courseClosed.IsHidden = true;
                await context.SaveChangesAsync();
            }

            // ========================================================
            // E12: KHÓA HỌC CHO TEACHER B (THẦY BÌNH - IDOR ISOLATION)
            // ========================================================
            var courseB = await context.Courses.FirstOrDefaultAsync(c => c.TeacherId == teacherB.Id && c.Title == "Giải tích 1 nâng cao");
            if (courseB == null)
            {
                courseB = new Course
                {
                    TeacherId = teacherB.Id,
                    Title = "Giải tích 1 nâng cao",
                    Description = "Khóa học nâng cao dành cho sinh viên Toán - Tin và Khoa học Dữ liệu.",
                    Status = CourseStatus.Published,
                    IsHidden = false,
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-8), DateTimeKind.Utc)
                };
                context.Courses.Add(courseB);
                await context.SaveChangesAsync();
            }

            var sessionB = await context.Sessions.FirstOrDefaultAsync(s => s.CourseId == courseB.Id && s.Title == "Buổi học: Đạo hàm và Vi phân");
            if (sessionB == null)
            {
                context.Sessions.Add(new Session
                {
                    CourseId = courseB.Id,
                    Title = "Buổi học: Đạo hàm và Vi phân",
                    Description = "Khảo sát hàm số, cực trị và vi phân cấp cao.",
                    StartTime = DateTime.SpecifyKind(now.AddDays(1), DateTimeKind.Utc),
                    EndTime = DateTime.SpecifyKind(now.AddDays(1).AddHours(2), DateTimeKind.Utc),
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-1), DateTimeKind.Utc)
                });
                await context.SaveChangesAsync();
            }

            var assignB = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseB.Id && a.Title == "Bài tập Đạo hàm cấp cao");
            if (assignB == null)
            {
                context.Assignments.Add(new Assignment
                {
                    CourseId = courseB.Id,
                    Title = "Bài tập Đạo hàm cấp cao",
                    Description = "Giải các bài toán ứng dụng đạo hàm trong tối ưu hóa.",
                    Deadline = DateTime.SpecifyKind(now.AddDays(3), DateTimeKind.Utc),
                    MaxScore = 10,
                    CreatedAt = DateTime.SpecifyKind(now.AddDays(-2), DateTimeKind.Utc)
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
