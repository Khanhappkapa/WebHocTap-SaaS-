// ============================================================
// File: Controllers/AccountController.cs
// Mô tả: Controller xử lý Register / Login / Logout / AccessDenied
// CHECKPOINT: Chặn cứng role "Admin" khi register (chống leo thang)
// CHECKPOINT: Kiểm tra IsActive trước khi cho đăng nhập
// CHECKPOINT: Đồng bộ UserRole + AddToRoleAsync khi register
// CHECKPOINT: Logout BẮT BUỘC dùng [HttpPost] (anti-CSRF)
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --------------------------------------------------
        // CHECKPOINT: GET Register — hiển thị form đăng ký
        // --------------------------------------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // --------------------------------------------------
        // CHECKPOINT: POST Register
        // 1. Chặn cứng UserRole == "Admin" (chống F12 sửa value)
        // 2. Tạo AppUser + CreateAsync
        // 3. BẮT BUỘC AddToRoleAsync (đồng bộ Identity Roles)
        // 4. SignInAsync → redirect /Client/Dashboard
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // CHECKPOINT: Chặn cứng — KHÔNG cho đăng ký role Admin
            // Kể cả user F12 sửa dropdown value thành "Admin" cũng bị chặn
            if (model.UserRole == "Admin")
            {
                ModelState.AddModelError("UserRole", "Không được phép đăng ký với vai trò Admin.");
                return View(model);
            }

            // Chỉ chấp nhận Teacher hoặc Student
            if (model.UserRole != "Teacher" && model.UserRole != "Student")
            {
                ModelState.AddModelError("UserRole", "Vai trò không hợp lệ. Chỉ chấp nhận Teacher hoặc Student.");
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                UserRole = model.UserRole,  // Đồng bộ điểm 1: lưu vào cột UserRole
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // CHECKPOINT: Đồng bộ điểm 2 — BẮT BUỘC AddToRoleAsync
                // Nếu thiếu dòng này, [Authorize(Roles="Teacher")] sẽ KHÔNG nhận diện user
                // → user đăng ký xong vào /Client/Dashboard bị đá ra AccessDenied
                await _userManager.AddToRoleAsync(user, model.UserRole);

                // Đăng nhập ngay sau khi đăng ký thành công
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Redirect về Client Dashboard
                return RedirectToAction("Index", "Dashboard", new { area = "Client" });
            }

            // Hiển thị lỗi từ Identity (vd: email trùng, password yếu)
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // --------------------------------------------------
        // CHECKPOINT: GET Login — hiển thị form đăng nhập
        // --------------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // --------------------------------------------------
        // CHECKPOINT: POST Login
        // 1. FindByEmailAsync — lỗi chung "Email hoặc mật khẩu không đúng"
        // 2. Kiểm tra IsActive — chặn tài khoản bị khóa
        // 3. PasswordSignInAsync
        // 4. Redirect theo role: Admin → /Admin/Dashboard, còn lại → /Client/Dashboard
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // CHECKPOINT: Thông báo chung — KHÔNG nói rõ "email không tồn tại"
                // để tránh kẻ tấn công dò danh sách email
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // CHECKPOINT: Kiểm tra IsActive TRƯỚC khi cho đăng nhập
            // Nếu admin set IsActive = false trong Supabase thì user không thể login
            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Liên hệ quản trị viên.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // CHECKPOINT: Redirect theo role
                // Admin → /Admin/Dashboard
                // Teacher/Student → /Client/Dashboard
                if (user.UserRole == "Admin")
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                return RedirectToAction("Index", "Dashboard", new { area = "Client" });
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        // --------------------------------------------------
        // CHECKPOINT: POST Logout
        // BẮT BUỘC [HttpPost] + [ValidateAntiForgeryToken]
        // Logout bằng <a href> là lỗi bảo mật CSRF nghiêm trọng
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // --------------------------------------------------
        // CHECKPOINT: GET AccessDenied
        // Khi user truy cập area không có quyền → hiển thị trang này
        // --------------------------------------------------
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
