# Bulb.edu — Nền tảng Quản lý Học tập & Lớp học Trực tuyến (SaaS)

> **Dự án**: Bulb.edu (ASP.NET Core MVC .NET 8 + Razor + Bootstrap 5 + PostgreSQL/Supabase)  
> **Ngôn ngữ**: C# (.NET 8.0) · **Database**: PostgreSQL (Entity Framework Core 8, lưu file `bytea`)  
> **Design System**: Hand-drawn Sketch System (FZ Caveat Local, 16 mẫu nét vẽ tay, Contrast Law WCAG, Responsive 375px)  
> **Phiên bản hoàn chỉnh**: `v1.0.0` (Đã hoàn thành toàn diện Phần 1 đến Phần 9)

---

## 1. Giới thiệu tổng quan

**Bulb.edu** là nền tảng quản lý học tập và lớp học trực tuyến dành cho giảng viên và học viên. Hệ thống được xây dựng theo kiến trúc đa tầng MVC, tuân thủ nguyên tắc bảo mật 12-Factor App, kiểm soát phân quyền chặt chẽ theo Role (Admin, Teacher, Student) và lớp phòng thủ chống IDOR kép.

Giao diện người dùng được thiết kế độc bản theo phong cách **phác thảo vẽ tay (Hand-drawn Sketch)** với font chữ bản địa ngữ hóa **FZ Caveat**, hệ màu mực bút vẽ tự nhiên (Ink, Blue-Pen, Pencil, Post-it, Highlighter), hiệu ứng tương tác vi mô tinh tế và bảo đảm chuẩn tương phản tiếp cận WCAG 2.b.

---

## 2. Tài khoản Demo & Ma trận Trạng thái Edge (E1 – E13)

Hệ thống tích hợp cơ chế **DbSeeder Idempotent (Tự chữa lành)**: Khi ứng dụng khởi động, cơ sở dữ liệu tự động kiểm tra và khởi tạo đầy đủ các thực thể mẫu ở mọi trạng thái biên mà không làm vỡ các ràng buộc Unique Index.

| # | Tài khoản | Mật khẩu | Vai trò | Mục đích Demo & Kiểm thử |
|---|---|---|---|---|
| **E13** | `admin@class.com` | `Admin@123` | **Admin** (Ngọc Linh) | Quản trị toàn hệ, đếm thống kê hệ thống, khóa/mở user, mở khóa lockout, đóng/ẩn khóa học |
| **E1** | `teacher@class.com` | `Teacher@123` | **Teacher** (Minh Anh) | Giáo viên chính: Khóa học đã công bố, lịch học sắp tới (now+2h), tài liệu PDF lưu trữ DB `bytea` |
| **E12** | `teacher_b@class.com` | `Teacher@123` | **Teacher** (Thầy Bình) | Giáo viên độc lập: Kiểm thử cách ly IDOR (không nhìn thấy khóa học và lịch học của Cô Minh Anh) |
| **E2** | `student@class.com` | `Student@123` | **Student** (Gia Hân) | Học viên chính: Widget to-do bài tập cận hạn (< 24h) kèm pill đỏ "Sắp hết hạn", điểm trung bình 85/100 |
| **E3** | `student_awaiting@class.com` | `Student@123` | **Student** (Bảo Ngọc) | Đã nộp bài tập 1, trạng thái chờ chấm (`Score = null`), nút nộp bị khóa chống duplicate |
| **E4** | `student@class.com` | `Student@123` | **Student** (Gia Hân) | Bài tập 2 đã được chấm (`Score = 85`), Score-box đỏ Caveat + Lời phê viết tay xanh Caveat |
| **E5** | `student@class.com` | `Student@123` | **Student** (Gia Hân) | Bài tập quá hạn (now-2 ngày): Server-side chặn nộp bài, hiển thị cảnh báo quá hạn |
| **E6** | `teacher@class.com` | `Teacher@123` | **Teacher** (Minh Anh) | Khóa học Bản nháp (Draft): Hiện trong mục Quản lý của GV, bị ẩn khỏi Catalog học viên |
| **E7** | `admin@class.com` | `Admin@123` | **Admin** / **Teacher** | Khóa học Đã đóng + Đã ẩn (Soft-delete): Quản trị viên theo dõi, Catalog không hiển thị |
| **E8** | `student_lockout@class.com` | `Student@123` | **Student** (Hải Đăng) | Bị tạm khóa do nhập sai mật khẩu 5 lần (`LockoutEnd > now`), Admin có nút mở khóa tức thì |
| **E9** | `student_inactive@class.com` | `Student@123` | **Student** (Tuấn Kiệt) | Bị khóa chủ động bởi Admin (`IsActive = false`), phân biệt rõ với trạng thái Lockout E8 |
| **E10** | `student_not_enrolled@class.com` | `Student@123` | **Student** (Thanh Trúc) | Học viên chưa ghi danh: Bị chặn và chuyển hướng kèm CTA khóa học khi cố truy cập bài tập |
| **E11** | `new_student@class.com` | `Student@123` | **Student** (Học viên Mới) | Học viên mới 0 dữ liệu: Dashboard hiển thị empty-state thân thiện, điểm trung bình 0.0 không crash |

