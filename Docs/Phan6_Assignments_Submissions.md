# PHẦN 6: BÀI TẬP & CHẤM ĐIỂM (ASSIGNMENTS & SUBMISSIONS)

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến (**Bulb.edu**)  
> **Ngày tạo**: 05/10/2026  
> **Phần trước**: Phần 5 (Sessions & Materials) + Phần 5.5 (Design System v1)  
> **Phần sau**: Phần 7 (User Profiles & Hoàn thiện)

---

## 0. Mở rộng Model & Database Schema

Hệ thống Bài tập & Nộp bài được xây dựng với cấu trúc thực thể chặt chẽ trên Entity Framework Core:

### A. Model `Submission.cs`
Tiếp nối quyết định kiến trúc từ Phần 5 (né bẫy đĩa tạm Ephemeral Disk của Render), file nộp bài của học sinh được lưu trữ trực tiếp vào cột `bytea` trong PostgreSQL:

| Thuộc tính | Kiểu C# | Kiểu DB (Postgres) | Nullable | Mô tả & Ràng buộc |
|---|---|---|:---:|---|
| `Id` | `int` | `serial` | ❌ | Khóa chính tự tăng |
| `AssignmentId` | `int` | `integer` | ❌ | FK trỏ về `Assignment` |
| `StudentId` | `string` | `text` | ❌ | FK trỏ về `AppUser` |
| `Content` | `string?` | `varchar(5000)` | ✅ | Ghi chú văn bản của học sinh |
| `FileName` | `string?` | `varchar(255)` | ✅ | Tên file đã sanitize (Guid + extension gốc) |
| `ContentType` | `string?` | `varchar(100)` | ✅ | MIME type thực tế lúc upload |
| `FileData` | `byte[]?` | `bytea` | ✅ | Dữ liệu nhị phân của tệp nộp bài |
| `SubmittedAt` | `DateTime` | `timestamp with time zone` | ❌ | Thời điểm nộp bài (UTC) |
| `Score` | `double?` | `double precision` | ✅ | Điểm số (0 đến MaxScore). **`null` = chưa chấm** |
| `Feedback` | `string?` | `varchar(2000)` | ✅ | Lời phê của giáo viên |
| `GradedAt` | `DateTime?` | `timestamp with time zone` | ✅ | Thời điểm chấm điểm (UTC) |

> ⚠️ **Single Source of Truth (SSOT) cho trạng thái chấm bài:**  
> Trạng thái bài tập được xác định duy nhất qua biểu thức `Score != null`. **Tuyệt đối không dùng cờ boolean `IsGraded`** để tránh dư thừa và bất đồng bộ dữ liệu.

### B. Mở rộng `FileType` Enum trong `Models/Enums.cs`
Mở rộng delta so với Phần 1:
```csharp
public enum FileType
{
    Pdf = 0,
    Docx = 1,
    Video = 2,
    Link = 3,
    Pptx = 4,  // Mới mở ở P6
    Xlsx = 5   // Mới mở ở P6
}
```

### C. Unique Constraint (Ràng buộc 1 Học sinh – 1 Lần nộp)
Cấu hình Fluent API trong `ApplicationDbContext`:
```csharp
builder.Entity<Submission>()
    .HasIndex(s => new { s.AssignmentId, s.StudentId })
    .IsUnique();
```
**Chính sách một bài nộp duy nhất (No Overwrite):** Học sinh không được ghi đè bài cũ để đảm bảo tính toàn vẹn và dấu vết nộp bài.

---

## 1. Trade-off Analysis: Lưu trữ Tệp Nộp Bài

| Tiêu chí | Lưu ổ đĩa cục bộ (`wwwroot/uploads`) | Lưu DB PostgreSQL (`bytea`) | Cloud Storage (S3 / Supabase) |
|---|---|---|---|
| **Độ bền dữ liệu trên Render** | ❌ **Mất sạch file** sau mỗi lần deploy/restart | ✅ **An toàn tuyệt đối** trong DB Supabase | ✅ Bền vững vĩnh viễn |
| **Kiểm soát quyền truy cập (IDOR)** | ❌ File có URL public tĩnh, lộ link ngoài | ✅ Bắt buộc qua controller kiểm tra quyền kép | ✅ Signed URL có thời hạn |
| **Chi phí & Độ phức tạp** | Miễn phí, đơn giản | Miễn phí, tận dụng connection DB có sẵn | Cần thêm SDK, API key, cấu hình bucket |
| **Quyết định cho dự án** | **BÁC BỎ** | **CHỌN (Phù hợp đồ án demo & Render)** | Hướng mở rộng cho Production lớn |

