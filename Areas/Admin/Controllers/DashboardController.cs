// ============================================================
// File: Areas/Admin/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Admin
// CHECKPOINT: [Area("Admin")] — CHƯA gắn [Authorize], sẽ thêm ở Phần 3
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace WebHocTap_SaaS_.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
