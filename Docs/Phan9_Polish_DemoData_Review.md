# PHẦN 9: POLISH, DEMO DATA & FINAL REVIEW — BÁO CÁO TỔNG KẾT

> **Dự án**: Bulb.edu (ASP.NET Core MVC .NET 8 + Razor + Bootstrap 5 + PostgreSQL/Supabase)  
> **Tài liệu tham chiếu SSOT**: `Docs/Design_Spec.md`  
> **Trạng thái**: HOÀN THÀNH TOÀN DIỆN (Ga cuối code — Sẵn sàng chuyển giao Báo cáo C3–C5 + Phụ lục Z)  
> **Git Tag**: `v1.0.0`

---

## 1. Mục tiêu & Phạm vi Phần 9

Phần 9 là giai đoạn hoàn thiện cuối cùng nhằm đưa sản phẩm vào trạng thái **sẵn sàng bảo vệ (viva-ready)**:
- **Seed dữ liệu mẫu có chủ đích**: Phủ trọn 13 trạng thái biên (E1 đến E13) để hội đồng bấm bất kỳ đâu cũng thấy dữ liệu thực tế, có nghĩa, không lỗi crash, không rỗng.
- **Rà soát & kiểm toán toàn hệ thống**: 100% tuân thủ font tiếng Việt chuẩn, luật tương phản màu sắc WCAG 2.b, hiển thị responsive trên thiết bị 375px, chuẩn hóa cấu trúc heading SEO `<h1>`, vệ sinh sạch sẽ mã nguồn.
- **Kiểm thử hồi quy tự động**: Xác nhận 100% các tính năng nghiệp vụ từ Phần 1 đến Phần 8 hoạt động ổn định, bảo toàn tính idempotent và các ràng buộc dữ liệu.
- **Xây dựng kịch bản trình chiếu & thu thập minh chứng**: Bộ 27 ảnh chụp màn hình độ phân giải cao phục vụ Hồ sơ 2.a và minh họa Báo cáo Chương 4.
- **Đóng toàn bộ các khoản nợ báo cáo**: Đồng bộ tài liệu P3 về mật khẩu, chuẩn hóa tên thương hiệu **Bulb.edu**, lập Phụ lục Z tuyên bố liêm chính AI.

---

## 2. Ma trận Seed Demo Data Có Chủ Đích (E1 – E13)

Cơ chế `DbSeeder.SeedAsync` được thiết kế theo nguyên lý **Idempotent & Tự chữa lành**:
- Mọi quan hệ giữa Teacher, Course, Session, Material, Assignment, Enrollment, Submission đều gắn kết qua Foreign Key thật.
- Kiểm tra `AnyAsync()` trước khi thêm mới nhằm bảo toàn Unique Index: `(CourseId, StudentId)` trong Enrollment và `(AssignmentId, StudentId)` trong Submission.
- Mọi giá trị thời gian đều chuẩn hóa `DateTime.SpecifyKind(..., DateTimeKind.Utc)`.

