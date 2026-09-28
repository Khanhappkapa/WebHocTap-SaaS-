// ============================================================
// File: Data/ApplicationDbContext.cs
// Mô tả: DbContext chính cho ứng dụng, kế thừa IdentityDbContext<AppUser>
// CHECKPOINT: Cấu hình Fluent API cho tất cả quan hệ và ràng buộc
// ============================================================

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Data
{
    /// <summary>
    /// DbContext chính, kế thừa IdentityDbContext để tích hợp ASP.NET Core Identity
    /// Sử dụng AppUser thay cho IdentityUser mặc định
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --------------------------------------------------
        // CHECKPOINT: Khai báo DbSet cho các bảng
        // Các bảng Identity (AspNetUsers, AspNetRoles, ...) đã được 
        // IdentityDbContext tự động khai báo
        // --------------------------------------------------

        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        /// <summary>
        /// Override OnModelCreating để cấu hình Fluent API
        /// CHECKPOINT: Tất cả quan hệ FK, Unique constraints, Delete behaviors
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // ============================================================
            // QUAN TRỌNG: Gọi base trước để cấu hình Identity tables
            // Nếu không gọi base, các bảng AspNetUsers, AspNetRoles, ... 
            // sẽ không được tạo đúng cách
            // ============================================================
            base.OnModelCreating(builder);

            // --------------------------------------------------
            // CHECKPOINT 1: Quan hệ Teacher → Course (1-nhiều)
            // Một Teacher (AppUser) có nhiều Course
            // Xóa Teacher KHÔNG xóa Course (Restrict)
            // --------------------------------------------------
            builder.Entity<Course>(entity =>
            {
                entity.HasOne(c => c.Teacher)               // Course có 1 Teacher
                      .WithMany(u => u.Courses)              // Teacher có nhiều Courses
                      .HasForeignKey(c => c.TeacherId)       // FK = TeacherId
                      .OnDelete(DeleteBehavior.Restrict);    // Không cascade delete

                // Đảm bảo Status được lưu dưới dạng int trong database
                entity.Property(c => c.Status)
                      .HasConversion<int>();
            });

            // --------------------------------------------------
            // CHECKPOINT 2: Quan hệ Student → Enrollment (1-nhiều)
            // Một Student có nhiều Enrollment
            // + Unique constraint: Mỗi Student chỉ enroll 1 Course 1 lần
            // --------------------------------------------------
            builder.Entity<Enrollment>(entity =>
            {
                entity.HasOne(e => e.Student)                // Enrollment có 1 Student
                      .WithMany(u => u.Enrollments)          // Student có nhiều Enrollments
                      .HasForeignKey(e => e.StudentId)       // FK = StudentId
                      .OnDelete(DeleteBehavior.Cascade);     // Xóa Student → xóa Enrollment

                entity.HasOne(e => e.Course)                 // Enrollment có 1 Course
                      .WithMany(c => c.Enrollments)          // Course có nhiều Enrollments
                      .HasForeignKey(e => e.CourseId)        // FK = CourseId
                      .OnDelete(DeleteBehavior.Cascade);     // Xóa Course → xóa Enrollment

                // UNIQUE CONSTRAINT: Một student chỉ được enroll 1 course 1 lần
                entity.HasIndex(e => new { e.CourseId, e.StudentId })
                      .IsUnique()
                      .HasDatabaseName("IX_Enrollment_CourseId_StudentId");
            });

            // --------------------------------------------------
            // CHECKPOINT 3: Quan hệ Course → Session (1-nhiều)
            // Xóa Course → xóa luôn Sessions
            // --------------------------------------------------
            builder.Entity<Session>(entity =>
            {
                entity.HasOne(s => s.Course)
                      .WithMany(c => c.Sessions)
                      .HasForeignKey(s => s.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --------------------------------------------------
            // CHECKPOINT 4: Quan hệ Course → Material (1-nhiều)
            // Xóa Course → xóa luôn Materials
            // --------------------------------------------------
            builder.Entity<Material>(entity =>
            {
                entity.HasOne(m => m.Course)
                      .WithMany(c => c.Materials)
                      .HasForeignKey(m => m.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // FileType enum → int
                entity.Property(m => m.FileType)
                      .HasConversion<int?>();
            });

            // --------------------------------------------------
            // CHECKPOINT 5: Quan hệ Course → Assignment (1-nhiều)
            // Xóa Course → xóa luôn Assignments (và cascade tiếp tới Submissions)
            // --------------------------------------------------
            builder.Entity<Assignment>(entity =>
            {
                entity.HasOne(a => a.Course)
                      .WithMany(c => c.Assignments)
                      .HasForeignKey(a => a.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // --------------------------------------------------
            // CHECKPOINT 6: Quan hệ Assignment → Submission (1-nhiều)
            // Xóa Assignment → xóa luôn Submissions (Cascade)
            // + Unique constraint: Mỗi Student chỉ nộp 1 lần cho 1 Assignment
            // --------------------------------------------------
            builder.Entity<Submission>(entity =>
            {
                entity.HasOne(s => s.Assignment)             // Submission có 1 Assignment
                      .WithMany(a => a.Submissions)          // Assignment có nhiều Submissions
                      .HasForeignKey(s => s.AssignmentId)    // FK = AssignmentId
                      .OnDelete(DeleteBehavior.Cascade);     // CHECKPOINT: Cascade Delete

                entity.HasOne(s => s.Student)                // Submission có 1 Student
                      .WithMany(u => u.Submissions)          // Student có nhiều Submissions
                      .HasForeignKey(s => s.StudentId)       // FK = StudentId
                      .OnDelete(DeleteBehavior.Restrict);    // Không cascade (tránh xung đột)

                // UNIQUE CONSTRAINT: Một student chỉ nộp 1 lần cho 1 assignment
                entity.HasIndex(s => new { s.AssignmentId, s.StudentId })
                      .IsUnique()
                      .HasDatabaseName("IX_Submission_AssignmentId_StudentId");
            });
        }
    }
}
