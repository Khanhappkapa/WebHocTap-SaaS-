# DESIGN SPEC — SKETCH HỌC ĐƯỜNG 2 TÔNG (v2)

> Dự án: **Bulb.edu** — Website SaaS Quản Lý Lớp Học Trực Tuyến  
> Vai trò: **NGUỒN SỰ THẬT DUY NHẤT (SSOT) cho giao diện.** 3 bên đọc từ đây:  
>   • Reviewer: soi mục 1, 3, 5, 6, 9      • Coding agent: dán mục 2, code theo mục 3–6 + 9  
>   • Owner: vẽ Figma/Canva theo mục 3–4, chụp đối chiếu mục 10  
> Quy tắc DRY: CSS CHỈ sống ở mục 2 (dán vào site.css). KHÔNG copy giá trị đi nơi khác.  
> File này THAY THẾ Design system v1 (Phần 5.5). P5.5 vẫn đúng về mặt migrate emoji→icon;  
> v2 chỉ đổi GIÁ TRỊ token + thêm class mới, GIỮ NGUYÊN tên class → view P2–P5 tự restyle.  
> TRẠNG THÁI ÁP DỤNG: trang chủ (Home/Index) ĐÃ code v2 và đạt (wave 0). Các trang khác  
> áp v2 theo thứ tự các Phase. P6 sinh ra mặc sẵn v2 (tầng NHẸ).

---

## 1. BẢNG TOKEN v2 (định nghĩa ngữ nghĩa — agent dán code ở mục 2)

| Token | Giá trị | Vai trò (GIỮ ĐÚNG NGHĨA) |
|---|---|---|
| `--cm-paper` | `#fdfbf7` | Nền giấy toàn cục |
| `--cm-surface` | `#ffffff` | Nền card |
| `--cm-ink` | `#2d2d2d` | Chữ chính + border + bóng sticker ("pencil black", không pure black) |
| `--cm-muted` | `#6b6b6b` | **CHỮ PHỤ** (không phải border!) |
| `--cm-line` | `#e5e0d8` | **border/phông phụ** + dot texture |
| `--cm-accent` | `#ff4d4d` | Red correction marker: nút filled, blob, hover, tia sét logo |
| `--cm-blue-pen` | `#2d5da1` | Blue ballpoint: link, focus ring, sidebar Client |
| `--cm-blue-pen-dark` | `#1f4275` | Gradient navbar/footer public (biến thể nếu cần) |
| `--cm-postit` | `#fff9c4` | Card/widget nổi bật (sticky-note) — KHÔNG dùng làm nền badge role |
| `--cm-success` | `#2e7d32` | Badge published / on-time pill |
| `--cm-danger` | `#c62828` | Badge lỗi |
| `--cm-hand-radius` | `255px 15px 225px 15px / 15px 225px 15px 255px` | Viền vẽ tay (tầng ĐẬM) |
| `--cm-hand-radius-md` | `125px 10px 20px 185px / 25px 205px 205px 25px` | Viền tay vừa (nút) |
| `--cm-soft-radius` | `.5rem` | Viền thẳng (tầng NHẸ) |
| `--cm-hand-border` | `2px solid var(--cm-ink)` | Viền ĐẬM |
| `--cm-hand-border-thin` | `1.5px solid var(--cm-ink)` | Viền NHẸ |
| `--cm-sticker` | `4px 4px 0 var(--cm-ink)` | Bóng cắt giấy CỨNG, ĐẬM |
| `--cm-sticker-sm` | `2px 2px 0 var(--cm-ink)` | Bóng cắt giấy CỨNG, NHẸ |
| `--cm-shadow` / `--cm-shadow-hover` | `var(--cm-sticker-sm)` / `var(--cm-sticker)` | Map để class cũ tự đổi |

**ALIAS v1→v2 — BẮT BUỘC, ĐỦ 15 TÊN, KHÔNG XÓA** (view P2–P5 + 2 sidebar đang tham chiếu tên v1):

