# PHẦN 5: BUỔI HỌC & TÀI LIỆU (SESSIONS & MATERIALS)

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến  
> **Ngày tạo**: 30/09/2026  
> **Phần trước**: Phần 4 (CRUD Course, AJAX Catalog)

---

## 0. Mở rộng Model & Migration

Phần 1 thiết kế ban đầu `Material.cs` chỉ có `FileUrl` (string) và `FileType` (enum). Phần 5 mở rộng thêm **3 cột** để lưu trữ file trực tiếp vào PostgreSQL (cột `bytea`), thay vì ghi ra ổ đĩa `wwwroot`:

| Cột mới | Kiểu C# | Kiểu DB (Postgres) | Nullable | Vai trò |
|---------|---------|------|----------|---------|
| `FileName` | `string?` | `varchar(300)` | ✅ | Tên file lưu trữ (Guid + extension gốc). Dùng làm tên khi trả về `Content-Disposition` lúc Download |
| `ContentType` | `string?` | `varchar(100)` | ✅ | MIME type (`application/pdf`, `application/vnd.openxmlformats-officedocument...`) |
| `FileData` | `byte[]?` | `bytea` | ✅ | Nội dung nhị phân của file. Null khi `FileType == Link` |

**Phân luồng dữ liệu theo FileType:**

| FileType | Dùng cột nào | Giải thích |
|----------|-------------|------------|
| `Link` (=3) | `FileUrl` | Đường dẫn bên ngoài (YouTube, Google Drive...). `FileData` = null |
| `Pdf/Docx/Video` (=0,1,2) | `FileName + ContentType + FileData` | File upload lưu trực tiếp vào DB. `FileUrl` = null |

> **Ghi chú Video:** Enum `FileType.Video = 2` tồn tại trong schema, nhưng whitelist upload *không* nhận `.mp4`. Video bắt buộc phải dùng `FileType = Link` (YouTube/Google Drive). Đây là quyết định có chủ đích: file video quá nặng cho cột `bytea`, phù hợp hơn với streaming CDN.

**Lệnh migration:**
```bash
dotnet ef migrations add AddMaterialFileStorage
dotnet ef database update
```

---

## 1. Trade-off Analysis: Lưu trữ File Upload ở đâu?

Do đặc thù dự án đang chạy trên **Render FREE (Ephemeral Disk)**, nếu ta lưu file upload vào thư mục `wwwroot/uploads` thì **mỗi khi có commit mới / server redeploy, toàn bộ file sẽ bị xóa sạch**. Đồng thời, các file trong `wwwroot` tự nhiên có URL public — ai cũng có thể tải nếu biết link, bất chấp chưa đăng ký khóa học.

| Giải pháp | Chi phí / Cài đặt | Độ bền dữ liệu | Bảo mật (Enrollment Check) | Hiệu năng Read |
|-----------|------------------|----------------|----------------------------|----------------|
| **1. Local `wwwroot`** | 0đ, quá dễ | ❌ Mất file mỗi lần redeploy (Ephemeral) | ❌ Lộ URL public | Nhanh (static file) |
| **2. DB (Cột `bytea` Postgres)** | 0đ, tận dụng Supabase | ✅ An toàn vĩnh viễn | ✅ Buộc đi qua Action Controller kiểm tra | Tốn RAM khi đọc file lớn |
| **3. AWS S3 / Supabase Storage** | Tốn công setup SDK, bucket | ✅ Cloud vô tận | ✅ Signed URL cấp quyền tạm thời | Siêu tốc |

**Quyết định chốt:** Trong phạm vi đồ án demo + ràng buộc Render Free → chọn **(2) bytea**. Production thật nên chuyển (3). Lợi ích kép: dữ liệu sống sót vĩnh viễn + ép mọi link tải file đi qua `MaterialController.Download(id)` — nơi thực thi kiểm tra enrollment/ownership.

---

## 2. Cấu trúc FileValidation.cs (Bức tường thép Upload)

```csharp
public static class FileValidation
{
    public static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".pptx", ".xlsx" };
    public const long MaxBytes = 10 * 1024 * 1024; // 10MB

    public static (bool Ok, string Error) Validate(IFormFile? file)
    {
        // → null / Length == 0  → "Vui lòng chọn file"
        // → Length > MaxBytes   → "File vượt quá 10MB"
        // → extension ngoài whitelist → "Định dạng không được hỗ trợ"
        // → FileName chứa '/' hoặc '\' → "Tên file không hợp lệ"
    }
}
```

**Nguyên tắc cốt lõi:** Không bao giờ tin `Content-Type` do browser gửi — trình duyệt hoàn toàn cho phép giả mạo MIME. Chốt chặn 100% bằng **extension whitelist phía server** (`Path.GetExtension().ToLowerInvariant()`).

**Guid Rename khi lưu:** `FileName = Guid.NewGuid() + ext`. Lý do:
- (a) Tránh trùng tên file khi nhiều teacher upload cùng `bai_giang.pdf`
- (b) Sanitize chuỗi tên khi trả về header `Content-Disposition` lúc Download — tránh ký tự đặc biệt gây lỗi HTTP header

> ⚠️ Guid rename ở đây **không phải** để chống Path Traversal, vì file không được ghi ra đĩa (lưu vào `bytea` trong DB). Path Traversal chỉ xảy ra khi ghi file vào filesystem.

