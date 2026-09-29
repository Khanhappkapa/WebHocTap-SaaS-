# PHẦN 4: MODULE CLASSES (KHÓA HỌC) - AJAX & IDOR PROTECTION

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến  
> **Ngày tạo**: 29/09/2026  
> **Phần trước**: Phần 3 (Auth, Seed, Base Controller, Patch 0.3.5)

---

## 1. Cơ chế IDOR Protection (Kiểm soát sở hữu dữ liệu)

**IDOR (Insecure Direct Object Reference)** là lỗ hổng xảy ra khi hệ thống cho phép người dùng thay đổi/xem tài nguyên của người khác chỉ bằng cách đổi ID trên URL.

### Bảng chống IDOR áp dụng tại `CourseController`:

| Action | URL Test | Lỗ hổng có thể xảy ra nếu code sai | Cách đã chặn (Ownership Check) |
|--------|----------|-----------------------------------|--------------------------------|
| **Edit** | `POST /Client/Course/Edit/5` | Giáo viên A F12 đổi tham số gửi ngầm id=5 của Giáo viên B và lưu đè. | Tìm khóa học theo ID, nếu `course.TeacherId != _userManager.GetUserId(User)` → văng `Forbid()`. |
| **Delete** | `POST /Client/Course/Delete/5` | Dùng Postman gọi AJAX xóa khóa học của người khác. | Kiểm tra `course.TeacherId != _userManager.GetUserId(User)` → văng `Forbid()`. Lỗi cố tình xóa = chặn. |
| **Manage** | `GET /Client/Course/Manage` | Teacher A thấy danh sách khóa học của tất cả thầy cô hệ thống. | `Where(c => c.TeacherId == userId)`. Chỉ Query data của chính mình. |
| **Details** | `GET /Client/Course/Details/5` | Student xem lén khóa học đang ở trạng thái Nháp (Draft) hoặc Đóng cửa. | Nếu là Student: chỉ được xem nếu `Status == Published` HOẶC đã mua/đăng ký (`isEnrolled == true`). Nếu Teacher: chỉ xem của mình. |

---

## 2. AJAX Fetch & Partial View Pattern (Duyệt Khóa Học)

**Vấn đề:** Muốn phân trang và tìm kiếm realtime trên Catalog nhưng không thả reload cái rầm làm xấu UX.

**Giải pháp ASP.NET Core:** 
- Thay vì trả về một trang mới toanh (`View()`), trả về một mẩu mã HTML cụ thể (`PartialView()`).
- Bắt lấy cái HTML đó bằng Javascript và gắn đè vào giao diện hiện tại.

### Sơ đồ tuần tự:

```
Trình duyệt Browser                          Server (CourseController)
       │                                              │
       │ Nhập chữ "Toán" vào ô tìm kiếm               │
       │ Chờ 300ms (Debounce)                          │
       │                                              │
       │ fetch(GET /Index?search=Toán&page=1)         │
       │ ── Headers: X-Requested-With: XMLHttpRequest ▶│
       │                                              │
       │                                              │ Kiểm tra Header AJAX
       │                                              │ ──> CHỈ render PartialView("_CourseCards")
       │                                              │
       │ ◀────── Trả về chuỗi HTML ───────────────────┤
       │                                              │
       │ document.getElementById('...').innerHTML     │
       │ = HTML mới                                   │
```

Tại sao dùng Partial View (Render trên Server) thay vì trả JSON (Render trên Client)?
- SEO tốt hơn.
- Không cần viết lại vòng lặp tạo div card bằng Javascript gây trùng lặp logic giao diện với C#. Tận dụng tính năng mạnh mẽ của Razor (`foreach`, `if else`) bằng C#.

---

## 3. Cơ chế đăng ký lớp (AJAX POST) & Chống trùng lặp 2 lớp

Rủi ro lớn của việc đăng ký (Enollment) là **Nhấp đúp chuột liên tục (Race Condition)**. 

### Quy trình "Đăng ký khóa học an toàn"

```csharp
[HttpPost]
[Authorize(Roles = "Student")]
[ValidateAntiForgeryToken] // Tăng cường CSRF bảo vệ 100% nhờ khai báo Global
public async Task<IActionResult> Enroll(int id) 
```
*Lưu ý: Mọi lệnh Fetch (AJAX POST) bắt buộc phải đọc giá trị Anti-Forgery Token ẩn trong form (do `@Html.AntiForgeryToken()` sinh ra) để gắn vào Header `RequestVerificationToken` gửi về.*

**Lớp bảo vệ 0 (Chặn khóa học không khả dụng):**
```csharp
// Nếu cố tình gọi fetch vào khóa học Draft hoặc Closed
if (course == null || course.Status != CourseStatus.Published) {
    return Json(new { success = false, message = "Khóa học không khả dụng." });
}
```