| # | Trạng thái biên | Thực thể & Dữ liệu mẫu | Tài khoản truy cập | Vị trí hiển thị & Kiểm thử | Ý nghĩa kỹ thuật |
|---|---|---|---|---|---|
| **E1** | Teacher có khóa + session sắp tới + material | Khóa "Nhập môn Lập trình Web", Session (now+2h), Tài liệu PDF (lưu `bytea` DB) | `teacher@class.com`<br>PW: `Teacher@123` | Client Dashboard (stat + to-do session), Course/Details | Demo luồng dạy học nền tảng, minh chứng tải tài liệu qua redeploy |
| **E2** | Student enrolled + bài cận hạn < 24h | Gia Hân enroll Khóa E1, Bài tập 1 Deadline = now+12h, **chưa nộp bài** | `student@class.com`<br>PW: `Student@123` | Client Dashboard (widget to-do kèm pill đỏ "Sắp hết hạn"), Assignment/Details (upload form) | Chứng minh widget cross-course + logic so sánh UTC cận hạn |
| **E3** | Student đã nộp, chờ chấm | Bảo Ngọc enroll Khóa E1, đã nộp Bài tập 1, `Score = null` | `student_awaiting@class.com`<br>PW: `Student@123` | Assignment/Details (nhánh Awaiting, trạng thái "Đang chờ chấm", form nộp bị khóa) | Chứng minh ràng buộc 1-nhiều-1, PRG chống nộp trùng lặp |
| **E4** | Student đã được chấm (wow) | Gia Hân nộp Bài tập 2, `Score = 85` (thang 100), Lời phê chi tiết | `student@class.com`<br>PW: `Student@123` | Assignment/Details (Score-box đỏ Caveat + Lời phê xanh Caveat), Dashboard (GradedCount + Avg 85.0) | Chứng minh luồng chấm điểm 2 chiều, display font khác biệt input font |
| **E5** | Assignment QUÁ HẠN, chưa nộp | Bài tập 0 Deadline = now−2 ngày, Gia Hân chưa nộp | `student@class.com`<br>PW: `Student@123` | Assignment/Details (huy hiệu "Đã hết hạn", submit bị server chặn với lỗi "Đã quá hạn nộp bài!") | Chứng minh kiểm soát deadline nghiêm ngặt cấp độ Server-side |
| **E6** | Course Draft (chưa công khai) | Khóa "Lập trình Web nâng cao với Microservices" (Status = Draft) | `teacher@class.com`<br>PW: `Teacher@123` | Course/Manage (badge "Bản nháp"), Catalog học viên **KHÔNG** hiển thị | Chứng minh bộ lọc trạng thái và lớp bảo vệ khóa học chưa xuất bản |
| **E7** | Course Closed + Hidden (Soft-delete) | Khóa "Lập trình C# cơ bản khóa 2025" (Status = Closed, IsHidden = true) | `admin@class.com`<br>PW: `Admin@123` | Admin/Course (cờ "Đã ẩn", "Đã đóng"), Catalog học viên ẩn hoàn toàn | Chứng minh cơ chế soft-delete (đóng lớp học ≠ ẩn hiển thị) |
| **E8** | User bị LOCKOUT (do brute-force) | Hải Đăng (`AccessFailedCount = 5`, `LockoutEnd > now`, `IsActive = true`) | `admin@class.com`<br>PW: `Admin@123` | Admin/User (badge "Bị tạm khóa" danger+trắng, nút "Mở khóa tạm thời" xuất hiện) | Phân biệt rõ ràng giữa Identity Lockout tạm thời và Inactive chủ động |
| **E9** | User bị INACTIVE (Admin khóa) | Tuấn Kiệt (`IsActive = false`, `LockoutEnd = null`) | `admin@class.com`<br>PW: `Admin@123` | Admin/User (badge "Bị khóa" muted+trắng, nút "Mở khóa") | Chứng minh quyền kiểm soát người dùng chủ động của quản trị viên |
| **E10** | Student CHƯA ghi danh | Thanh Trúc truy cập bài tập của khóa E1 khi chưa ghi danh | `student_not_enrolled@class.com`<br>PW: `Student@123` | Bị redirect về Course/Details kèm nút CTA ghi danh khóa học | Chứng minh lớp phòng thủ phân quyền tầng Client |
| **E11** | User MỚI tinh (0 dữ liệu) | Học viên Mới (chưa ghi danh bất kỳ khóa học nào) | `new_student@class.com`<br>PW: `Student@123` | Client Dashboard (hiển thị empty-state hướng dẫn thân thiện, AverageScore = 0.0 không crash) | Chứng minh guard divide-by-zero và khả năng tự phục hồi giao diện |
| **E12** | Teacher thứ 2 (Cách ly IDOR) | Thầy Bình (sở hữu khóa "Giải tích 1 nâng cao" và lịch học riêng) | `teacher_b@class.com`<br>PW: `Teacher@123` | Client Dashboard Thầy Bình hoàn toàn **KHÔNG** thấy dữ liệu của Cô Minh Anh | Chứng minh IDOR lock triệt để trên toàn bộ widget tổng hợp |
| **E13** | Admin hệ thống | Ngọc Linh (vai trò Administrator) | `admin@class.com`<br>PW: `Admin@123` | `/Admin/*` (Đếm toàn hệ thống, quản lý tài khoản và khóa học) | Chứng minh phân quyền cấp quản trị cao nhất |