| Tên v1 | Map sang | Lý do |
|---|---|---|
| `--cm-text` | `var(--cm-ink)` | chữ chính |
| `--cm-bg` | `var(--cm-paper)` | nền trang |
| `--cm-radius` | `var(--cm-soft-radius)` | bo góc |
| `--cm-primary` | **`var(--cm-blue-pen)`** | ⚠️ PHẢI XANH, KHÔNG ink — v1 #0d47a1 dùng cho LINK + sidebar Client. Map sai = link đen + sidebar vỡ |
| `--cm-primary-dark` | `var(--cm-blue-pen-dark)` | gradient |
| `--cm-primary-light` | `var(--cm-blue-pen)` | gradient sidebar Client |
| `--cm-admin-dark` | `#1f1f1f` | sidebar Admin |
| `--cm-admin-light` | `#2d2d2d` | sidebar Admin |
| `--cm-accent` | `var(--cm-accent)` | màu đỏ nhấn |
| `--cm-surface` | `var(--cm-surface)` | nền card |
| `--cm-muted` | `var(--cm-muted)` | chữ phụ |
| `--cm-success` | `var(--cm-success)` | thành công |
| `--cm-danger` | `var(--cm-danger)` | nguy hiểm / lỗi |
| `--cm-shadow` | `var(--cm-sticker-sm)` | bóng nhẹ |
| `--cm-shadow-hover` | `var(--cm-sticker)` | bóng đậm khi hover |

---

## 2. KHỐI CSS v2 — NGUYÊN KHỐI DÁN VÀO site.css

> **CÁCH DÁN (sai là vỡ hệ thống):**  
> • CHỈ THAY khối `:root` v1 bằng `:root` v2 dưới đây.  
> • THÊM các class sketch mới + ĐỊNH NGHĨA LẠI `.card-course` / `.form-page` / `.empty-state` / `.btn-cta` / `.btn-cta-ghost` ở CUỐI file (rule sau đè rule trước theo cascade — chủ đích).  
> • TUYỆT ĐỐI KHÔNG XÓA bất kỳ rule CSS nào khác đang có: layout sidebar/offcanvas/navbar P2, `.card-stat`, `.stat-icon`, `.stat-number`, `.badge-status-*`, `.empty-state i`… Chúng VẪN HOẠT ĐỘNG nhờ ALIAS. Xóa = vỡ 2 khu vực Client/Admin.  
> • `site.css` KHÔNG cần 0 hex toàn file; hex còn sót trong rule CŨ không vỡ là CHẤP NHẬN.

