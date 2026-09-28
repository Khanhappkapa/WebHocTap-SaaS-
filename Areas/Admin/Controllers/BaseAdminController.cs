// ============================================================
// File: Areas/Admin/Controllers/BaseAdminController.cs
// Mô tả: Base Controller cho toàn bộ Admin Area
// CHECKPOINT: [Authorize(Roles = "Admin")] gắn 1 chỗ → ăn tất cả controller kế thừa
// Pattern: mọi controller trong Admin Area kế thừa class này
//          → không cần gắn [Area] + [Authorize] lặp đi lặp lại
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebHocTap_SaaS_.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public abstract class BaseAdminController : Controller
    {
    }
}