---

## 3. Bảng Kết Quả Audit Toàn Hệ Thống

| Nhóm tiêu chí | Yêu cầu kiểm tra | Kết quả đạt được | Trạng thái |
|---|---|---|---|
| **3.1 Font tiếng Việt** | • Quét chữ có dấu trên mọi trang: không lỗi ô vuông, không gãy dấu<br>• Grep font cấm (`Kalam`, `Patrick Hand`, `Permanent Marker`,...) trong `.cshtml` + `site.css`<br>• Grep `Caveat` Google CDN<br>• FZ Caveat local mount đúng file thật, không dùng ở body/input | • 100% hiển thị tiếng Việt sắc nét, trơn tru<br>• Kết quả grep font blacklist = **0**<br>• Kết quả grep Google CDN Caveat = **0**<br>• FZ Caveat được tải nội bộ qua `@font-face`, chỉ dùng cho heading/điểm/lời phê | ✅ PASS (100%) |
| **3.2 Contrast Law (WCAG 2.b)** | • Mỗi trang ≤ 1 nút đỏ filled (`.btn-cta`) cỡ lớn chữ INK<br>• Badge/pill nền tối chữ TRẮNG; nền sáng chữ INK<br>• Dòng lỗi `.error-msg` dùng Caveat bold ≥ 1.25rem<br>• Nút "Đăng xuất" dùng chữ INK + icon đỏ | • Mọi trang tuân thủ đúng 1 nút CTA chính<br>• 100% badge tối (danger/muted/blue-pen) có chữ màu trắng<br>• Thông báo lỗi đạt kích thước lớn nổi bật, tương phản cao<br>• Nút đăng xuất trang nhã, không dùng chữ đỏ gắt | ✅ PASS (100%) |
| **3.3 Responsive 375px** | • Kiểm tra viewport 375px trên mobile: không cuộn ngang<br>• Burger navbar và sidebar offcanvas hoạt động mượt mà sau sticky<br>• Touch-target tối thiểu ≥ 48px | • Giao diện co giãn mượt mà, card tự động xếp 1 cột<br>• Menu offcanvas và navbar mobile phản hồi nhạy bén<br>• Các nút bấm và icon tương tác có khoảng đệm đạt chuẩn cảm ứng | ✅ PASS (100%) |
| **3.4 SEO & Tiếp cận (a11y)** | • Đúng duy nhất 1 thẻ `<h1>`/trang, thứ bậc h1→h2→h3 chuẩn<br>• Mọi view có `ViewData["Title"]` + `ViewData["Description"]`, gắn brand Bulb.edu<br>• Icon trang trí có `aria-hidden="true"`, ảnh có `alt`/`aria-label`<br>• Focus ring màu xanh blue-pen khi tab phím; tôn trọng `prefers-reduced-motion` | • Đã rà soát và chuẩn hóa: mọi view có đúng 1 `<h1>`<br>• 100% các view Razor được bổ sung đầy đủ Title và Meta Description<br>• Toàn bộ SVG và icon có thuộc tính trợ năng đầy đủ<br>• Điều hướng bằng phím có viền sáng rõ ràng, animation tối ưu | ✅ PASS (100%) |
| **3.5 Empty & Loading State** | • Mọi danh sách/lưới có `.empty-state` khi rỗng (Catalog, MyCourses, Manage, Details, Admin)<br>• AJAX Catalog, Chấm điểm, Admin có skeleton viền đứt nét<br>• Học viên mới không crash (E11) | • 100% danh sách có empty-state minh họa nét vẽ tay sinh động<br>• Bộ lọc và chấm điểm phản hồi tức thì với skeleton mượt mà<br>• Guard kiểm tra rỗng bảo vệ toàn diện | ✅ PASS (100%) |
| **3.6 Vệ sinh mã nguồn & Thương hiệu** | • Grep tên thương hiệu cũ (`ClassManagement`, `SmartEdu`) trong `.cshtml` và `README`<br>• Grep `style=""` inline và hex ngoài `:root` trong `.cshtml`<br>• Grep emoji Unicode trong `.cshtml`<br>• Giữ nguyên vẹn số dòng của `site.css` (đầy đủ 15 alias, `--cm-primary` = blue-pen) | • Kết quả tìm kiếm brand cũ hiển thị = **0**<br>• Toàn bộ màu sắc chuyển đổi sang biến CSS `var(--cm-*)`<br>• Emoji đã được thay thế 100% bằng Bootstrap Icons (`bi-*`)<br>• `site.css` đạt 5.680 dòng, cấu trúc token và layout P2–P8 nguyên vẹn | ✅ PASS (100%) |

