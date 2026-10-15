# PHẦN 5.5: DESIGN SYSTEM v1 — NỀN MÓNG CSS & MIGRATE GIAO DIỆN

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến  
> **Ngày tạo**: 02/10/2026 (Cập nhật chuẩn hóa: 12/10/2026)  
> **Phần trước**: [Phần 5 (Sessions & Materials)](Phan5_Sessions_Materials.md)  
> **Phần sau**: [Phần 6 (Assignments & Submissions)](Phan6_Assignments_Submissions.md)  
> **Nguồn sự thật hiện tại (SSOT)**: [Docs/Design_Spec.md](Design_Spec.md) (Design Spec v2 — Sketch Học Đường 2 Tông)

---

> [!NOTE]
> **VAI TRÒ CỦA TÀI LIỆU NÀY**:  
> File này ghi lại mốc lịch sử **Phase 5.5**: Thiết lập nền móng CSS token trong `site.css` và chiến dịch quét sạch Unicode Emoji sang Bootstrap Icons trên toàn bộ ~20 view từ P2–P5.  
> Hiện tại, hệ thống đã nâng cấp lên **Design System v2 (Sketch Học Đường 2 Tông)** được quy định chi tiết tại **[Design_Spec.md](Design_Spec.md)**. Toàn bộ 15 token v1 trong file này được giữ nguyên tên thông qua cơ chế **Alias v1 → v2** trong `site.css` để đảm bảo DRY và giúp các view P2–P5 tự động tương thích giao diện mới mà không bị gãy vỡ.

---

## 0. Bối cảnh & phạm vi

Từ P2–P5, giao diện dùng **màu hardcode** rải rác và **emoji Unicode** làm icon. Kết quả: mỗi màn một kiểu, sửa 1 chỗ phải sửa N nơi. Phần 5.5 làm **2 chân**:

| Chân | Việc | Phạm vi |
|---|---|---|
| **A. Đặt nền** | Khai báo 15 token trong `:root` + đúc 6 class component + thêm font Be Vietnam Pro + Bootstrap Icons | `site.css` + 3 layout `<head>` |
| **B. Quét migrate** | Thay emoji → Bootstrap Icons + hardcode hex → `var(--cm-...)` ở **tất cả view P2–P5** | ~20 file `.cshtml` |

> ⚠️ Từ **P6 trở đi**, mọi view mới sinh ra **đã dùng sẵn class** → không phải migrate emoji nữa.

---

## 1. Bảng Design Token v1 & Cơ Chế Ánh Xạ Sang v2

### 1.1. Bảng 15 Token Gốc (v1)
Được định nghĩa ban đầu ở Phase 5.5 để chuẩn hóa màu sắc và khoảng cách:

| Token | Giá trị gốc (v1) | Vai trò ban đầu |
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

### 1.2. Cơ chế Alias v1 → v2 (Đang áp dụng trong `site.css`)
Khi nâng cấp lên **Design Spec v2 (Sketch Học Đường 2 Tông)**, để không phải sửa lại hàng chục file `.cshtml` cũ, 15 token v1 được ánh xạ trực tiếp sang token v2:

| Tên v1 | Ánh xạ sang Token v2 | Lý do & Hiệu ứng |
|---|---|---|
| `--cm-text` | `var(--cm-ink)` (`#2d2d2d`) | Màu nét chì đậm pencil black |
| `--cm-bg` | `var(--cm-paper)` (`#fdfbf7`) | Nền giấy ấm áp |
| `--cm-radius` | `var(--cm-soft-radius)` (`.5rem`) | Bo góc mềm phác thảo |
| `--cm-primary` | `var(--cm-blue-pen)` (`#2d5da1`) | Mực bút bi xanh cho link và điểm nhấn Client |
| `--cm-primary-dark` | `var(--cm-blue-pen-dark)` (`#1f4275`) | Màu xanh mực đậm |
| `--cm-primary-light` | `var(--cm-blue-pen)` | Xanh bút bi |
| `--cm-admin-dark` | `#1f1f1f` | Nền tối sổ tay Admin |
| `--cm-admin-light` | `#2d2d2d` | Nét mực tối Admin |
| `--cm-accent` | `var(--cm-accent)` (`#ff4d4d`) | Bút dạ đỏ chấm bài / điểm nhấn |
| `--cm-surface` | `var(--cm-surface)` (`#ffffff`) | Nền trắng thẻ bài tập/card |
| `--cm-muted` | `var(--cm-muted)` (`#6b6b6b`) | Chữ phụ xám nhẹ |
| `--cm-success` | `var(--cm-success)` (`#2e7d32`) | Xanh lá bài nộp / published |
| `--cm-danger` | `var(--cm-danger)` (`#c62828`) | Đỏ cảnh báo / lỗi |
| `--cm-shadow` | `var(--cm-sticker-sm)` (`2px 2px 0 var(--cm-ink)`) | Bóng sticker vẽ tay 2px |
| `--cm-shadow-hover` | `var(--cm-sticker)` (`4px 4px 0 var(--cm-ink)`) | Bóng sticker nổi 4px khi hover |

