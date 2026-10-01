# PHẦN 5.5: DESIGN SYSTEM v1 — NỀN MÓNG CSS & MIGRATE GIAO DIỆN

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến  
> **Ngày tạo**: 02/10/2026  
> **Phần trước**: Phần 5 (Sessions & Materials)  
> **Phần sau**: Phần 6 (Assignments & Submissions)

---

## 0. Bối cảnh & phạm vi

Từ P2–P5, giao diện dùng **màu hardcode** rải rác và **emoji Unicode** làm icon. Kết quả: mỗi màn một kiểu, sửa 1 chỗ phải sửa N nơi. Phần 5.5 làm **2 chân**:

| Chân | Việc | Phạm vi |
|---|---|---|
| **A. Đặt nền** | Khai báo 15 token trong `:root` + đúc 6 class component + thêm font Be Vietnam Pro + Bootstrap Icons | `site.css` + 3 layout `<head>` |
| **B. Quét migrate** | Thay emoji → Bootstrap Icons + hardcode hex → `var(--cm-...)` ở **tất cả view P2–P5** | ~20 file `.cshtml` |

> ⚠️ Từ **P6 trở đi**, mọi view mới sinh ra **đã dùng sẵn class** → không phải migrate nữa.

---

## 1. Bảng Design Token (`:root` trong `site.css`)

| Token | Giá trị | Vai trò |
|---|---|---|
| `--cm-primary` | `#0d47a1` | Nút chính, sidebar Client, link |
| `--cm-primary-dark` | `#1a237e` | Gradient navbar Public, footer |
| `--cm-primary-light` | `#1565c0` | Gradient sidebar Client |
| `--cm-admin-dark` | `#1a1a2e` | Gradient sidebar Admin |
| `--cm-admin-light` | `#16213e` | Gradient sidebar Admin |
| `--cm-accent` | `#ffc107` | Nút CTA vàng |
| `--cm-bg` | `#f4f6f9` | Nền trang |
| `--cm-surface` | `#ffffff` | Nền card |
| `--cm-text` | `#212529` | Chữ chính |
| `--cm-muted` | `#6c757d` | Chữ phụ |
| `--cm-success` | `#198754` | Badge Published |
| `--cm-danger` | `#dc3545` | Badge lỗi |
| `--cm-radius` | `.5rem` | Bo góc mọi card |
| `--cm-shadow` | `0 .25rem .75rem rgba(...)` | Bóng card |
| `--cm-shadow-hover` | `0 .5rem 1rem rgba(...)` | Bóng khi hover |

---

## 2. Kho Component (6 class)

| Component | Class | Dùng ở |
|---|---|---|
| Card khóa học | `.card-course` | Catalog, MyCourses, Details |
| Card số liệu | `.card-stat` + `.stat-icon` + `.stat-number` | 3 Dashboard |
| Badge trạng thái | `.badge-status-draft/published/closed` | Card + bảng quản lý |
| Nút CTA | `.btn-cta` | Landing CTA "Bắt đầu miễn phí" |
| Empty state | `.empty-state` + `i` | Catalog rỗng, MyCourses trống |
| Khung form | `.form-page` | Login, Register, Tạo/Sửa |

---

## 3. Font & Icon

```html
<!-- Thêm vào <head> CẢ 3 layout -->
<link href="https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:wght@400;500;700&display=swap" rel="stylesheet">
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
```

Bootstrap Icons là **CSS riêng cho icon**, không xung đột với Bootstrap 5 đã có trong `wwwroot/lib`.

---

## 4. Bảng Emoji → Icon đã thay

