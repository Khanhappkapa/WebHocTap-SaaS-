// ============================================================
// File: Areas/Admin/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Admin
// CHECKPOINT: Kế thừa BaseAdminController → tự động có [Area("Admin")] + [Authorize(Roles="Admin")]
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace WebHocTap_SaaS_.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