> Chi tiết toàn bộ hệ thống token v2 đầy đủ xem tại [Docs/Design_Spec.md](Design_Spec.md).

---

## 2. Kho Component (Kế thừa & mở rộng)

### 2.1. 6 Class Component nền móng từ P5.5:
| Component | Class | Dùng ở |
|---|---|---|
| Card khóa học | `.card-course` | Catalog, MyCourses, Details |
| Card số liệu | `.card-stat` + `.stat-icon` + `.stat-number` | 3 Dashboard |
| Badge trạng thái | `.badge-status-draft/published/closed` | Card + bảng quản lý |
| Nút CTA | `.btn-cta` | Landing CTA "Bắt đầu miễn phí" |
| Empty state | `.empty-state` + `i` | Catalog rỗng, MyCourses trống |
| Khung form | `.form-page` | Login, Register, Tạo/Sửa |

### 2.2. Các Component phác thảo nâng cấp ở v2 (Xem chi tiết tại Design_Spec.md):
- `.sketch-role-badge`: Huy hiệu phân quyền vẽ tay cho Teacher / Student / Admin (đạt chuẩn tương phản AAA).
- `.btn-sketch-logout`: Nút đăng xuất sketch hữu cơ chống CSRF.
- `.client-mobile-dropdown` / `.admin-mobile-dropdown`: Menu thả từ trên xuống cho mobile.
- `.sidebar-title-badge` & `.sidebar-link`: Thanh điều hướng desktop kiểu sổ tay kèm active Post-it note.
- `.sketch-sidebar-note`: Thẻ ghi chú dán ghim đỏ (`.sketch-note-pin`) ở chân sidebar.
- `.bulb-button` (`.bulb-button-primary` / `.bulb-button-ghost`): Cặp nút Đăng ký & Đăng nhập sketch trên Header.

---

## 3. Font & Icon

Hệ thống sử dụng kết hợp 2 font và 1 thư viện icon:

```html
<!-- 1. Font chữ nội dung: Be Vietnam Pro (Google Fonts) -->
<link href="https://fonts.googleapis.com/css2?family=Be+Vietnam+Pro:ital,wght@0,400;0,500;0,600;0,700;1,400&display=swap" rel="stylesheet">

<!-- 2. Hệ thống icon: Bootstrap Icons 1.11.3 (CDN) -->
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">
```

```css
/* 3. Font viết tay phác thảo: FZ Caveat (Khai báo nội bộ trong site.css) */
@font-face {
    font-family: 'FZ Caveat';
    src: url('../asset/Fz-Caveat-Regular.ttf') format('truetype');
    font-weight: 400;
    font-style: normal;
}
```

- **Be Vietnam Pro (`--cm-font-body`)**: Dùng cho toàn bộ văn bản nội dung, form input, bảng dữ liệu cần độ đọc chuẩn xác và thẳng hàng.
- **FZ Caveat (`--cm-font-hand`)**: Dùng cho logo `Bulb.edu`, tiêu đề H1/H2, Mục lục, điểm số, lời phê và các mẩu giấy ghi chú vẽ tay.
- **Bootstrap Icons**: Thay thế toàn bộ Unicode emoji, tương thích mượt mà với Bootstrap 5 nội bộ.

---

## 4. Bảng Emoji → Icon đã thay

Bảng tra cứu các emoji được thay thế bằng Bootstrap Icons ở Phase 5.5 (chuẩn xác 100%):

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
| 📅 | calendar-date.svg & calendar-grid.svg | Buổi học, Hạn nộp bài tập (Date) / Lịch trình khóa học (Grid) — Streamline Freehand SVG |
| 📄 | `bi-file-earmark-text` | Tài liệu |
| ⏳ | `bi-hourglass-split` | Đang xử lý |
| 🔴 | `bi-broadcast` | Đang diễn ra |
| ⏰ | `bi-clock` | Sắp diễn ra |
| 💾 | `bi-save` | Lưu thay đổi |
| ➕ | `bi-plus-circle` | Thêm mới |

---

## 5. Danh sách file tạo/sửa trong Phase 5.5

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

- [x] `:root` có đủ 15 token alias tương thích ngược v1 + hệ thống token v2 sketch
- [x] 6 class component cơ bản hoạt động chuẩn xác qua cơ chế biến CSS
- [x] 3 layout đều tích hợp đầy đủ font Be Vietnam Pro, FZ Caveat và Bootstrap Icons
- [x] Toàn bộ emoji Unicode (`📚📝🎥🏠📊🚀💰🔐`) trong các view P2–P5 đã được dọn sạch về 0
- [x] `dotnet build` đạt **Build succeeded** (0 lỗi, 0 cảnh báo)

---

## 7. Thư viện 16 Mẫu Sketch Component Mở Rộng (Box, Bar, Container, Button)

