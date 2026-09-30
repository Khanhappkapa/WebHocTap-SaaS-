# PHẦN 5: BUỔI HỌC & TÀI LIỆU (SESSIONS & MATERIALS)

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến  
> **Ngày tạo**: 30/09/2026  
> **Phần trước**: Phần 4 (CRUD Course, AJAX Catalog)

---

## 1. Trade-off Analysis: Lưu trữ File Upload ở đâu?

Do đặc thù dự án đang chạy trên **Render FREE (Ephemeral Disk)**, nếu ta lưu file upload vào thư mục `wwwroot/uploads` thì **mỗi khi có commit mới / server redeploy, toàn bộ file sẽ bị xóa sạch**. Đồng thời, các file lưu trong `wwwroot` sẽ tự nhiên có URL public (dẫn đến hệ lụy ai cũng có thể tải nếu biết link, bất chấp chưa cấu hình kiểm tra đăng ký).

Do vậy, chúng ta đối mặt với quyết định về chiến lược kiến trúc lưu trữ:

| Giải pháp | Chi phí / Cài đặt | Độ bền dữ liệu | Bảo mật (Enrollment Check) | Hiệu năng Read |
|-----------|------------------|----------------|----------------------------|----------------|
| **1. Local `wwwroot`** | Quá dễ, 0đ | ❌ Bị xóa mỗi lần mạng redeploy (Ephemeral) | ❌ Lộ URL public (Ai cũng mò tải được) | Nhanh |
| **2. DB (Cột `bytea` Postgres)** | 0đ, tận dụng luôn Server DB hiện tại | ✅ An toàn vĩnh viễn với Supabase Database | ✅ Buộc phải chui qua cổng Action Controller kiểm tra | Khá tốn RAM khi đọc file lớn |
| **3. AWS S3 / Supabase Storage** | Tốn công thiết lập SDK, cấu hình bucket | ✅ Cloud lưu vĩnh viễn, vô tận | ✅ Hỗ trợ Signed URL cấp quyền tạm thời | Siêu tốc |

**QUYẾT ĐỊNH ĐƯỢC CHỐN**: Trong phạm vi đồ án demo và ràng buộc hạ tầng Render Free, **lựa chọn (2) Lưu trực tiếp vô Database bằng column `bytea`** là hoàn hảo nhất. (Production thật thì nên đổi sang 3). Nó vừa đảm bảo dữ liệu **sống sót vĩnh viễn**, vừa ép toàn bộ đường link tải file đi qua `MaterialController.Download(id)` - nơi thực thi triệt để Security Policy (chỉ owner hoặc student đã enrolled mới được dòm).

---

## 2. Checklist Bảo mật Upload Dữ Liệu (File Auth)

Chúng ta không bao giờ tin tưởng Input từ Frontend. `FileValidation.cs` đã được dựng ra như bức tường thép:
- **Kiểm soát Extension (Whitelist):** Dứt khoát chỉ nhận `.pdf`, `.docx`, `.pptx`, `.xlsx`. Tránh tuyệt đối injection dạng `.exe`, `.js`, `.sh`.
- **Chặn Max Size:** Chỉ cho phép `<= 10MB` để tránh thảm họa DoS ăn mòn PostgreSQL.
- **Guid Rename:** Cắt vứt tên file gốc, lưu bằng `Guid.NewGuid().ToString()` tránh tấn công Path Traversal lùi thư mục `../../virus.exe` hoặc ký tự bẩn. (nhưng ta lưu lại File gốc trong Cột `FileName` để trả ra cho đẹp lúc Download).

---

## 3. Cơ Chế Chống IDOR Mở Rộng 

Không chỉ dừng lại ở Course, tôi đã nâng cấp chốt bảo vệ Sở hữu (Ownership Check) ăn sâu vào hệ thống Session & Material. Gồm:

| Tác vụ | Endpoint API | Cách nhận diện & Chặn thảm họa IDOR |
|--------|--------------|--------------------------------------|
| Thêm Buổi Học (Session) | `POST /Session/Create` | Dò ngược `CourseId` truyền vào. Nếu thuộc về Teacher khác (`course.TeacherId != userId`), đuổi ra ngay. Lỗi này thường do User F12 sửa ID ngầm định bậy bạ. |
| Xóa Nội Dung | `POST /Delete/{id}` | Lấy Session/Material ra rồi chiếu về `entity.Course.TeacherId`. Không phải của mình = cấm. |
| **Tải File (Đặc biệt)** | `GET /Material/Download` | Kiểm tra kép (Dual Authority):<br/>1. Bạn có phải là `Teacher Chủ Kênh` không?<br/>2. Hoặc, Bạn có phải là `Student đã join bảng Enrollments` không?<br/>(Nếu URL bị rò rỉ gửi cho Group khác không đăng ký học ──> 403 Forbid ngay lập tức). |

---

## 4. PRG Pattern (Post - Redirect - Get)

Khung Form tạo/sửa (Session/Material) đã được thi hành triệt để mô hình **PRGPattern**:
Thay vì `return View()` ở cuối Action Post (bị lỗi nếu User ấn phím F5 trình duyệt sẽ Submit phát sinh ra 2 buổi học trùng lặp). Ta đã trả về `return RedirectToAction("Details", "Course")`. Chặt đứng con đường Spam Database kiểu vô tình của các Teacher lóng ngóng. 
Thêm nữa, StartTime và EndTime của buổi học đã kẹp Validation cứng trên Controller `if (model.EndTime <= model.StartTime) AddModelError(...)`, cấm vĩnh viễn kỹ năng nộp buổi học vượt thời gian (ngay cả khi Hacker dùng Postman tắt JS front-end).