---

## 2. Kiểm soát Upload & FileValidation

Tái sử dụng nguyên xi lớp tiện ích `FileValidation` từ Phần 5:
- **Whitelist định dạng:** `.pdf`, `.docx`, `.pptx`, `.xlsx`. (Cấm `.doc` vì là định dạng nhị phân cũ tiềm ẩn rủi ro).
- **Giới hạn dung lượng:** Tối đa 10MB (`10 * 1024 * 1024` bytes).
- **Sanitize tên file khi lưu:** `Guid.NewGuid().ToString("N") + Path.GetExtension(File.FileName).ToLowerInvariant()`.
- **Form truyền tải:** Bắt buộc `enctype="multipart/form-data"`.

---

## 3. Ma trận Chống IDOR (4 Chốt Chặn Trọng Yếu)

| Endpoint | Quyền hạn | Cơ chế kiểm tra Ownership / IDOR | Xử lý vi phạm |
|---|---|---|---|
| `POST /Assignment/Create`<br/>`POST /Assignment/Edit` | Teacher (Chủ khóa) | Kiểm tra `course.TeacherId == currentUserId` | `Forbid()` (403) |
| `POST /Assignment/Submit` | Student (Đã enroll) | Kiểm tra `Enrollments.Any(CourseId, StudentId)` + Server-side Deadline | `Forbid()` nếu chưa enroll; Báo lỗi nếu quá hạn |
| `POST /Assignment/Grade` | Teacher (Chủ khóa) | Kiểm tra `submission.Assignment.Course.TeacherId == currentUserId` | `Forbid()` (403) |
| `GET /Assignment/Download` | Quyền kép (Dual Authority) | Cho phép nếu: `isTeacherOwner || submission.StudentId == currentUserId` | `Forbid()` (403) nếu là user ngoài |

---

## 4. Quy trình Nghiệp Vụ & Cơ chế Giao Tiếp (PRG vs AJAX)

### A. Tạo & Sửa Bài Tập (`Create` / `Edit` — Teacher)
- **Tái sử dụng View:** Gộp chung vào `CreateEdit.cshtml` thông qua `AssignmentCreateViewModel`.
- **Chuẩn hóa thời gian:** Ép kiểu tường minh `DateTime.SpecifyKind(vm.Deadline, DateTimeKind.Utc)`.
- **Mô hình điều hướng:** **PRG (Post-Redirect-Get)** để chống F5 lặp form.

### B. Nộp Bài (`Submit` — Student)
- **Kiểm tra 3 lớp chống trùng lặp:**
  1. *Lớp UX View:* Nút nộp bị vô hiệu hóa (`disabled`) khi bài đã nộp.
  2. *Lớp Controller:* `AnyAsync(s => s.AssignmentId == id && s.StudentId == userId)` -> `TempData["Error"] = "Bạn đã nộp bài rồi!"`.
  3. *Lớp Database:* Bắt ngoại lệ `DbUpdateException` khi vi phạm Unique Index -> Điều hướng về `Details` an toàn.
- **Mô hình điều hướng:** **PRG** (RedirectToAction `Details`).

### C. Chấm Điểm (`Grade` — Teacher)
- **Phương thức giao tiếp:** **Bắt buộc dùng AJAX (`XMLHttpRequest`)**.
- **Lý do kỹ thuật:** Khi giáo viên chấm danh sách 20–30 học sinh, nếu dùng PRG (tải lại toàn trang) sẽ làm mất vị trí cuộn chuột (`scroll position`) và xóa sạch các ô điểm/nhận xét đang gõ dở ở các dòng khác.
- **Bảo mật:** Gửi kèm token `RequestVerificationToken` qua header HTTP.
- **Ràng buộc điểm:** `0 <= vm.Score <= assignment.MaxScore` (MaxScore truy vấn động từ DB, tuyệt đối không hardcode 100).
- **Fallback:** Nếu client không gửi header AJAX, controller hỗ trợ fallback PRG.