| Emoji | Icon Bootstrap | Ngữ cảnh |
|---|---|---|
| 🎓 | `bi-mortarboard-fill` | Brand navbar |
| 📚 | `bi-journal-code` | Khóa học |
| 📝 | `bi-pencil-square` | Bài tập, sửa |
| 🎥 | `bi-camera-video` | Buổi học trực tuyến |
| 🏠 | `bi-house-door` | Dashboard |
| 📊 | `bi-bar-chart` | Quản trị |
| 👤 | `bi-person-circle` | User icon |
| 👥 | `bi-people-fill` | Người dùng |
| 🔍 | `bi-search` | Duyệt khóa học |
| 🛡️ | `bi-shield-lock` | Admin brand |
| 🛠️ | `bi-gear` | Quản lý |
| 📈 | `bi-graph-up` | Thống kê |
| 🚀 | `bi-rocket-takeoff` / `bi-box-arrow-in-right` | CTA / Đăng ký |
| 💰 | `bi-cash-stack` | Bảng giá |
| 🔐 | `bi-box-arrow-in-right` | Đăng nhập |
| 🔑 | `bi-key` | Nút đăng nhập |
| ✅ | `bi-check-circle-fill` / `bi-check-circle` | Tính năng có / Đã đăng ký |
| ❌ | `bi-x-circle` | Tính năng không có |
| 🔒 | `bi-lock` | Nội dung bị khóa |
| 🗑️ | `bi-trash` | Xóa |
| ✏️ | `bi-pencil-square` | Sửa |
| 👁️ | `bi-eye` | Xem chi tiết |
| ⬇️ | `bi-download` | Tải xuống |
| 📅 | `bi-calendar-event` | Buổi học |
| 📄 | `bi-file-earmark-text` | Tài liệu |
| ⏳ | `bi-hourglass-split` | Đang xử lý |
| 🔴 | `bi-broadcast` | Đang diễn ra |
| ⏰ | `bi-clock` | Sắp diễn ra |
| 💾 | `bi-save` | Lưu thay đổi |
| ➕ | `bi-plus-circle` | Thêm mới |

---

## 5. Danh sách file tạo/sửa

| STT | File | Thao tác | Mô tả |
|---|---|---|---|
| 1 | `wwwroot/css/site.css` | Sửa | Thêm `:root` 15 token + 6 class component + font body; dọn hex hardcode |
| 2 | `Views/Shared/_Layout.cshtml` | Sửa | Thêm font + icon CDN; emoji → icon (brand, footer fix) |
| 3 | `Areas/Client/Views/Shared/_ClientLayout.cshtml` | Sửa | Thêm font + icon CDN; emoji → icon (sidebar Desktop + Mobile) |
| 4 | `Areas/Admin/Views/Shared/_AdminLayout.cshtml` | Sửa | Thêm font + icon CDN; emoji → icon (sidebar Desktop + Mobile) |
| 5 | `Views/Home/Index.cshtml` | Sửa | emoji → icon (hero CTA, 3 feature cards) + `.btn-cta` |
| 6 | `Views/Home/Pricing.cshtml` | Sửa | emoji → icon (3 gói: ✅→bi-check, ❌→bi-x, headers) |
| 7 | `Views/Account/Login.cshtml` | Sửa | emoji → icon (heading, submit button) |
| 8 | `Views/Account/Register.cshtml` | Sửa | emoji → icon (heading, select options, submit) |
| 9 | `Views/Account/AccessDenied.cshtml` | Sửa | emoji → icon (nút về trang chủ) |
| 10 | `Areas/Client/Views/Dashboard/Index.cshtml` | Sửa | emoji → icon + `.card-stat` + `.stat-icon` + `.stat-number` |
| 11 | `Areas/Admin/Views/Dashboard/Index.cshtml` | Sửa | emoji → icon + `.card-stat` (4 stat cards) |
| 12 | `Areas/Client/Views/Course/Details.cshtml` | Sửa | 23 emoji → icon (badges, buttons, JS innerHTML) |
| 13 | `Areas/Client/Views/Course/Manage.cshtml` | Sửa | emoji → icon (header, action buttons) |
| 14 | `Areas/Client/Views/Course/MyCourses.cshtml` | Sửa | emoji → icon (heading) |
| 15 | `Areas/Client/Views/Course/CreateEdit.cshtml` | Sửa | emoji → icon (header, submit button) |
| 16 | `Areas/Client/Views/Course/_CourseFormPartial.cshtml` | Sửa | emoji → text (select options) |
| 17 | `Areas/Client/Views/Course/_CourseCards.cshtml` | Sửa | emoji → icon (teacher label) |
| 18 | `Areas/Client/Views/Session/CreateEdit.cshtml` | Sửa | emoji → icon (submit button) |
| 19 | `Areas/Client/Views/Material/Create.cshtml` | Sửa | emoji → icon/text (header, select options, submit) |

---

## 6. Kết quả nghiệm thu

- [x] `:root` có đủ 15 token; `body` dùng `var(--cm-bg/text)` + font Be Vietnam Pro
- [x] 6 class component tồn tại trong `site.css`
- [x] 3 layout đều có link font + bootstrap-icons
- [x] Grep emoji (`📚📝🎥🏠📊🚀💰🔐`) trong `*.cshtml` = **0 kết quả**
- [x] Dashboard cards dùng `.card-stat` + `.stat-icon`
- [x] Landing CTA dùng `.btn-cta`
- [x] `dotnet build` = **Build succeeded**
