# PHẦN 2: CẤU TRÚC AREAS, LAYOUT KHUNG & RESPONSIVE - Chi tiết chức năng

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến  
> **Ngày tạo**: 28/09/2026  
> **Phần trước**: Phần 1 (Models + DbContext + Identity + Migration)

---

## 1. Sơ đồ kiến trúc Areas + Luồng Route

```
WebHocTap(SaaS)/
├── Controllers/
│   └── HomeController.cs              → Public: Landing (/), Pricing (/Home/Pricing)
├── Views/
│   ├── Shared/_Layout.cshtml          → Layout PUBLIC (navbar trên)
│   ├── _ViewStart.cshtml              → Layout = "_Layout"
│   ├── _ViewImports.cshtml            → @using + @addTagHelper
│   └── Home/
│       ├── Index.cshtml               → Landing page (hero + 3 cards)
│       └── Pricing.cshtml             → Bảng giá SaaS (3 gói)
│
├── Areas/
│   ├── Client/                        → Khu vực Teacher + Student
│   │   ├── Controllers/
│   │   │   └── DashboardController.cs → [Area("Client")]
│   │   └── Views/
│   │       ├── _ViewStart.cshtml      → Layout = ~/Areas/Client/Views/Shared/_ClientLayout.cshtml
│   │       ├── _ViewImports.cshtml
│   │       ├── Shared/_ClientLayout.cshtml  → Topbar xanh + Sidebar trái
│   │       └── Dashboard/Index.cshtml
│   │
│   └── Admin/                         → Khu vực quản trị
│       ├── Controllers/
│       │   └── DashboardController.cs → [Area("Admin")]
│       └── Views/
│           ├── _ViewStart.cshtml      → Layout = ~/Areas/Admin/Views/Shared/_AdminLayout.cshtml
│           ├── _ViewImports.cshtml
│           ├── Shared/_AdminLayout.cshtml   → Topbar tối + Sidebar trái
│           └── Dashboard/Index.cshtml
```

### Luồng Route (Program.cs)

```csharp
// ① Route cho Areas — PHẢI đặt TRƯỚC default route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// ② Route mặc định — cho Public pages
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Cách hoạt động:**
1. Khi user truy cập `/Admin/Dashboard` → Route ① bắt, vì "Admin" khớp `{area:exists}` (có Area "Admin" tồn tại)
2. Khi user truy cập `/Home/Pricing` → Route ① không bắt (vì "Home" không phải area), đến Route ② bắt
3. **Nếu đặt ngược** (Route ② trước Route ①) → `/Admin/Dashboard` bị Route ② bắt, hiểu "Admin" là tên controller → 404

### Bảng URL Mapping

| URL | Route | Area | Controller | Action | Layout |
|-----|-------|------|-----------|--------|--------|
| `/` | default | — | Home | Index | _Layout.cshtml |
| `/Home/Pricing` | default | — | Home | Pricing | _Layout.cshtml |
| `/Client/Dashboard` | areas | Client | Dashboard | Index | _ClientLayout.cshtml |
| `/Admin/Dashboard` | areas | Admin | Dashboard | Index | _AdminLayout.cshtml |

---

## 2. Bảng mô tả 3 Layout

| Layout | File | Khu vực | Màu sắc | Cấu trúc | Responsive |
|--------|------|---------|---------|-----------|------------|
| **Public** | `Views/Shared/_Layout.cshtml` | Khách vãng lai | Navbar: gradient xanh đậm (#1a237e → #0d47a1) | Navbar trên + content + footer | Navbar thu gọn thành hamburger khi < 992px |
| **Client** | `Areas/Client/Views/Shared/_ClientLayout.cshtml` | Teacher + Student | Sidebar: gradient xanh dương (#0d47a1 → #1565c0) | Topbar + Sidebar trái (250px) + content | Sidebar ẩn thành offcanvas khi < 992px |
| **Admin** | `Areas/Admin/Views/Shared/_AdminLayout.cshtml` | Quản trị viên | Sidebar: gradient tối (#1a1a2e → #16213e) | Topbar + Sidebar trái (250px) + content | Sidebar ẩn thành offcanvas khi < 992px |

### Tại sao Client và Admin khác màu?
- **Client (xanh dương)**: truyền cảm giác thân thiện, phù hợp cho học sinh/giáo viên
- **Admin (xanh tối/đen)**: truyền cảm giác chuyên nghiệp, nghiêm túc, dễ phân biệt khi chuyển qua lại

---

## 3. Giải thích kỹ thuật

### 3.1 Tại sao dùng Areas?

**Areas** trong ASP.NET Core MVC cho phép chia ứng dụng thành các khu vực (module) độc lập:

| Tiêu chí | Không dùng Areas | Dùng Areas |
|----------|------------------|------------|
| Controllers | Tất cả nằm chung `Controllers/` | Tách riêng `Areas/Admin/Controllers/`, `Areas/Client/Controllers/` |
| Views | Tất cả nằm chung `Views/` | Mỗi area có layout riêng |
| Phân quyền | Khó áp dụng [Authorize] theo nhóm | Dễ gắn [Authorize(Roles="Admin")] cho cả area |
| Bảo trì | Controller/View lẫn lộn | Mỗi khu vực độc lập, dễ tìm dễ sửa |
| Mở rộng | Thêm feature → phình to folder | Thêm area mới không ảnh hưởng area cũ |

### 3.2 Cách Area Routing hoạt động

```
URL: /Admin/Dashboard/Index/5

