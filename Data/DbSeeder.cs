using Microsoft.EntityFrameworkCore;
// ============================================================
// File: Data/DbSeeder.cs
// Mô tả: Seed 3 roles + 3 tài khoản demo
// CHECKPOINT: IDEMPOTENT — chạy nhiều lần không trùng lặp
//   - Tự chữa lành: Luôn đảm bảo tài khoản demo Active, không bị Lockout và có đúng mật khẩu
// ============================================================

using Microsoft.AspNetCore.Identity;
using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. Seed 3 Roles
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed / Update 3 Users demo
            await SeedOrResetUserAsync(userManager,
                email: "admin@class.com",
                password: "Admin@123",
                fullName: "Ngọc Linh",
                role: "Admin");

            await SeedOrResetUserAsync(userManager,
                email: "teacher@class.com",
                password: "Teacher@123",
                fullName: "Minh Anh",
                role: "Teacher");

            await SeedOrResetUserAsync(userManager,
                email: "student@class.com",
                password: "Student@123",
                fullName: "Gia Hân",
                role: "Student");

            // Seed dữ liệu test cho Phần 8 (Dashboard & ToDo)
            await SeedPart8DemoDataAsync(serviceProvider);
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
                // Tự chữa lành: Đảm bảo IsActive, hủy lockout, đồng bộ role
                user.IsActive = true;
                user.LockoutEnd = null;
                user.AccessFailedCount = 0;
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

        public static async Task SeedPart8DemoDataAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. Seed Teacher B & New Student (0 dữ liệu)
            await SeedOrResetUserAsync(userManager, "teacher_b@class.com", "Teacher@123", "Thầy Bình", "Teacher");
            await SeedOrResetUserAsync(userManager, "new_student@class.com", "Student@123", "Học viên Mới", "Student");

            var teacherA = await userManager.FindByEmailAsync("teacher@class.com");
            var teacherB = await userManager.FindByEmailAsync("teacher_b@class.com");
            var student = await userManager.FindByEmailAsync("student@class.com");

            if (teacherA == null || teacherB == null || student == null) return;

            var now = DateTime.UtcNow;

            // 2. Khóa học cho Teacher A
            var courseA = await context.Courses.FirstOrDefaultAsync(c => c.TeacherId == teacherA.Id && c.Title == "Nhập môn Lập trình Web");
            if (courseA == null)
            {
                courseA = new Course
                {
                    TeacherId = teacherA.Id,
                    Title = "Nhập môn Lập trình Web",
                    Description = "Khóa học nền tảng về phát triển ứng dụng Web với ASP.NET Core MVC và C#.",
                    Status = CourseStatus.Published,
                    CreatedAt = now.AddDays(-10)
                };
                context.Courses.Add(courseA);
                await context.SaveChangesAsync();
            }

            // Ghi danh học viên Gia Hân vào courseA
            var enrollmentA = await context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseA.Id && e.StudentId == student.Id);
            if (enrollmentA == null)
            {
                context.Enrollments.Add(new Enrollment
                {
                    CourseId = courseA.Id,
                    StudentId = student.Id,
                    EnrolledAt = now.AddDays(-5)
                });
                await context.SaveChangesAsync();
            }

            // Buổi học sắp tới của Teacher A
            var sessionA = await context.Sessions.FirstOrDefaultAsync(s => s.CourseId == courseA.Id && s.Title == "Buổi học: Lập trình MVC cơ bản");
            if (sessionA == null)
            {
                context.Sessions.Add(new Session
                {
                    CourseId = courseA.Id,
                    Title = "Buổi học: Lập trình MVC cơ bản",
                    Description = "Tìm hiểu mô hình Model-View-Controller trong ASP.NET Core",
                    StartTime = now.AddDays(2),
                    EndTime = now.AddDays(2).AddHours(2),
                    CreatedAt = now.AddDays(-2)
                });
                await context.SaveChangesAsync();
            }
            else if (sessionA.StartTime <= now)
            {
                sessionA.StartTime = now.AddDays(2);
                sessionA.EndTime = now.AddDays(2).AddHours(2);
                await context.SaveChangesAsync();
            }

            // Assignment 1: Hạn nộp còn 12 giờ (< 24h -> IsOverdueSoon = true) - học sinh chưa nộp
            var assign1 = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseA.Id && a.Title == "Bài tập 1: Xây dựng Layout Razor");
            if (assign1 == null)
            {
                assign1 = new Assignment
                {
                    CourseId = courseA.Id,
                    Title = "Bài tập 1: Xây dựng Layout Razor",
                    Description = "Thiết kế layout responsive với Bootstrap 5",
                    Deadline = now.AddHours(12),
                    MaxScore = 10,
                    CreatedAt = now.AddDays(-1)
                };
                context.Assignments.Add(assign1);
                await context.SaveChangesAsync();
            }
            else
            {
                assign1.Deadline = now.AddHours(12);
                await context.SaveChangesAsync();
            }

            // Assignment 2: Hạn nộp còn 4 ngày (> 24h -> không đỏ) - học sinh đã nộp và được chấm điểm 8.5
            var assign2 = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseA.Id && a.Title == "Bài tập 2: Cơ sở dữ liệu Entity Framework");
            if (assign2 == null)
            {
                assign2 = new Assignment
                {
                    CourseId = courseA.Id,
                    Title = "Bài tập 2: Cơ sở dữ liệu Entity Framework",
                    Description = "Thực hành truy vấn LINQ và Code First",
                    Deadline = now.AddDays(4),
                    MaxScore = 10,
                    CreatedAt = now.AddDays(-3)
                };
                context.Assignments.Add(assign2);
                await context.SaveChangesAsync();
            }
            else
            {
                assign2.Deadline = now.AddDays(4);
                await context.SaveChangesAsync();
            }

            var sub2 = await context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assign2.Id && s.StudentId == student.Id);
            if (sub2 == null)
            {
                context.Submissions.Add(new Submission
                {
                    AssignmentId = assign2.Id,
                    StudentId = student.Id,
                    FileName = "baitap2.pdf",
                    SubmittedAt = now.AddDays(-1),
                    Score = 8.5,
                    Feedback = "Làm bài rất tốt!",
                    GradedAt = now.AddHours(-2)
                });
                await context.SaveChangesAsync();
            }

            // Assignment 3: Có bài nộp chưa chấm để Teacher chấm bài (ToGrade)
            var assign3 = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseA.Id && a.Title == "Bài tập 3: Authentication & Authorization");
            if (assign3 == null)
            {
                assign3 = new Assignment
                {
                    CourseId = courseA.Id,
                    Title = "Bài tập 3: Authentication & Authorization",
                    Description = "Cấu hình Identity và phân quyền",
                    Deadline = now.AddDays(3),
                    MaxScore = 10,
                    CreatedAt = now.AddDays(-2)
                };
                context.Assignments.Add(assign3);
                await context.SaveChangesAsync();
            }

            var sub3 = await context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assign3.Id && s.StudentId == student.Id);
            if (sub3 == null)
            {
                context.Submissions.Add(new Submission
                {
                    AssignmentId = assign3.Id,
                    StudentId = student.Id,
                    FileName = "baitap3.zip",
                    SubmittedAt = now.AddHours(-3),
                    Score = null // Chờ chấm!
                });
                await context.SaveChangesAsync();
            }

            // 3. Khóa học cho Teacher B (để test IDOR cô lập dữ liệu 2 giáo viên)
            var courseB = await context.Courses.FirstOrDefaultAsync(c => c.TeacherId == teacherB.Id && c.Title == "Giải tích 1 nâng cao");
            if (courseB == null)
            {
                courseB = new Course
                {
                    TeacherId = teacherB.Id,
                    Title = "Giải tích 1 nâng cao",
                    Description = "Khóa học nâng cao dành cho sinh viên Toán - Tin",
                    Status = CourseStatus.Published,
                    CreatedAt = now.AddDays(-8)
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
                    Description = "Khảo sát hàm số nâng cao",
                    StartTime = now.AddDays(1),
                    EndTime = now.AddDays(1).AddHours(2),
                    CreatedAt = now.AddDays(-1)
                });
                await context.SaveChangesAsync();
            }

            var assignB = await context.Assignments.FirstOrDefaultAsync(a => a.CourseId == courseB.Id && a.Title == "Bài tập Đạo hàm cấp cao");
            if (assignB == null)
            {
                assignB = new Assignment
                {
                    CourseId = courseB.Id,
                    Title = "Bài tập Đạo hàm cấp cao",
                    Description = "Giải các bài toán đạo hàm",
                    Deadline = now.AddDays(3),
                    MaxScore = 10,
                    CreatedAt = now.AddDays(-2)
                };
                context.Assignments.Add(assignB);
                await context.SaveChangesAsync();
            }
        }
    }
}