```css
:root {
  --cm-paper: #fdfbf7; --cm-surface: #ffffff; --cm-ink: #2d2d2d;
  --cm-muted: #6b6b6b; --cm-line: #e5e0d8;
  --cm-accent: #ff4d4d; --cm-blue-pen: #2d5da1; --cm-blue-pen-dark: #1f4275;
  --cm-postit: #fff9c4; --cm-success: #2e7d32; --cm-danger: #c62828;
  --cm-hand-radius: 255px 15px 225px 15px / 15px 225px 15px 255px;
  --cm-hand-radius-md: 125px 10px 20px 185px / 25px 205px 205px 25px;
  --cm-soft-radius: .5rem;
  --cm-hand-border: 2px solid var(--cm-ink); --cm-hand-border-thin: 1.5px solid var(--cm-ink);
  --cm-sticker: 4px 4px 0 var(--cm-ink); --cm-sticker-sm: 2px 2px 0 var(--cm-ink);
  --cm-shadow: var(--cm-sticker-sm); --cm-shadow-hover: var(--cm-sticker);
  --cm-text: var(--cm-ink); --cm-bg: var(--cm-paper); --cm-radius: var(--cm-soft-radius);
  --cm-primary: var(--cm-blue-pen); --cm-primary-dark: var(--cm-blue-pen-dark); --cm-primary-light: var(--cm-blue-pen);
  --cm-admin-dark: #1f1f1f; --cm-admin-light: #2d2d2d;
}
body { background: var(--cm-paper); color: var(--cm-ink); font-family: "Be Vietnam Pro", system-ui, sans-serif; }
a { color: var(--cm-blue-pen); } a:hover { color: var(--cm-accent); }
/* handwriting = FZ Caveat LOCAL (đã đổi từ Caveat Google). 1 font handwriting DUY NHẤT. */
.handwritten { font-family: "FZ Caveat", cursive; }
.sketch-hand { border: var(--cm-hand-border); border-radius: var(--cm-hand-radius); box-shadow: var(--cm-sticker); }
.tilt-1 { transform: rotate(-1deg); } .tilt-2 { transform: rotate(1.5deg); }  /* CHỈ decorative, CẤM lên input/bảng/nút */
.blob { position: absolute; border-radius: 50%; background: var(--cm-accent); z-index: -1; pointer-events: none; }
.tack { color: var(--cm-accent); }                       /* thumbtack, CẤM inline color */
.sketch-arrow, .squiggly { stroke: var(--cm-ink); fill: none; stroke-width: 2.5; stroke-dasharray: 6 4; pointer-events: none; }  /* CẤM inline stroke */
@media (prefers-reduced-motion: reduce) { * { animation: none !important; transition: none !important; } }
/* TẦNG ĐẬM: .on-public = giấy + dot texture + override viền/sticker đậm cho class cũ */
.on-public { background: var(--cm-paper); background-image: radial-gradient(var(--cm-line) 1px, transparent 1px); background-size: 24px 24px; }
.on-public .card-course, .on-public .form-page, .on-public .empty-state, .on-public .btn-cta {
  border: var(--cm-hand-border); border-radius: var(--cm-hand-radius); box-shadow: var(--cm-sticker); }
/* TẦNG NHẸ (mặc định, KHÔNG bọc .on-public) — Acme-lite: viền mảnh + sticker nhẹ */
.card-course, .form-page, .empty-state { border: var(--cm-hand-border-thin); border-radius: var(--cm-soft-radius); box-shadow: var(--cm-sticker-sm); }
/* NÚT: base = FILLED accent (giữ nghĩa CTA nổi bật); ghost = trắng cho nút phụ */
.btn-cta { background: var(--cm-accent); color: var(--cm-ink); border: var(--cm-hand-border-thin);
  border-radius: var(--cm-hand-radius-md); box-shadow: var(--cm-sticker-sm); font-weight: 700;
  transition: transform .12s, box-shadow .12s; min-height: 48px; }
.btn-cta:hover { box-shadow: var(--cm-sticker); transform: translate(-2px, -2px); }
.btn-cta:active { box-shadow: none; transform: translate(2px, 2px); }
.btn-cta-ghost { background: var(--cm-surface); color: var(--cm-ink); }
/* KHỐI ĐẢO TÔNG (bottom CTA): ĐẢO nút thành nền paper + chữ ink (AAA), bóng accent */
.invert-tone { background: var(--cm-ink); color: var(--cm-paper); }
.invert-tone a { color: var(--cm-paper); }
.invert-tone .btn-cta { background: var(--cm-paper); color: var(--cm-ink); border-color: var(--cm-paper); box-shadow: 4px 4px 0 var(--cm-accent); }
.invert-tone .btn-cta:hover { box-shadow: 6px 6px 0 var(--cm-accent); transform: translate(-2px, -2px); }
.invert-tone .btn-cta-ghost { background: transparent; color: var(--cm-paper); border-color: var(--cm-paper); box-shadow: 4px 4px 0 var(--cm-accent); }
```

---

## 3. COMPONENT INVENTORY (giữ tên v1 → view cũ tự restyle)

**Class CŨ (P5.5, giữ nguyên tên):**
| Class | Vai trò | Tầng ĐẬM (`.on-public`) | Tầng NHẸ (mặc định) |
|---|---|---|---|
| `.card-course` | Card khóa học / nội dung | viền tay 2px + sticker 4px | viền mảnh 1.5px + sticker 2px |
| `.form-page` | Khung form căn giữa | đồng trên | đồng trên |
| `.empty-state` | Khối "chưa có dữ liệu" + `<i>` | đồng trên | đồng trên |
| `.btn-cta` | **NÚT FILLED đỏ, chữ INK** (≤1/trang, cỡ lớn) | viền tay + sticker | viền mảnh + sticker sm |
| `.btn-cta-ghost` | Nút phụ (trắng, viền ink, chữ ink = AAA) | — | — |
| `.btn-ink` | Nút đen chữ trắng (AAA) | — | — |
| `.card-stat` / `.stat-icon` / `.stat-number` | Số liệu dashboard | (alias giữ, không override) | như cũ |
| `.badge-status-draft` / `published` / `closed` | Trạng thái khóa học | draft→muted, published→success, closed→danger | như cũ |

