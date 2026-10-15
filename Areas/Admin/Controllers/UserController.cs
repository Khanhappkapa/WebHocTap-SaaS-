// ============================================================
// File: Areas/Admin/Controllers/UserController.cs
// Mô tả: Admin quản lý người dùng (P7)
// CHECKPOINT: Kế thừa BaseAdminController → tự có [Area("Admin")] + [Authorize(Roles = "Admin")]
// Hành động: Index (danh sách), ToggleActive (khóa/mở), Unlock (mở khóa lockout)
// An toàn: Admin KHÔNG thể tự khóa chính mình, KHÔNG thể khóa admin cuối cùng
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;
using WebHocTap_SaaS_.Models.ViewModels;

namespace WebHocTap_SaaS_.Areas.Admin.Controllers
{
    public class UserController : BaseAdminController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ApplicationDbContext _db;

        public UserController(UserManager<AppUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // --------------------------------------------------
        // GET: /Admin/User
        // Danh sách toàn bộ user, hỗ trợ search + filter role + filter status
        // --------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? role, string? status, int page = 1)
        {
            var currentUserId = _userManager.GetUserId(User);

            // Thống kê toàn hệ thống
            var now = DateTimeOffset.UtcNow;
            var totalCount = await _db.Users.CountAsync();
            var activeCount = await _db.Users.CountAsync(u => u.IsActive);
            var lockedCount = await _db.Users.CountAsync(u => !u.IsActive);
            var lockoutCount = await _db.Users.CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > now);

            var query = _db.Users.AsQueryable();

            // Tìm kiếm theo tên hoặc email
            if (!string.IsNullOrWhiteSpace(q))
            {
                var search = q.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(search)
                                      || u.Email!.ToLower().Contains(search));
            }

            // Lọc theo vai trò
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.UserRole == role);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "Active") query = query.Where(u => u.IsActive);
                else if (status == "Locked") query = query.Where(u => !u.IsActive);
                else if (status == "Lockout") query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > now);
            }

            // Lấy danh sách: đưa user hiện tại lên đầu nếu có
            var allMatchingUsers = await query.ToListAsync();
            var sortedUsers = allMatchingUsers
                .OrderByDescending(u => u.Id == currentUserId)
                .ThenBy(u => u.FullName)
                .ToList();

            var vm = new AdminUserIndexViewModel
            {
                SearchQuery = q,
                RoleFilter = role,
                StatusFilter = status,
                TotalCount = totalCount,
                ActiveCount = activeCount,
                LockedCount = lockedCount,
                LockoutCount = lockoutCount,
                CurrentPage = page < 1 ? 1 : page,
                PageSize = 10,
                Users = sortedUsers.Select(u => new AdminUserRowViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    UserRole = u.UserRole,
                    IsActive = u.IsActive,
                    IsLockedOut = u.LockoutEnd != null && u.LockoutEnd > now,
                    LockoutEnd = u.LockoutEnd,
                    IsCurrentUser = (u.Id == currentUserId)
                }).ToList()
            };

            return View(vm);
        }

        // --------------------------------------------------
        // POST: /Admin/User/ToggleActive/{id}
        // Khóa / mở khóa tài khoản (IsActive = !IsActive)
        // CHECKPOINT: Chặn tự khóa mình + chặn khóa admin cuối cùng
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            // CHECKPOINT: Chặn tự khóa chính mình
            if (id == currentUserId)
            {
                TempData["Error"] = "Không thể khóa tài khoản của chính mình.";
                return RedirectToAction(nameof(Index));
            }

            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // CHECKPOINT: Chặn khóa admin cuối cùng
            if (targetUser.IsActive && targetUser.UserRole == "Admin")
            {
                var activeAdminCount = await _db.Users
                    .CountAsync(u => u.UserRole == "Admin" && u.IsActive);
                if (activeAdminCount <= 1)
                {
                    TempData["Error"] = "Không thể khóa admin cuối cùng trong hệ thống.";
                    return RedirectToAction(nameof(Index));
                }
            }

            targetUser.IsActive = !targetUser.IsActive;
            await _userManager.UpdateAsync(targetUser);

            TempData["Success"] = targetUser.IsActive
                ? $"Đã mở khóa tài khoản {targetUser.FullName}."
                : $"Đã khóa tài khoản {targetUser.FullName}.";

            return RedirectToAction(nameof(Index));
        }

        // --------------------------------------------------
        // POST: /Admin/User/Unlock/{id}
        // Mở khóa lockout (Identity lockout, do nhập sai mật khẩu)
        // --------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction(nameof(Index));
            }

            // Reset lockout: đặt LockoutEnd = null + reset AccessFailedCount
            await _userManager.SetLockoutEndDateAsync(targetUser, null);
            await _userManager.ResetAccessFailedCountAsync(targetUser);

            TempData["Success"] = $"Đã mở khóa lockout cho {targetUser.FullName}.";
            return RedirectToAction(nameof(Index));
        }
    }
}