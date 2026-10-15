// ============================================================
// File: Models/ViewModels/AdminUserViewModel.cs
// Mô tả: ViewModel cho Admin quản lý người dùng (P7)
// ============================================================

namespace WebHocTap_SaaS_.Models.ViewModels
{
    /// <summary>
    /// Hiển thị 1 dòng/card trong danh sách User (Admin)
    /// </summary>
    public class AdminUserRowViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool IsCurrentUser { get; set; }

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName)) return "U";
                var parts = FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
        }
    }

    /// <summary>
    /// ViewModel tổng cho trang Admin/User/Index
    /// </summary>
    public class AdminUserIndexViewModel
    {
        public List<AdminUserRowViewModel> Users { get; set; } = new();
        public string? SearchQuery { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }

        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int LockedCount { get; set; }
        public int LockoutCount { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
    }
}