**Class MỚI (đúc 1 lần, dùng mãi — thêm CUỐI `site.css` bằng `var()`, 0 hex):**
| Class | Trang xuất hiện | Vai trò / màu chữ-on-nền |
|---|---|---|
| `.sketch-input` | Mọi form | Ô nhập: surface, viền ink, chữ ink, focus blue-pen |
| `.error-msg` | Mọi form | Dòng lỗi FZ Caveat **bold ≥1.25rem** đỏ (large → pass contrast) |
| `.grade-grid` / `.assignment-grading-row` | Assignment teacher | Lưới chấm điểm thẳng hàng, responsive theo cột |
| `.assignment-score-input` / `.feedback-input` | Assignment teacher | Ô điểm 70px + textarea lời phê (Be Vietnam Pro) |
| `.badge-state-unsubmitted` / `submitted` / `graded` | Dashboard & Assignment | Unsubmitted=muted, Submitted=blue-pen, Graded=success |
| `.sticky-top` / `.sidebar` (sticky) | Header & Sidebar Client/Admin | Navigation cố định khi cuộn trang, z-index 1020 |
| `.client-topbar` / `.admin-topbar` | Layout Client & Admin | Thanh topbar header trắng (Client) hoặc đen ink (Admin) |
| `.client-sidebar` / `.admin-sidebar` | Layout Client & Admin | Thanh sidebar điều hướng dọc cố định |

---

## 4. LUẬT 2 TẦNG (bảng áp dụng theo trang — đừng trộn)

| Tầng | Trang áp dụng | Đặc tính trực quan |
|---|---|---|
| **ĐẬM** (bọc `.on-public`) | `Home/Index`, `Pricing`, `Login`, `Register`, `AccessDenied` | Viền 2px (`--cm-hand-border`), sticker 4px (`--cm-sticker`), dot texture `24px`, tilt OK trên decorative elements, có blob, mũi tên sketch-arrow, thumbtack. |
| **NHẸ** (KHÔNG bọc `.on-public`) | Dashboard (Client + Admin), Catalog (`Course/Index`), `MyCourses`, `Course/Details`, `Course/Manage`, `Course/CreateEdit`, `Session/CreateEdit`, `Material/Create`, **Assignment CreateEdit + Details** | Viền 1.5px (`--cm-hand-border-thin`), sticker 2px (`--cm-sticker-sm`), KHÔNG dot texture, KHÔNG xoay tilt trên vùng tương tác (interactive), bảng và danh sách phải quét mắt thẳng hàng; FZ Caveat chỉ dùng ở H1/H2, số điểm, lời phê, dòng lỗi. |

---

## 5. FONT LAW (ưu tiên cao nhất — chống mất dấu tiếng Việt)

- **Body / input / bảng / label / button text / dropdown item:** Bắt buộc dùng **"Be Vietnam Pro"** (Google Fonts, nhúng trong thẻ `<head>` của 3 layout).
- **Handwriting:** Bắt buộc dùng **"FZ Caveat"** (LOCAL font, file `asset/Fz-Caveat-Regular.ttf` có subset tiếng Việt đầy đủ). **1 font handwriting DUY NHẤT**.
- **Faux-bold simulation:** Vì file font local chỉ có bản Regular, `site.css` cấu hình giả lập nét đậm:
  ```css
  h1, h2, h3, h4, .fw-bold, .bold, b, strong {
      -webkit-text-stroke: 0.5px currentColor;
  }
  ```
- **Phạm vi dùng FZ Caveat:** CHỈ heading trang trí, số điểm to, lời phê display, dòng thông báo lỗi validation.
- **CẤM TUYỆT ĐỐI:** Không đặt `.handwritten` / FZ Caveat lên body text, input, bảng, label, button nhỏ.
- **BLACKLIST tuyệt đối** (không hỗ trợ `vietnamese` đầy đủ → mất dấu ễ/ệ/í/ố/ầ/ế): Kalam, Patrick Hand, Permanent Marker, Gochi Hand, Indie Flower, Gloria Hallelujah, Architects Daughter, Reenie Beanie, Handlee, Coming Soon, Rock Salt, Schoolbell, Covered By Your Grace, Just Another Hand, Nothing You Could Do, La Belle Aurore, Satisfy, Yellowtail, Sacramento, Allura, Great Vibes, Parisienne, Caveat Brush.

---

## 6. CONTRAST LAW (WCAG AA/AAA)