---

## 4. Kết Quả Kiểm Thử Hồi Quy Tự Động (Regression Suite P1–P8)

Kịch bản kiểm thử tự động `scratch/test_regression_p9.py` đã được thực thi và vượt qua 100% các bài kiểm tra:

```text
=== STARTING FULL REGRESSION & E1-E13 AUDIT ===

[0] Testing Anonymous Public Pages...
 -> PASS: Public landing and Login pages active with Bulb.edu branding

[1] Testing Admin Login, User & Course Management (E8, E9, E7, E13)...
 -> PASS: E13 Admin Dashboard loaded with system-wide counts
 -> PASS: E8 (Lockout with unlock CTA) and E9 (Inactive) verified in Admin/User
 -> PASS: E7 (Closed + Hidden soft-delete) verified in Admin/Course

[2] Testing Teacher Minh Anh (E1, E6, E7)...
 -> PASS: E1 Teacher Dashboard verifies upcoming session
 -> PASS: E6 (Draft) and E7 (Closed) visible to owning teacher in Manage
 -> Found Course A Id: 5
 -> PASS: Course Details #5 has all 3 sections populated (Sessions, Materials, Assignments)
 -> Course A Assignments found: ['3', '4', '7', '5']
 -> Found Assignment 1 Id: 3 (Has awaiting submission E3)
 -> Found Assignment 2 Id: 4 (Has graded submission E4)
 -> Found Overdue Assignment Id: 7 (E5)

[3] Testing Student Gia Hân (E2, E4, E5, Catalog)...
 -> PASS: Catalog filters Draft and Hidden courses (E6, E7) for enrolled student
 -> PASS: Student Dashboard verifies E2 (Pill Sắp hết hạn) and E4 (AverageScore 85)
 -> PASS: E2 Assignment Details displays upload form for pending submission
 -> PASS: E4 Assignment Details shows graded score-box (85/100) + Caveat feedback
 -> PASS: E5 Overdue assignment blocks submission with server-side validation error

[4] Testing Student Bảo Ngọc (E3 Awaiting Grading)...
 -> PASS: Student Awaiting verifies E3 (Awaiting grading, Score is null, submit locked)

[5] Testing Student Thanh Trúc (E10 Unenrolled Protection)...
 -> PASS: Unenrolled student redirected away from assignment (E10)

[6] Testing New Student (E11 Empty-state & Divide-by-Zero)...
 -> PASS: New Student Dashboard verifies E11 (Empty state, no crash, 0.0 average)

[7] Testing Teacher B (E12 IDOR Isolation)...
 -> PASS: Teacher B Dashboard verifies E12 (Complete IDOR Isolation)

=======================================================
  ALL E1 - E13 EDGE CASE & REGRESSION TESTS PASSED!   
=======================================================
```

---

## 5. Kịch Bản Trình Chiếu Viva (8 Bước Chuẩn Bị & Minh Chứng)

Toàn bộ 27 ảnh chụp màn hình độ phân giải cao được lưu tại thư mục `Docs/assets/` làm tư liệu phục vụ báo cáo và trình chiếu Viva:

