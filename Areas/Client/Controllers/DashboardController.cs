// ============================================================
// File: Areas/Client/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Client (Teacher + Student)
// CHECKPOINT: [Area("Client")] — CHƯA gắn [Authorize], sẽ thêm ở Phần 3
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    [Area("Client")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
