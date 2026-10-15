// ============================================================
// File: Areas/Client/Controllers/ProfileController.cs
// Mô tả: Hồ sơ cá nhân (P7) - Teacher + Student đều dùng
// CHECKPOINT: Kế thừa BaseClientController → [Area("Client")] + [Authorize(Roles = "Teacher,Student")]
// IDOR-proof: KHÔNG nhận userId từ request/form; luôn dùng CurrentUserId
// Email là read-only, KHÔNG cho sửa
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    public class ProfileController : BaseClientController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public ProfileController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --------------------------------------------------
        // GET: /Client/Profile
        // Trang hồ sơ cá nhân
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = new ProfileViewModel
            {
                Email = user.Email ?? "",
                FullName = user.FullName,
                UserRole = user.UserRole,
                JoinedAt = user.LockoutEnd?.DateTime ?? DateTime.UtcNow
            };

            return View(vm);
        }

        // --------------------------------------------------
        // POST: /Client/Profile/UpdateProfile
        // Cập nhật họ tên (chỉ FullName, email read-only)
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? "";
                model.UserRole = user.UserRole;
                return View("Index", model);
            }

            user.FullName = model.FullName.Trim();
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Đã lưu hồ sơ";
            }
            else
            {
                TempData["Error"] = "Cập nhật thất bại. Vui lòng thử lại.";
            }

            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------
        // POST: /Client/Profile/ChangePassword
        // Đổi mật khẩu
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                var profileVm = new ProfileViewModel
                {
                    Email = user.Email ?? "",
                    FullName = user.FullName,
                    UserRole = user.UserRole
                };

                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ViewData["PasswordMismatch"] = true;
                }
                else
                {
                    var firstErr = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    TempData["Error"] = firstErr ?? "Vui lòng kiểm tra lại thông tin mật khẩu.";
                }
                return View("Index", profileVm);
            }

            var result = await _userManager.ChangePasswordAsync(user,
                model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Đã đổi mật khẩu";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordMismatch")
                        TempData["Error"] = "Mật khẩu hiện tại không đúng.";
                    else
                        TempData["Error"] = error.Description;
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}