- **Nút đỏ `#ff4d4d` FILLED (`.btn-cta`):** Bắt buộc **chữ INK `#2d2d2d`**, CHỈ dùng cỡ lớn (≥24px hoặc ≥14pt bold), **TỐI ĐA 1 NÚT ĐỎ / TRANG**. (Tỷ lệ tương phản red+ink = 4.22:1, chỉ đạt chuẩn large text). CẤM dùng nút đỏ fill cho từng dòng trong bảng/danh sách.
- **Nút trên dòng bảng / list:** Dùng `.btn-cta-ghost` (trắng, viền ink, chữ ink = tỷ lệ AAA) hoặc `.btn-ink` (nền ink, chữ trắng paper = tỷ lệ AAA).
- **Badge / Pill nền tối:** Success `#2e7d32` / Closed / Graded = **chữ TRẮNG**. Nền sáng (Draft accent / Post-it) = **chữ INK**.
- **CẤM chữ đỏ nhỏ trên nền trắng** (tỷ lệ 3.94:1 bị fail WCAG). Chức năng Đăng xuất dùng icon đỏ + chữ INK. Dòng lỗi dùng FZ Caveat **bold ≥1.25rem** đỏ (large text pass).
- **Khối đảo tông (`.invert-tone`):** Nút CTA ĐẢO thành nền paper (`#fdfbf7`) + chữ ink (`#2d2d2d`) (đạt AAA) + bóng accent (`#ff4d4d`). Tuyệt đối không để nút đỏ chữ ink bên trong khối đen.
- **Khả năng tiếp cận:** Touch-target ≥ 48px; Focus ring dùng `var(--cm-blue-pen)`; Tuân thủ `prefers-reduced-motion` tắt hiệu ứng chuyển động khi người dùng yêu cầu.

---

## 7. BRAND & LOGO

- **Tên thương hiệu:** **Bulb.edu** (viết dạng domain nhận diện, không bọc thẻ `<a>` ra link ngoài, brand click về `/`).
- **Logo:** Hình biểu tượng người tốt nghiệp cầm tia sét bo tròn trong `wwwroot/asset/images/logo.svg` (hoặc logo bulb sketch), stroke màu mực chì than `#2d2d2d`, điểm xuyết chi tiết đỏ `#ff4d4d`.
- **Phạm vi đổi tên:** Navbar, footer, `ViewData["Title"]`, metadata, báo cáo hiển thị "Bulb.edu". **KHÔNG đổi** namespace C#, `.csproj`, tên repo hay route URL nội bộ.
- **Navigation cố định:** Header dùng `sticky-top z-3`, Sidebar dùng `position: sticky` với chiều cao `calc(100vh - 56px)` và `z-index: 1020` để cố định mượt mà khi cuộn trang.

---

## 8. LAYOUT TRANG CHỦ — CẤU TRÚC 10 KHỐI (`Views/Home/Index.cshtml`)

Header và Footer do `_Layout.cshtml` cung cấp. Toàn bộ nội dung trang chủ được bọc trong `<div class="on-public hs-home">`:

1. **Khối 1 (Topbar / Navbar):** Do `_Layout` cung cấp, logo Bulb.edu + menu điều hướng + auth links.
2. **Khối 2 (Hero 2 cột):** H1 + từ khóa `.handwritten hs-accent` ("dễ dàng") + 2 nút (`.btn-cta` "Bắt đầu miễn phí" và `.btn-cta-ghost` "Xem tính năng") + SVG `.sketch-arrow` + note viết tay + cột phải có `.blob`, khung rỗng `.hs-dashboard`, thumbtack `.tack`.
3. **Khối 3 (Stats `#stats`):** 4 thẻ số liệu `.sketch-hand hs-stat`, số lớn `.handwritten hs-stat-number` ("0+"), nhãn Be Vietnam Pro.
4. **Khối 4 (Features `#features`):** H2 + 4 thẻ tính năng `.sketch-hand hs-feature` (cột `col-md-6 col-lg-3`), trong đó có thẻ Post-it `.hs-postit tilt-1` nổi bật.
5. **Khối 5 (Zigzag `#audiences`):** 2 hàng: Hàng 1 (Ảnh trái - Chữ phải dành cho Giáo viên), Hàng 2 (Đảo cột `order-lg-*` dành cho Học viên); Khung ảnh thẳng, không xoay tilt.
6. **Khối 6 (How it works `#how`):** H2 + 3 bước số viết tay `.sketch-hand.handwritten` (1 - 2 - 3) + đường cong SVG `.squiggly` nối các bước trên desktop.
7. **Khối 7 (Pricing `#pricing`):** 3 thẻ xem nhanh (Free, Pro với `.hs-postit .hs-ribbon tilt-2`, Enterprise) -> Nút bấm dẫn sang `/Home/Pricing`.
8. **Khối 8 (Testimonials `#testimonials`):** Hộp thoại trích dẫn `.sketch-hand hs-speech` + ghi chú minh bạch *"Dữ liệu mẫu minh họa — chưa có người dùng thật"*.
9. **Khối 9 (Bottom CTA):** Khối tối `.invert-tone sketch-hand hs-bottom-cta` (không full-bleed) + H2 + nút `.btn-cta` lớn nền paper chữ ink + ghi chú "Không cần thẻ tín dụng".
10. **Khối 10 (Footer):** Do `_Layout` cung cấp, link chính sách, bản quyền Bulb.edu.

