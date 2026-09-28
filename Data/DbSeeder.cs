// ============================================================
// File: Data/DbSeeder.cs
// Mô tả: Seed 3 roles + 3 tài khoản demo
// CHECKPOINT: IDEMPOTENT — chạy nhiều lần không trùng lặp
//   - Kiểm tra RoleExistsAsync trước khi tạo role
//   - Kiểm tra FindByEmailAsync == null trước khi tạo user
//   - BẮT BUỘC AddToRoleAsync (đồng bộ Identity Roles)
// ============================================================

using Microsoft.AspNetCore.Identity;
using WebHocTap_SaaS_.Models;

namespace WebHocTap_SaaS_.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Seed dữ liệu ban đầu: 3 roles + 3 users demo.
        /// Phương thức này PHẢI IDEMPOTENT: gọi bao nhiêu lần cũng không crash hay tạo trùng.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // --------------------------------------------------
            // CHECKPOINT: Seed 3 Roles (idempotent)
            // RoleExistsAsync kiểm tra trước → chỉ tạo nếu chưa có
            // --------------------------------------------------
            string[] roles = { "Admin", "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // --------------------------------------------------
            // CHECKPOINT: Seed 3 Users demo (idempotent)
            // FindByEmailAsync == null → chỉ tạo nếu chưa có
            // Mỗi user: set cả UserRole (cột DB) + AddToRoleAsync (Identity)
            // --------------------------------------------------

            // ① Admin
            await SeedUserAsync(userManager,
                email: "admin@class.com",
                password: "Admin@123",
                fullName: "Quản trị viên hệ thống",
                role: "Admin");

            // ② Teacher
            await SeedUserAsync(userManager,
                email: "teacher@class.com",
                password: "Teacher@123",
                fullName: "Giáo viên mẫu",
                role: "Teacher");

            // ③ Student
            await SeedUserAsync(userManager,
                email: "student@class.com",
                password: "Student@123",
                fullName: "Học viên mẫu",
                role: "Student");
        }

        /// <summary>
        /// Helper: tạo 1 user nếu chưa tồn tại.
        /// Đồng bộ cả 2 nguồn sự thật: cột UserRole + Identity Roles.
        /// </summary>
        private static async Task SeedUserAsync(
            UserManager<AppUser> userManager,
            string email, string password, string fullName, string role)
        {
            // CHECKPOINT: Idempotent — chỉ tạo nếu email chưa tồn tại
            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                UserRole = role,        // Đồng bộ điểm 1: cột trong DB
                IsActive = true,
                EmailConfirmed = true   // Seed user không cần xác nhận email
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // CHECKPOINT: Đồng bộ điểm 2 — thêm vào AspNetUserRoles
                // Nếu thiếu, [Authorize(Roles="Admin")] sẽ không nhận diện user này
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