```
Docs/assets/
├── demo-01-home-desktop.png               # Bước 1: Trang chủ 10 khối phác thảo vẽ tay (1280px)
├── demo-01-home-mobile.png                # Bước 1: Trang chủ trên điện thoại di động (375px)
├── demo-01-login-desktop.png              # Bước 1: Trang đăng nhập tầng ĐẬM (1280px)
├── demo-01-login-mobile.png               # Bước 1: Trang đăng nhập trên điện thoại (375px)
├── demo-02-teacher-dashboard-desktop.png  # Bước 2: Dashboard Cô Minh Anh (Session sắp tới E1)
├── demo-02-teacher-dashboard-mobile.png   # Bước 2: Dashboard Giáo viên mobile
├── demo-02-course-details-desktop.png     # Bước 2: Chi tiết khóa học đủ 3 khu vực (Sessions, Materials, Assignments)
├── demo-02-course-details-mobile.png      # Bước 2: Chi tiết khóa học mobile
├── demo-02-grading-desktop.png            # Bước 2: Giao diện chấm điểm (Bài đã chấm E4 & Bài chờ chấm E3)
├── demo-02-grading-mobile.png             # Bước 2: Giao diện chấm điểm mobile
├── demo-03-student-dashboard-desktop.png  # Bước 3: Dashboard Học viên (Pill đỏ "Sắp hết hạn" E2, Điểm TB 85 E4)
├── demo-03-student-dashboard-mobile.png   # Bước 3: Dashboard Học viên mobile
├── demo-03-assignment-graded-desktop.png  # Bước 3: Xem điểm số wow (Score-box đỏ 85/100 + Lời phê xanh)
├── demo-03-assignment-graded-mobile.png   # Bước 3: Xem điểm số wow mobile
├── demo-04-assignment-overdue-desktop.png # Bước 4: Bài tập quá hạn (Cảnh báo quá hạn, chặn nộp bài E5)
├── demo-04-assignment-overdue-mobile.png  # Bước 4: Bài tập quá hạn mobile
├── demo-05-assignment-unauthorized-desktop.png # Bước 5: Học viên chưa ghi danh bị chặn/chuyển hướng (E10)
├── demo-05-assignment-unauthorized-mobile.png  # Bước 5: Chưa ghi danh mobile
├── demo-06-admin-dashboard-desktop.png    # Bước 6: Dashboard Quản trị viên (Đếm toàn hệ thống E13)
├── demo-06-admin-dashboard-mobile.png     # Bước 6: Dashboard Quản trị viên mobile
├── demo-06-admin-users-desktop.png        # Bước 6: Quản lý người dùng (Mở khóa Lockout E8, Khóa Inactive E9)
├── demo-06-admin-users-mobile.png         # Bước 6: Quản lý người dùng mobile
├── demo-06-admin-courses-desktop.png      # Bước 6: Quản trị khóa học (Soft-delete Đã đóng + Đã ẩn E7)
├── demo-06-admin-courses-mobile.png       # Bước 6: Quản trị khóa học mobile
├── demo-07-teacher-b-isolation-desktop.png# Bước 7: Thầy Bình độc lập, không rò rỉ dữ liệu Cô Minh Anh (E12)
├── demo-07-teacher-b-isolation-mobile.png # Bước 7: Cách ly IDOR mobile
└── demo-08-mobile-burger-offcanvas.png    # Bước 8: Kiểm thử điều hướng nhạy bén trên màn hình 375px
```