---

## 5. Kiến trúc View & 5 Phân Nhánh `Assignment/Details`

Để ngăn chặn triệt để nguy cơ lộ dữ liệu (Data Leakage) ở tầng View:
- **Teacher View:** Query toàn bộ bài nộp (`Submissions`) qua `AsNoTracking()`, kèm `Student` profile, tính toán `SubmittedCount` và `TotalStudents`.
- **Student View:** **CHỈ truy vấn duy nhất 1 bản ghi `Submission` của chính học sinh đó**. Không bao giờ đẩy danh sách bài của người khác xuống trình duyệt.

```
                  ┌──────────────────────────────────────────┐
                  │         GET /Assignment/Details/5        │
                  └────────────────────┬─────────────────────┘
                                       │
                      Kiểm tra Quyền & Enrollment
                                       │
            ┌──────────────────────────┴──────────────────────────┐
            ▼                                                     ▼
     [Chưa Enroll & Không phải Owner]                       [Hợp lệ]
            │                                                     │
    Redirect Course/Details                                       ├──────────────────────────┐
                                                                  ▼                          ▼
                                                             [Teacher/Owner]          [Student Enrolled]
                                                                  │                          │
                                                        Danh sách chấm bài AJAX              ├──────────────────────────┐
                                                                                             ▼                          ▼
                                                                                       [Chưa Nộp]                 [Đã Nộp]
                                                                                             │                          │
                                                                                       Form Upload File                 ├──────────────────┐
                                                                                                                        ▼                  ▼
                                                                                                                   [Chờ Chấm]          [Đã Chấm]
                                                                                                                  (Score==null)      (Score!=null)
                                                                                                                   Khóa Form,         Điểm số to,
                                                                                                                   Badge Chờ          Lời phê GV
```

---

## 6. Danh sách Tệp Tin Triển Khai (Phần 6)

| STT | Tệp tin | Thao tác | Vai trò kỹ thuật |
|---|---|---|---|
| 1 | `Models/Submission.cs` | Sửa | Cập nhật `byte[] FileData`, `FileName`, `ContentType`, `Score`, `Feedback` |
| 2 | `Models/Enums.cs` | Sửa | Bổ sung `FileType.Pptx = 4` và `FileType.Xlsx = 5` |
| 3 | `Models/ViewModels/AssignmentViewModels.cs` | Mới | `AssignmentCreateViewModel`, `AssignmentDetailsViewModel`, `GradeViewModel` |
| 4 | `Areas/Client/Controllers/AssignmentController.cs` | Mới | 8 Actions xử lý CRUD, Submit (PRG), Grade (AJAX), Download (Dual Check) |
| 5 | `Areas/Client/Views/Assignment/CreateEdit.cshtml` | Mới | Form tạo/sửa bài tập giao diện Acme Sketch (viền mảnh, FZ Caveat) |
| 6 | `Areas/Client/Views/Assignment/Details.cshtml` | Mới | View tổng hợp phân nhánh 5 trạng thái (Teacher grading list, Student submit/score) |
| 7 | `Areas/Client/Views/Course/Details.cshtml` | Sửa | Điền placeholder section "Bài tập" để kết nối luồng khóa học sang bài tập |
| 8 | `wwwroot/css/site.css` | Sửa | Bổ sung rule hiển thị bảng chấm điểm và trạng thái nộp bài chuẩn token |

---

## 7. Tiêu Chí Nghiệm Thu (Acceptance Checklist)

- [x] Upload file lưu vào DB PostgreSQL (`bytea`), không ghi ra `wwwroot`.
- [x] Download kiểm tra quyền kép: Teacher sở hữu khóa hoặc Student chính chủ mới tải được.
- [x] Nộp bài áp dụng PRG, chặn nộp lại (Unique Constraint DB + Catch `DbUpdateException`).
- [x] Chấm điểm hỗ trợ AJAX, giữ nguyên vị trí cuộn và dữ liệu đang nhập.
- [x] Điểm số kiểm tra chặt chẽ `0 <= Score <= MaxScore`.
- [x] View Details phân chia rõ rệt 5 nhánh, học sinh không bao giờ nhận được danh sách bài nộp của học sinh khác.
- [x] `dotnet build` hoàn toàn sạch lỗi (0 errors).
