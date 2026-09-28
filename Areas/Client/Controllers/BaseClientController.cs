// ============================================================
// File: Areas/Client/Controllers/BaseClientController.cs
// Mô tả: Base Controller cho toàn bộ Client Area
// CHECKPOINT: [Authorize(Roles = "Teacher,Student")] gắn 1 chỗ → ăn tất cả
// Pattern: mọi controller trong Client Area kế thừa class này
// ============================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebHocTap_SaaS_.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Teacher,Student")]
    public abstract class BaseClientController : Controller
    {
    }
}