**Form upload bắt buộc:** `enctype="multipart/form-data"` — thiếu thì `IFormFile` luôn nhận `null` (bẫy kinh điển #6).

---

## 3. Cơ chế chống IDOR mở rộng (Session + Material + Download)

| Tác vụ | Endpoint | Kiểm tra IDOR | Chi tiết |
|--------|----------|---------------|----------|
| **Thêm Buổi Học** | `POST /Session/Create` | `course.TeacherId != userId` → Forbid | Teacher A không thể nhét session vào khóa Teacher B (F12 đổi courseId) |
| **Sửa Buổi Học** | `POST /Session/Edit` | `session.Course.TeacherId != userId` → Forbid | Load session → Include Course → kiểm tra ownership |
| **Xóa Buổi Học** | `POST /Session/Delete` | `session.Course.TeacherId` | Chiếu ngược từ entity về Course owner |
| **Thêm Tài liệu** | `POST /Material/Create` | `course.TeacherId != userId` → Forbid | Y hệt Create Session — teacher A nhét file vào khóa Teacher B |
| **Xóa Tài liệu** | `POST /Material/Delete` | `material.Course.TeacherId` | Chiếu ngược entity.Course.TeacherId |
| **Tải File (Đặc biệt)** | `GET /Material/Download` | Kiểm tra kép (Dual Authority) | 1. Bạn có phải Teacher chủ khóa?<br/>2. Hoặc Student đã join bảng Enrollments?<br/>→ Đều không = 403 Forbid |

**Lớp bảo vệ hiển thị tại Details:** Ngoài việc chặn ở endpoint, tầng View cũng tham gia bảo vệ. `CourseController.Details` chỉ load và render section Buổi học + Tài liệu khi `isEnrolled || isOwner`. Student chưa đăng ký → không render nội dung, chỉ hiển thị CTA "🔒 Đăng ký khóa học để xem nội dung". Đây là lớp bảo vệ UX + Defense-in-Depth ở tầng View.

---

## 4. PRG Pattern & Server-side Validation

Mọi form tạo/sửa (Session, Material) đều thi hành triệt để **PRG Pattern** (Post → Redirect → Get):
- Thay vì `return View()` ở cuối POST action → `return RedirectToAction("Details", "Course")` 
- Chặt đứt lỗi F5 duplicate submit

**Validation cứng trên Server:** `if (model.EndTime <= model.StartTime) AddModelError(...)` — cấm vĩnh viễn kỹ năng nộp buổi học nghịch thời gian, ngay cả khi Hacker dùng Postman tắt JS front-end.

**DateTime xử lý:** Mọi thời gian từ form `<input type="datetime-local">` đều được ép `DateTime.SpecifyKind(..., DateTimeKind.Utc)` trước khi lưu DB — tránh lỗi Npgsql "Cannot write DateTime with Kind=Local/Unspecified".

---

## 5. Quy tắc kỹ thuật chung (Session + Material)

- `DateTime.UtcNow` cho `CreatedAt` / `UploadedAt` — tuyệt đối không `DateTime.Now`
- `AsNoTracking()` cho mọi query chỉ đọc (list session, list material trong Details)
- Global `AutoValidateAntiforgeryTokenAttribute` đã bảo vệ tất cả form POST (bao gồm form upload)
- Section **Bài tập** (Assignment) trong Details hiện còn placeholder → sẽ hoàn thiện ở Phần 6

---

## 6. Danh sách tệp tin tạo mới & sửa đổi (Phần 5)

| STT | File | Thao tác | Mô tả |
|-----|------|----------|-------|
| 1 | `Models/Material.cs` | Sửa | Thêm 3 cột: `FileName`, `ContentType`, `FileData` (bytea) |
| 2 | `Data/Migrations/..._AddMaterialFileStorage.cs` | Mới | EF Core migration cho 3 cột mới |
| 3 | `Utils/FileValidation.cs` | Mới | Utility kiểm tra upload: whitelist extension, max 10MB, chống path traversal |
| 4 | `Models/ViewModels/SessionViewModel.cs` | Mới | ViewModel tạo/sửa buổi học (StartTime, EndTime, MeetingUrl...) |
| 5 | `Models/ViewModels/MaterialCreateViewModel.cs` | Mới | ViewModel upload file / nhập link |
| 6 | `Models/ViewModels/CourseDetailsViewModel.cs` | Mới | ViewModel tổng hợp Course + Sessions + Materials + flags |
| 7 | `Areas/Client/Controllers/SessionController.cs` | Mới | CRUD buổi học + IDOR ownership + EndTime > StartTime validation |
| 8 | `Areas/Client/Controllers/MaterialController.cs` | Mới | Upload-to-DB + Download kiểm tra enrollment + Delete ownership |
| 9 | `Areas/Client/Controllers/CourseController.cs` | Sửa | Nâng cấp `Details` → dùng `CourseDetailsViewModel`, load Sessions/Materials |
| 10 | `Areas/Client/Views/Session/CreateEdit.cshtml` | Mới | Form tạo/sửa buổi học (datetime-local, default start=now, end=now+1d) |
| 11 | `Areas/Client/Views/Material/Create.cshtml` | Mới | Form upload file (`enctype="multipart/form-data"`) + toggle Link/Upload |
| 12 | `Areas/Client/Views/Course/Details.cshtml` | Sửa | Thay placeholder bằng bảng Sessions + danh sách Materials + CTA cho chưa enroll |
| 13 | `Views/Shared/_Layout.cshtml` | Sửa | Fix footer dính đáy trang (flexbox `min-vh-100` + `mt-auto`) |