---

## 9. ACCEPTANCE (14 Tiêu Chí Nghiệm Thu)

1. `style=""` inline trong các view mới = 0.
2. Mã màu hex / stroke / fill cứng ngoài `:root` trong view = 0 (tận dụng biến CSS token).
3. Emoji Unicode trong view = 0 (100% dùng Bootstrap Icons `bi-*`).
4. Font blacklist = 0; `.handwritten` / FZ Caveat KHÔNG xuất hiện trên body, input, bảng, button nhỏ.
5. Đầy đủ 15 alias v1→v2 trong `:root`; `--cm-primary` ánh xạ sang `var(--cm-blue-pen)`.
6. `site.css` giữ nguyên các rule kế thừa từ P2 (`.card-stat`, `.badge-status-*`, offcanvas, sidebar).
7. Đúng 1 thẻ `<h1>` trên mỗi trang, phân cấp heading rõ ràng h1 → h2 → h3.
8. `.on-public` chỉ bọc đúng các trang tầng ĐẬM; khu vực Client/Admin/Assignment thuộc tầng NHẸ.
9. Tối đa 1 nút đỏ filled (`.btn-cta`) trên mỗi trang; badge tối chữ trắng; lỗi dùng `error-msg` bold ≥ 1.25rem; `.invert-tone` nút đảo nền paper chữ ink.
10. Touch target các phần tử bấm được ≥ 48px; focus ring màu blue-pen; tuân thủ `prefers-reduced-motion`.
11. Đầy đủ `ViewData["Title"]` và `ViewData["Description"]` trên mỗi view.
12. Nhận diện thương hiệu "Bulb.edu" nhất quán trên toàn bộ giao diện người dùng.
13. `dotnet build` đạt kết quả Build Succeeded (0 errors).
14. Mobile responsive: Menu burger và offcanvas hoạt động mượt mà, không bị vỡ hoặc thanh cuộn ngang ngoài ý muốn.

---

## 10. GHI CHÚ FIGMA / CANVA

- Bản vẽ Figma/Canva là nguồn tài liệu tham khảo thẩm mỹ trực quan, **Design Spec này là Nguồn Sự Thật Duy Nhất (SSOT)** cho việc thi công mã nguồn.
- Khi Figma có thành phần giao diện mới: Đo thông số (màu, góc bo, đổ bóng, font) -> Ánh xạ về Token tương ứng ở Mục 1 hoặc thêm class mới vào Mục 3. **Tuyệt đối không copy-paste CSS thô do Figma export** vào `site.css` vì sẽ gây tràn mã hex cứng và class lạ (`.frame-xxxx`).
- **Prompt mẫu cho Figma AI / Designer:** Xuất frame vector thuần túy, không chèn `<style>` hay HTML; tuân thủ font whitelist (Mục 5); tuân thủ contrast law (Mục 6); áp dụng đúng 2 tầng Đậm/Nhẹ (Mục 4); chỉ dùng bảng màu token (Mục 1).

---

## 11. CHANGELOG

- **v2 (Bản hiện tại):** Palette Sketch học đường 2 tông (Acme-lite) + Bảng 15 Token & Alias tương thích ngược + Nút đảo tông `.invert-tone` + Font FZ Caveat local có giả lập faux-bold thay thế Caveat Google + Nhận diện thương hiệu Bulb.edu + Siết chặt Contrast Law (WCAG) + Sticky navigation cho Client/Admin Topbar & Sidebar.
- **v1 (Phần 5.5):** Thiết lập 15 token đầu tiên (`#0d47a1`, `#ffc107`) + 6 class component cơ bản + Migrate toàn bộ emoji Unicode sang Bootstrap Icons + Dọn dẹp màu hex cứng sang `var(--cm-...)`.
