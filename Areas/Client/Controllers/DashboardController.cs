// ============================================================
// File: Areas/Client/Controllers/DashboardController.cs
// Mô tả: Controller trang Dashboard cho Client (Teacher + Student)
// CHECKPOINT: Kế thừa BaseClientController → tự động có [Area("Client")] + [Authorize(Roles="Teacher,Student")]
// ============================================================

using Microsoft.AspNetCore.Mvc;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    public class DashboardController : BaseClientController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