Giải phân tích:
  {area}       = "Admin"      → Areas/Admin/
  {controller} = "Dashboard"  → Controllers/DashboardController.cs
  {action}     = "Index"      → public IActionResult Index()
  {id}         = 5            → parameter int id = 5
```

**`{area:exists}`** là route constraint: chỉ khớp nếu area có thật trong project. Nếu URL là `/FakeArea/Test` thì constraint `exists` trả `false`, route bỏ qua.

### 3.3 Tại sao mỗi Area cần _ViewStart.cshtml riêng?

File `Views/_ViewStart.cshtml` ở gốc **KHÔNG** áp dụng cho views trong `Areas/`. ASP.NET Core chỉ tìm `_ViewStart.cshtml` theo đường dẫn thư mục từ view lên root:

```
Areas/Admin/Views/Dashboard/Index.cshtml
  ↑ tìm: Areas/Admin/Views/Dashboard/_ViewStart.cshtml (không có)
  ↑ tìm: Areas/Admin/Views/_ViewStart.cshtml             ✅ TÌM THẤY
  (dừng, KHÔNG tiếp tục lên Views/_ViewStart.cshtml ở gốc)
```

### 3.4 Cách Bootstrap Responsive hoạt động

Bootstrap 5 sử dụng hệ thống breakpoints:

| Breakpoint | Ký hiệu | Kích thước | Thiết bị |
|-----------|---------|-----------|---------|
| Extra small | (none) | < 576px | Điện thoại dọc |
| Small | `sm` | ≥ 576px | Điện thoại ngang |
| Medium | `md` | ≥ 768px | Tablet |
| Large | `lg` | ≥ 992px | Desktop nhỏ |
| Extra large | `xl` | ≥ 1200px | Desktop |
| XXL | `xxl` | ≥ 1400px | Desktop lớn |

**Cách áp dụng trong project:**

- **Navbar** (`navbar-expand-lg`): < 992px thu gọn thành hamburger, ≥ 992px hiển thị đầy đủ
- **Sidebar** (`d-none d-lg-flex`): < 992px ẩn sidebar, hiện nút burger → mở offcanvas
- **Cards** (`col-lg-4 col-md-6`): 3 cột desktop, 2 cột tablet, 1 cột mobile
- **Offcanvas** (`offcanvas-start`): sidebar trượt từ trái khi bấm nút burger trên mobile

---

## 4. Danh sách file đã tạo/sửa

| STT | File | Thao tác | Mô tả |
|-----|------|----------|-------|
| 1 | `Program.cs` | Sửa | Thêm area route trước default route |
| 2 | `Controllers/HomeController.cs` | Sửa | Thêm action `Pricing()` |
| 3 | `Areas/Admin/Controllers/DashboardController.cs` | Mới | [Area("Admin")] |
| 4 | `Areas/Client/Controllers/DashboardController.cs` | Mới | [Area("Client")] |
| 5 | `Areas/Admin/Views/_ViewStart.cshtml` | Mới | Layout path đầy đủ |
| 6 | `Areas/Admin/Views/_ViewImports.cshtml` | Mới | @using + @addTagHelper |
| 7 | `Areas/Admin/Views/Shared/_AdminLayout.cshtml` | Mới | Sidebar tối + offcanvas |
| 8 | `Areas/Admin/Views/Dashboard/Index.cshtml` | Mới | Admin Dashboard (4 cards) |
| 9 | `Areas/Client/Views/_ViewStart.cshtml` | Mới | Layout path đầy đủ |
| 10 | `Areas/Client/Views/_ViewImports.cshtml` | Mới | @using + @addTagHelper |
| 11 | `Areas/Client/Views/Shared/_ClientLayout.cshtml` | Mới | Sidebar xanh + offcanvas |
| 12 | `Areas/Client/Views/Dashboard/Index.cshtml` | Mới | Client Dashboard (3 cards) |
| 13 | `Views/Shared/_Layout.cshtml` | Sửa | Public navbar + SEO |
| 14 | `Views/Home/Index.cshtml` | Sửa | Landing page (hero + features) |
| 15 | `Views/Home/Pricing.cshtml` | Mới | 3 gói SaaS |
| 16 | `wwwroot/css/site.css` | Sửa | Sidebar, responsive, themes |

---

## 5. SEO cơ bản đã áp dụng

| Yếu tố | Cách áp dụng |
|---------|-------------|
| `<html lang="vi">` | Khai báo ngôn ngữ tiếng Việt |
| `<title>` | Mỗi trang set `ViewData["Title"]` |
| `<meta description>` | Mỗi trang public set `ViewData["Description"]` |
| Heading | h1 → h2 → h3 đúng thứ tự, mỗi trang 1 h1 |
| Semantic HTML | `<header>`, `<nav>`, `<main>`, `<footer>`, `<section>` |
| Responsive | `<meta name="viewport">` + Bootstrap grid |

---

## 6. Lưu ý quan trọng

> **KHÔNG thêm Bootstrap CDN** — project đã có Bootstrap 5 trong `wwwroot/lib/bootstrap`. 
> Chèn CDN trùng sẽ gây xung đột version, giao diện nhảy lung tung.

> **Area controllers CHƯA có `[Authorize]`** — sẽ thêm ở Phần 3 (Authentication). 
> Hiện tại ai cũng truy cập được `/Admin/Dashboard` và `/Client/Dashboard`.