**Lớp bảo vệ 1 (UX Check):** 
```csharp
// Kiểm tra mềm: AnyAsync
bool isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == id && e.StudentId == userId);
// Nếu có, đẩy cảnh báo ra front-end nhẹ nhàng.
```

Nhưng giả sử User bấm F5 liên tục tại hàm Fetch, lớp bảo vệ 1 có thể không cản kịp (vì Data chưa kịp Insert thì 2 request đã chạy qua dòng kiểm tra đó). Do đó cần màng bọc DB dưới cùng:

**Lớp bảo vệ 2 (Database Constraint Backup):**
Trong Phần 1 (Models), ta đã cấu hình Unique Index cho cặp key (CourseId, StudentId).
```csharp
try {
    await _context.SaveChangesAsync();
}
catch (DbUpdateException) {  
    // Đón lõng lỗi trùng lặp khi 2 request vượt qua lớp 1
    // Database văng lỗi, EF Core truyền lỗi lên
    return Json(new { success = false, message = "Lỗi xử lý đăng ký (do click đúp)." });
}
```

---

## 4. Giải thích Data Paging (CountAsync + Skip/Take)

**Nguyên lý phân trang tối ưu:** Không tải hàng ngàn Courses ra RAM rồi đếm số lượng, mà bắt Database đếm / cắt.

```csharp
// Đếm tổng để chia trang, chạy thẳng xuống DB ra câu SQL `SELECT COUNT(*)`
int totalItems = await query.CountAsync(); 

// Phân trang bằng Skip Take, chỉ Query đúng 6 dòng ── SQL `LIMIT 6 OFFSET (trang_hien_tai * 6)`
var courses = await query
    .OrderByDescending(c => c.CreatedAt)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(...)
```
Và đặc biệt mọi Query chỉ để LẤY RA HIỂN THỊ đều phải dùng `AsNoTracking()`. Việc này báo với EF Core: "Đọc xong thì quên nó đi, không cần lưu vào Tracking Cache để kiểm tra SaveChanges", từ đó **giảm đáng kể overhead của change-tracker** giúp xử lý bộ nhớ tốt hơn rất nhiều.

---

## 5. Danh sách tệp tin tạo mới & sửa đổi (Phần 4)

| STT | File | Cập nhật | Mô tả logic |
|-----|------|----------|-------------|
| 1 | `Models/ViewModels/CourseViewModel.cs` | Mới | View model cho việc Tạo/Sửa khóa học |
| 2 | `Models/ViewModels/CourseCardViewModel.cs` | Mới | View model thu gọn dùng để render danh sách Card |
| 3 | `Models/ViewModels/CourseListViewModel.cs` | Mới | View model chuẩn bọc danh sách + thông tin phân trang (Page, Total) |
| 4 | `Areas/Client/Controllers/CourseController.cs` | Mới | Controller trung tâm điều phối CRUD + AJAX AJAX (Index, Enroll, Manage, Details) |
| 5 | `Areas/Client/Views/Course/Index.cshtml` | Mới | **Student**: Form tìm kiếm, JS fetch AJAX đè Partial |
| 6 | `Areas/Client/Views/Course/_CourseCards.cshtml` | Mới | Partial UI render lưới thẻ khóa học (chứa empty state) |
| 7 | `Areas/Client/Views/Course/MyCourses.cshtml` | Mới | **Student**: Danh sách khóa học đã tham gia (Join với bảng Enrollment) |
| 8 | `Areas/Client/Views/Course/Details.cshtml` | Mới | Trang chi tiết khóa học. View có gắn JS Fetch nút "Đăng ký khóa học" kèm lấy `__RequestVerificationToken`. |
| 9 | `Areas/Client/Views/Course/Manage.cshtml` | Mới | **Teacher**: Danh sách khóa học cá nhân của Giáo viên (Bảng). Nút Tạo/Sửa/Xóa. |
| 10| `Areas/Client/Views/Course/CreateEdit.cshtml` | Mới | Cấu trúc form tổng Tạo/Sửa. Gọi FormPartial tái sử dụng. |
| 11| `Areas/Client/Views/Course/_CourseFormPartial.cshtml`| Mới| Thân Form HTML nhập Title, Description, Trạng Thái. |
| 12| `Areas/Client/Views/Shared/_ClientLayout.cshtml`| Sửa | Cài cắm Menu Role-based: Cắt giao diện hiển thị Menu Sidebar. <br/>- **Teacher** thấy: `Quản lý khóa học`<br/>- **Student** thấy: `Khóa học của tôi`, `Duyệt khóa học` |