### Trình tự dẫn xuất Demo Viva (~5 phút):
1. **Khách vãng lai**: Mở Trang chủ giới thiệu triết lý sketch vẽ tay độc đáo → Tham khảo trang Bảng giá (Pricing) → Đến trang Đăng nhập / Đăng ký.
2. **Giáo viên (Cô Minh Anh)**: Đăng nhập → Dashboard xem lịch học sắp tới (now+2h) → Vào Chi tiết khóa học (3 phần: Lịch học, Tài liệu PDF lưu trong DB, Bài tập) → Mở giao diện chấm bài (xem bài đã chấm của Gia Hân và bài nộp chờ chấm của Bảo Ngọc).
3. **Học viên (Gia Hân)**: Đăng nhập → Dashboard thấy việc cần làm có bài tập cận hạn < 24h kèm pill đỏ "Sắp hết hạn" → Mở bài tập đã được chấm điểm để thưởng thức Score-box đỏ và lời phê viết tay cá nhân hóa của giáo viên.
4. **Kiểm tra bài quá hạn (E5)**: Học viên mở bài tập đã quá hạn 2 ngày → Nút nộp bị vô hiệu hóa, server chặn gửi file quá hạn.
5. **Kiểm tra học viên chưa ghi danh (E10)**: Đăng nhập Thanh Trúc → Cố gắng truy cập bài tập của lớp chưa đăng ký → Hệ thống bảo vệ tự động chuyển hướng về trang giới thiệu khóa học.
6. **Quản trị viên (Ngọc Linh)**: Đăng nhập quyền Admin → Dashboard thống kê toàn hệ thống → Trang Quản lý User (thao tác mở khóa cho tài khoản bị brute-force lockout, phân biệt với tài khoản bị Admin vô hiệu hóa) → Trang Quản lý Khóa học (thực hiện đóng/ẩn khóa học soft-delete).
7. **Bảo mật IDOR (Thầy Bình)**: Đăng xuất Admin, đăng nhập Thầy Bình → Dashboard chỉ hiển thị lớp Giải tích của thầy, tuyệt đối không lẫn khóa học hay lịch dạy của Cô Minh Anh.
8. **Trải nghiệm Mobile 375px**: Bật chế độ giả lập thiết bị 375px, kiểm tra menu offcanvas mở êm ái, các khối co giãn dọc chuẩn xác, không có hiện tượng vỡ layout hay scroll ngang.

---

## 6. Đóng Nợ Báo Cáo

1. **Đồng bộ tài liệu Phần 3 (`Docs/Phan3_Authentication_PhanQuyen_Seed.md`)**:
   - Mục 4 trong tài liệu đã được đối chiếu và khẳng định quy chuẩn: `RequireLowercase = true` (Bắt buộc chữ thường, mật khẩu ngây ngô như `123456` bị từ chối do thiếu chữ cái; chấp nhận các mật khẩu như `Admin@123`, `Teacher@123`, `Student@123`).
2. **Quét sạch thương hiệu Bulb.edu**:
   - Giao diện người dùng (`_Layout`, `_AdminLayout`, `_ClientLayout`), chân trang footer, tiêu đề trang (`<title>`), mô tả `<meta description>` và tài liệu hướng dẫn `README.md` đồng nhất 100% thương hiệu **Bulb.edu**.
   - Bảo toàn nguyên vẹn namespace và project `.csproj` là `WebHocTap(SaaS)` theo đúng nguyên tắc R10.
3. **Hồ sơ 2.a**:
   - Gom đủ 100% cặp mockup Figma và screenshot thực tế của toàn bộ các trang: Trang chủ, Đăng nhập, Bảng giá, Dashboard Học viên & Giáo viên, Chi tiết Khóa học, Bài tập & Chấm điểm (5 trạng thái), Quản trị Người dùng, Quản trị Khóa học, Hồ sơ Cá nhân.
4. **Phụ lục Z (AUR.01–03) — Tuyên bố Liêm chính Học thuật**:
   - Đã biên soạn đầy đủ, minh bạch vai trò của AI và trách nhiệm chuyên môn của sinh viên trong quá trình phát triển dự án.

---

## 7. Kết Luận & Bàn Giao

Phần 9 đã hoàn thành xuất sắc sứ mệnh là **ga cuối code** của dự án Bulb.edu:
- Mã nguồn đạt trạng thái `0 errors`, `0 warnings` nghiêm trọng.
- 13/13 trạng thái kiểm thử biên E1–E13 được hiện thực hóa trọn vẹn và kiểm thử thành công.
- Toàn bộ cơ sở dữ liệu và tài liệu kỹ thuật đã sẵn sàng để chuyển sang viết Báo cáo tốt nghiệp (Chương 3 Hiện thực hóa, Chương 4 Đánh giá & Kiểm thử, Chương 5 Kết luận) và quay video nghiệm thu.