> **Cập nhật Phase 5.6**: Đã tích hợp 16 mẫu giao diện phác thảo vẽ tay (Acme-lite) dùng chung, sử dụng inline SVG symbols linh hoạt và co giãn tự nhiên theo khung layout (`vector-effect: non-scaling-stroke`).

### 7.1. Cấu trúc SVG Symbols Dùng Chung
Các symbols đã được định nghĩa tại `Views/Shared/_SketchSymbols.cshtml` và tích hợp sẵn vào toàn bộ Layouts (`_Layout.cshtml`, `_ClientLayout.cshtml`, `_AdminLayout.cshtml`). Khi sử dụng trên bất kỳ trang nào, bạn chỉ cần gọi thẻ `<svg class="sketch-outline"><use href="#<symbol-id>"></use></svg>`.

| Symbol ID | Archetype | Mô tả kiểu dáng |
|---|---|---|
| `#sketch-clean` | Nét mực gọn (Clean) | Viền đơn nhẹ nhàng, thanh lịch, dùng cho card và box nội dung thường ngày. |
| `#sketch-double` | Phác nét đôi (Double) | Viền kép nghệ thuật như phác họa chì than, tạo cảm giác sổ tay mỹ thuật. |
| `#sketch-note` | Giấy ghi chú (Note) | Viền giấy nhớ có độ nghiêng tự nhiên, kết hợp màu vàng Post-it (`var(--cm-postit)`). |
| `#sketch-sticker` | Sticker nổi (Heavy) | Viền dày dặn (2px) đổ bóng khối đậm (`--cm-shadow-hover`), ấn tượng cho marketing, CTA. |
| `#sketch-rule` | Đường kẻ sóng (Rule) | Nét gạch phân cách màu xanh mực bút máy (`var(--cm-primary)`). |

---

### 7.2. Danh mục 16 Mẫu Component

#### 1. Nhóm Box (4 Loại)
* **Box 1 (Nét mực gọn)**: `.sketch-surface.sketch-box` + `<use href="#sketch-clean">`
* **Box 2 (Phác nét đôi)**: `.sketch-surface.sketch-box` + `<use href="#sketch-clean">` (ngoài) & `<use href="#sketch-double">` (trong `.sketch-inner-outline`)
* **Box 3 (Giấy ghi chú)**: `.sketch-surface.sketch-box.sketch-postit` + `<use href="#sketch-note">`
* **Box 4 (Sticker nổi)**: `.sketch-surface.sketch-box.sketch-heavy` + `<use href="#sketch-sticker">`

#### 2. Nhóm Bar (4 Loại)
* **Bar 1 (Nét mực gọn)**: `.sketch-surface.sketch-bar` + `<use href="#sketch-clean">`
* **Bar 2 (Phác nét đôi)**: `.sketch-surface.sketch-bar` + `<use href="#sketch-clean">` & `<use href="#sketch-double">`
* **Bar 3 (Giấy ghi chú)**: `.sketch-surface.sketch-bar.sketch-postit` + `<use href="#sketch-note">`
* **Bar 4 (Sticker nổi)**: `.sketch-surface.sketch-bar.sketch-heavy` + `<use href="#sketch-sticker">`

#### 3. Nhóm Container (4 Loại)
* **Container 1 (Nét mực gọn)**: `.sketch-surface.sketch-container` + `<use href="#sketch-clean">`
* **Container 2 (Phác nét đôi)**: `.sketch-surface.sketch-container` + `<use href="#sketch-clean">` & `<use href="#sketch-double">`
* **Container 3 (Giấy ghi chú)**: `.sketch-surface.sketch-container.sketch-postit` + `<use href="#sketch-note">`
* **Container 4 (Sticker nổi)**: `.sketch-surface.sketch-container.sketch-heavy` + `<use href="#sketch-sticker">`

#### 4. Nhóm Button (4 Loại)
* **Nút 1 (Chính - Clean Primary)**: `.sketch-surface.sketch-button.sketch-primary` + `<use href="#sketch-clean">` (Màu đỏ cam `--cm-accent`)
* **Nút 2 (Mực đặc - Ink Solid)**: `.sketch-surface.sketch-button.sketch-ink` + `<use href="#sketch-clean">` & `<use href="#sketch-double">` (Nền đen mực `--cm-text`, chữ sáng)
* **Nút 3 (Viền xanh - Ghost Outline)**: `.sketch-surface.sketch-button.sketch-blue-outline` + `<use href="#sketch-note">` (Viền bút bi xanh `--cm-primary`)
* **Nút 4 (Sticker nổi - Heavy Action)**: `.sketch-surface.sketch-button.sketch-heavy` + `<use href="#sketch-sticker">`

---

### 7.3. Trang Trưng Bày Trực Quan (Showcase)
Toàn bộ 16 mẫu có thể được xem và thử nghiệm trực tiếp tại:
* Đường dẫn web tĩnh: `wwwroot/sketch-showcase.html` (truy cập `http://localhost:5230/sketch-showcase.html`).