---

## 3. Lịch sử triển khai & Cấu trúc Phân hệ (P1 – P9)

- **Phần 1: Thiết kế Cơ sở Dữ liệu & DbContext** (`Docs/Phan1_ThietKe_Models_DbContext.md`)
  - Thiết kế 7 Model: `AppUser`, `Course`, `Enrollment`, `Session`, `Material`, `Assignment`, `Submission`.
  - Cấu hình quan hệ Cascade, Unique Constraints chống trùng lặp.
- **Phần 2: Kiến trúc Area & Layout Responsive** (`Docs/Phan2_Areas_Layout_Responsive.md`)
  - 3 Areas: Public, Client, Admin.
  - Layout Sketch, Navigation Offcanvas trên mobile 375px.
- **Phần 3: Xác thực, Phân quyền & Identity Security** (`Docs/Phan3_Authentication_PhanQuyen_Seed.md`)
  - ASP.NET Core Identity, Cookie 4h (Sliding Expiration), Password Policy chuẩn mực (`RequireLowercase = true`).
  - Brute-force Lockout 5 lần thử, Self-healing Seeder.
- **Phần 4: Quản lý Khóa học, AJAX Catalog & Phòng thủ IDOR** (`Docs/Phan4_ModuleClasses_Ajax_IDOR.md`)
  - CRUD Khóa học, bộ lọc tìm kiếm AJAX mượt mà không reload trang.
  - Chống truy cập trái phép chéo tài khoản (IDOR).
- **Phần 5: Buổi học & Tài liệu Học tập** (`Docs/Phan5_Sessions_Materials.md`)
  - Lịch học trực tuyến, PRG pattern chống duplicate F5.
  - Tải lên tài liệu lưu trữ trực tiếp kiểu nhị phân `bytea` trong PostgreSQL (an toàn tuyệt đối qua redeploy).
- **Phần 6: Quản lý Bài tập, Nộp bài & Chấm điểm 2 chiều** (`Docs/Phan6_Assignments_Submissions.md`)
  - Nộp file bài tập nhị phân, kiểm soát Deadline chặt chẽ ở cấp độ Server.
  - Chấm điểm AJAX tức thì với lời phê viết tay cá nhân hóa.
- **Phần 7 & 8: Phân hệ Admin, Hồ sơ Cá nhân & Dashboard Thống kê** (`Docs/Phan7_8_Admin_Profile_Dashboard.md`)
  - Admin quản trị người dùng và khóa học, mở khóa lockout, soft-delete.
  - Dashboard tổng hợp chỉ số với truy vấn tối ưu hóa DB-side `COUNT`, widget To-Do cross-course thông minh.
- **Phần 9: Hoàn thiện, Kiểm thử Hồi quy & Chuẩn bị Báo cáo Viva** (`Docs/Phan9_Polish_DemoData_Review.md`)
  - Phủ trọn 13 trạng thái kiểm thử biên E1–E13.
  - Audit 100% tuân thủ font tiếng Việt, Contrast Law, SEO semantic `<h1>`, Responsive 375px.
  - Bộ ảnh Demo Viva 27 ảnh chuẩn bị cho Báo cáo Chương 4 và Hồ sơ 2.a.

---

## 4. Hướng dẫn Chạy Thử Nghiệm

### 4.1 Chạy trên Localhost
```bash
# Clone repository
git clone https://github.com/dovanminh2005/WebHocTap-SaaS-.git
cd WebHocTap-SaaS-

# Restore và build
dotnet restore
dotnet build

# Chạy ứng dụng (Port mặc định: 5230)
dotnet run --launch-profile http
```
Mở trình duyệt truy cập: `http://localhost:5230`

### 4.2 Chạy bộ Kiểm thử Tự động (Regression & E1-E13 Matrix)
```bash
python scratch/test_regression_p9.py
```

---

## 5. Tuyên bố Liêm chính Học thuật (Phụ lục Z - AUR.01–03)

- Dự án sử dụng mô hình trí tuệ nhân tạo (AI Assistant) hỗ trợ sinh bản thảo markup Razor, gợi ý cú pháp truy vấn LINQ và cấu trúc CSS.
- Sinh viên chịu trách nhiệm 100% về:
  1. Định hướng kiến trúc, lựa chọn giải pháp lưu trữ `bytea` để khắc phục ổ cứng tạm thời (ephemeral disk) của đám mây.
  2. Rà soát và thẩm định bảo mật IDOR kép, CSRF tokens, chính sách Password và Brute-force Lockout.
  3. Thẩm mỹ giao diện: Chuẩn hóa font chữ địa phương hóa FZ Caveat, luật tương phản WCAG 2.b, trải nghiệm responsive trên màn hình 375px.
  4. Thực hiện kiểm thử hồi quy và nghiệm thu toàn diện hệ thống.

---
*Bản quyền © 2026 Bulb.edu. Giữ toàn quyền.*
