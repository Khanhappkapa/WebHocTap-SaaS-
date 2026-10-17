# PHẦN 7 & 8: QUẢN TRỊ HỆ THỐNG, HỒ SƠ NGƯỜI DÙNG & DASHBOARD SỐ LIỆU THẬT

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến (**Bulb.edu**)  
> **Ngăn xếp công nghệ**: ASP.NET Core MVC (.NET 8) + Entity Framework Core + PostgreSQL (Supabase) + Bootstrap 5  
> **Phần trước**: Phần 6 (Assignments & Submissions)  
> **Phần sau**: Phần 9 (Polish, Demo Data & Final Review)  
> **Chuẩn thiết kế (SSOT)**: `Docs/Design_Spec.md` — Tầng **NHẸ** (Light Tier), 0 `.on-public`, 0 inline style, 0 mã hex ngoài `:root`, FZ Caveat dùng cho heading và con số thống kê lớn, nhãn/body dùng Be Vietnam Pro.

---

## 0. Tổng quan & Ranh giới Kiến trúc (P7 vs P8)

Tài liệu này tích hợp toàn diện hai học phần cốt lõi hoàn thiện chu trình nghiệp vụ quản trị, cá nhân hóa và giám sát số liệu vận hành của hệ thống Bulb.edu:

```
  +-----------------------------------------------------------------------------------+
  |                                     BULB.EDU                                      |
  +-----------------------------------------------------------------------------------+
             |                                                       |
             v                                                       v
  +---------------------------------------+   +---------------------------------------+
  |    PHẦN 7: ADMIN & PROFILE            |   |    PHẦN 8: DASHBOARD SỐ LIỆU THẬT     |
  |    - Quản trị Người dùng (/Admin/User)|   |    - DB-side COUNT (0 in-memory)      |
  |    - Giám sát Khóa học (/Admin/Course)|   |    - Cross-course To-Do (Siết IDOR)   |
  |    - Hồ sơ cá nhân (/Client/Profile)  |   |    - Thống kê 2 chiều Teacher/Student |
  |    - Đổi mật khẩu & Khóa Lockout      |   |    - Giám sát toàn hệ thống (Admin)   |
  |    - 1 Migration (Cột IsHidden)       |   |    - 0 MIGRATION (Chỉ đọc)            |
  +---------------------------------------+   +---------------------------------------+
```

### Bảng phân định trách nhiệm kiến trúc:

| Tiêu chí | Phần 7 (Admin Module & Profile) | Phần 8 (Real Dashboard & Cross-Course To-Do) |
|---|---|---|
| **Mục đích chính** | Hành động thay đổi trạng thái quản trị & cập nhật hồ sơ người dùng | Tính toán, đếm tổng hợp và hiển thị widget việc cần làm liên khóa học |
| **Thao tác dữ liệu** | Ghi & Cập nhật trạng thái (`IsActive`, `LockoutEnd`, `Status`, `IsHidden`, `FullName`, `PasswordHash`) | **Chỉ đọc (Read-only)**, truy vấn song song với `.AsNoTracking()` |
| **Cơ sở dữ liệu** | **1 migration**: Thêm cờ `IsHidden` vào bảng `Courses` | **0 migration**: Mọi con số suy ra từ các DbSet đã tồn tại từ P1–P6 |
| **Phân quyền** | `[Authorize(Roles="Admin")]` cho Admin; `[Authorize(Roles="Teacher,Student")]` cho Profile | `[Authorize(Roles="Admin")]` cho Admin Dashboard; `[Authorize(Roles="Teacher,Student")]` cho Client |
| **Điểm then chốt** | Chống tự khóa tài khoản Admin, chống leo quyền, chống CSRF form Logout | DB-side `COUNT(*)`, siết IDOR lock To-Do theo `userId`, so sánh hạn UTC, guard divide-by-zero |

---

## 1. Mở rộng Database Schema & ViewModels

### 1.1 Mở rộng Entity `Course.cs` (Phần 7)
Để phục vụ tính năng quản trị viên ẩn/hiện khóa học mà không làm thay đổi trạng thái vòng đời học thuật (`Draft` -> `Published` -> `Closed`), cờ `IsHidden` được bổ sung:

```csharp
// Models/Course.cs
public class Course
{
    // ... các trường kế thừa từ P1-P6 ...

    /// <summary>
    /// Cờ ẩn khóa học do Admin kiểm duyệt (true = ẩn khỏi Catalog công khai)
    /// </summary>
    [Display(Name = "Bị ẩn bởi quản trị")]
    public bool IsHidden { get; set; } = false;
}
```
> **Migration tương ứng**: `20261015152339_AddCourseIsHidden.cs` thêm cột `IsHidden` kiểu `boolean` (giá trị mặc định `false`) vào bảng `Courses`.

---

### 1.2 Hệ thống ViewModels (`Models/ViewModels/`)

#### A. ViewModels Quản trị (`AdminUserViewModel.cs`, `AdminCourseViewModel.cs`)
```csharp
// Models/ViewModels/AdminUserViewModel.cs
public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEnd { get; set; }
}

// Models/ViewModels/AdminCourseViewModel.cs
public class AdminCourseViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
    public bool IsHidden { get; set; }
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### B. ViewModel Hồ sơ người dùng (`ProfileViewModel.cs`)
```csharp
// Models/ViewModels/ProfileViewModel.cs
public class ProfileViewModel
{
    public string Email { get; set; } = string.Empty; // Readonly
    public string Role { get; set; } = string.Empty;  // Readonly, hiển thị qua badge phác thảo

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string? ConfirmPassword { get; set; }
}
```

#### C. ViewModels Dashboard (`DashboardViewModels.cs` - Phần 8)
```csharp
// Models/ViewModels/DashboardViewModels.cs
public class ClientDashboardViewModel
{
    public string Role { get; set; } = string.Empty; // "Teacher" | "Student"
    public bool IsTeacher => Role == "Teacher";
    public string FullName { get; set; } = "bạn";

    // Thống kê Giáo viên (DB-side COUNT)
    public int CourseCount { get; set; }
    public int StudentCount { get; set; }
    public int AssignmentCount { get; set; }
    public int UngradedCount { get; set; }
    public int UpcomingSessionCount { get; set; }

    // Thống kê Học viên (DB-side COUNT)
    public int EnrolledCount { get; set; }
    public int PendingSubmissionCount { get; set; }
    public int GradedCount { get; set; }
    public double AverageScore { get; set; } // 0 nếu chưa có điểm (Guard divide-by-zero)

    // Khóa học gần đây (cột trái)
    public List<CourseCardViewModel> Courses { get; set; } = new();

    // Widget to-do cross-course (cột phải - gộp & sắp xếp theo WhenUtc, Take 5)
    public List<ToDoItem> ToDo { get; set; } = new();
}

public class ToDoItem
{
    public ToDoKind Kind { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime WhenUtc { get; set; }
    public string ActionUrl { get; set; } = string.Empty;
    public bool IsOverdueSoon { get; set; } // Deadline < 24h -> pill đỏ viền mực
}

public enum ToDoKind
{
    Session,
    AssignmentDue,
    SubmissionToGrade
}

public class AdminDashboardViewModel
{
    // Thống kê Người dùng
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int LockedUsers { get; set; }

    // Thống kê Khóa học
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int DraftCourses { get; set; }
    public int ClosedCourses { get; set; }
    public int HiddenCourses { get; set; }

    // Thống kê Hoạt động học tập
    public int TotalEnrollments { get; set; }
    public int TotalSubmissions { get; set; }
    public int UngradedSubmissions { get; set; }
}
```

---

## 2. Phần 7: Admin Module & Hồ Sơ Người Dùng (Admin & Profile)

### 2.1 Quản trị Người Dùng (`Areas/Admin/Controllers/UserController.cs`)

Controller kế thừa `BaseAdminController` (`[Area("Admin")]` + `[Authorize(Roles = "Admin")]`).

#### A. Ma trận Bảo vệ Tài khoản Quản trị (Self-Lockout Prevention):
Khi quản trị viên thao tác khóa tài khoản hoặc hủy kích hoạt, hệ thống bắt buộc kiểm tra định danh hiện tại:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleActive(string id)
{
    var currentUserId = _userManager.GetUserId(User);
    if (id == currentUserId)
    {
        TempData["ErrorMessage"] = "Bạn không thể tự khóa tài khoản của chính mình.";
        return RedirectToAction(nameof(Index));
    }

    var user = await _userManager.FindByIdAsync(id);
    if (user == null) return NotFound();

    user.IsActive = !user.IsActive;
    await _userManager.UpdateAsync(user);

    TempData["SuccessMessage"] = user.IsActive 
        ? $"Đã kích hoạt tài khoản {user.Email}." 
        : $"Đã vô hiệu hóa tài khoản {user.Email}.";

    return RedirectToAction(nameof(Index));
}
```

#### B. Mở khóa Lockout do đăng nhập thất bại:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> UnlockUser(string id)
{
    var user = await _userManager.FindByIdAsync(id);
    if (user == null) return NotFound();

    // Hủy thời hạn khóa và reset bộ đếm thử sai
    user.LockoutEnd = null;
    user.AccessFailedCount = 0;
    await _userManager.UpdateAsync(user);

    TempData["SuccessMessage"] = $"Đã mở khóa đăng nhập cho tài khoản {user.Email}.";
    return RedirectToAction(nameof(Index));
}
```

---

### 2.2 Giám sát Khóa Học Toàn Hệ Thống (`Areas/Admin/Controllers/CourseController.cs`)

Admin có thẩm quyền giám sát toàn bộ khóa học do bất kỳ Giáo viên nào tạo ra:
- **Đóng khóa học (`ToggleStatus`)**: Chuyển trạng thái từ `Published` sang `Closed` (ngăn học sinh mới đăng ký).
- **Ẩn khóa học (`ToggleHidden`)**: Bật/tắt cờ `IsHidden` (loại khỏi kết quả tìm kiếm ở trang Catalog mà không xóa dữ liệu học tập lịch sử).
- **Bảo vệ chống thao tác trái phép**: 100% action thay đổi trạng thái đều sử dụng phương thức `POST` kèm `[ValidateAntiForgeryToken]`.

---

### 2.3 Hồ Sơ Cá Nhân (`Areas/Client/Controllers/ProfileController.cs`)

Học sinh và Giáo viên truy cập `/Client/Profile` để quản lý thông tin:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Index(ProfileViewModel model)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return RedirectToAction("Login", "Account", new { area = "" });

    // 1. Cập nhật họ và tên
    if (user.FullName != model.FullName)
    {
        user.FullName = model.FullName;
        await _userManager.UpdateAsync(user);
    }

    // 2. Đổi mật khẩu nếu người dùng nhập mật khẩu mới
    if (!string.IsNullOrEmpty(model.NewPassword))
    {
        if (string.IsNullOrEmpty(model.CurrentPassword))
        {
            ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu hiện tại để đổi mật khẩu.");
            return View(model);
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!changeResult.Succeeded)
        {
            foreach (var error in changeResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // Tự động làm mới cookie đăng nhập sau khi đổi mật khẩu thành công
        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Cập nhật hồ sơ và đổi mật khẩu thành công!";
        return RedirectToAction(nameof(Index));
    }

    TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
    return RedirectToAction(nameof(Index));
}
```

---

### 2.4 Bảng Đối Chiếu 8 Màn Hình Mockup Figma (P7)

Toàn bộ giao diện Phần 7 đã được thiết kế và đối soát 1:1 với 8 bản mockup chuẩn:

| STT | Tên màn hình Mockup | Tệp đối chiếu (`wwwroot/8png/`) | Thành phần giao diện thực tế |
|:---:|---|---|---|
| **1** | Hồ sơ cá nhân Desktop | `Client — Hồ sơ — Desktop.png` | Form hồ sơ 2 cột, khung nét mực gọn, badge vai trò FZ Caveat |
| **2** | Hồ sơ cá nhân Mobile | `Client — Hồ sơ — Mobile.png` | Bố cục co giãn 1 cột trên màn hình nhỏ, các nút full-width dễ bấm |
| **3** | Thông báo lỗi đổi mật khẩu | `Client — Hồ sơ — Lỗi mật khẩu.png` | Khung cảnh báo lỗi nét vẽ tay, dòng chữ đỏ ink-accent rõ ràng |
| **4** | Admin - Quản lý người dùng | `Admin — Quản lý người dùng.png` | Bảng quản lý kẻ sọc, nhãn trạng thái (Active/Locked), nút thao tác nhanh |
| **5** | Admin - Người dùng rỗng | `Admin — Người dùng — Rỗng.png` | Trạng thái rỗng `.empty-state` với icon phác thảo và hướng dẫn |
| **6** | Admin - Giám sát khóa học | `Admin — Giám sát khóa học.png` | Danh sách khóa học toàn hệ thống, hiển thị số học viên và giáo viên phụ trách |
| **7** | Admin - Khóa học rỗng | `Admin — Khóa học — Rỗng.png` | Khung nét đứt phác thảo khi hệ thống chưa có dữ liệu khóa học |
| **8** | Modal xác nhận đóng khóa | `Admin — Xác nhận thao tác khóa học.png` | Modal hộp thoại viền mực đôi, nút cảnh báo rõ ràng chống bấm nhầm |

---

## 3. Phần 8: Dashboard Thật (Số Liệu DB-Side Count & Widget To-Do Cross-Course)

### 3.1 Chiến lược đếm dữ liệu (Trade-off Analysis)

| Phương pháp | Lựa chọn | Đánh giá kỹ thuật |
|---|:---:|---|
| **Load DbSet rồi `.Count()` in-memory** | ❌ **LOẠI BỎ** | Kéo toàn bộ bảng về bộ nhớ RAM của Web Server, gây nghẽn mạng, lãng phí bộ nhớ và sai sót khi có phân trang. |
| **Cache (`IMemoryCache`)** | ❌ **NGOÀI PHẠM VI** | Quy mô đồ án (demo vài chục user) chưa cần thiết; thêm phức tạp trong việc vô hiệu hóa cache (cache invalidation). |
| **DB-side `COUNT(*)` độc lập** | ✅ **LỰA CHỌN TỐI ƯU** | EF Core biên dịch trực tiếp sang câu lệnh SQL `SELECT count(*)::int`. Tận dụng tối đa chỉ mục của PostgreSQL, thời gian truy vấn dưới 80ms, mã nguồn phẳng, dễ bảo trì, hoàn toàn không bị N+1. |

> **Quy tắc bất di bất dịch**: 100% câu truy vấn đọc trên Dashboard đều sử dụng `.AsNoTracking()`.

---

### 3.2 Ma trận Siết Chặt IDOR cho Widget To-Do Cross-Course

Widget "Việc cần làm" tổng hợp sự kiện trên tất cả các khóa học của người dùng. Để ngăn chặn hoàn toàn việc rò rỉ dữ liệu chéo (IDOR), mọi truy vấn đều **bắt buộc** ràng buộc chặt chẽ với `userId`:

```csharp
var now = DateTime.UtcNow;

// 1. BUỔI HỌC SẮP TỚI (Cả Giáo viên & Học sinh)
var sessionQuery = _context.Sessions
    .AsNoTracking()
    .Where(s => s.StartTime > now)
    .Where(s => isTeacher
        ? s.Course.TeacherId == userId                               // Giáo viên: chỉ khóa mình dạy
        : s.Course.Enrollments.Any(e => e.StudentId == userId));      // Học sinh: chỉ khóa mình đã ghi danh

var upcomingSessions = await sessionQuery
    .OrderBy(s => s.StartTime)
    .Take(3)
    .Select(s => new ToDoItem
    {
        Kind = ToDoKind.Session,
        Title = s.Title,
        CourseTitle = s.Course.Title,
        WhenUtc = DateTime.SpecifyKind(s.StartTime, DateTimeKind.Utc),
        IsOverdueSoon = false,
        ActionUrl = Url.Action("Details", "Course", new { area = "Client", id = s.CourseId }) ?? ""
    })
    .ToListAsync();

// 2. BÀI TẬP CẬN HẠN CHƯA NỘP (CHỈ DÀNH CHO HỌC SINH)
List<ToDoItem> dueAssignments = new();
if (!isTeacher)
{
    dueAssignments = await _context.Assignments
        .AsNoTracking()
        .Where(a => a.Deadline > now)
        .Where(a => a.Course.Enrollments.Any(e => e.StudentId == userId))                                 // IDOR Lock
        .Where(a => !_context.Submissions.Any(s => s.AssignmentId == a.Id && s.StudentId == userId))     // Chưa nộp
        .OrderBy(a => a.Deadline)
        .Take(3)
        .Select(a => new ToDoItem
        {
            Kind = ToDoKind.AssignmentDue,
            Title = a.Title,
            CourseTitle = a.Course.Title,
            WhenUtc = DateTime.SpecifyKind(a.Deadline, DateTimeKind.Utc),
            IsOverdueSoon = a.Deadline < now.AddHours(24),                                               // Cận hạn < 24h
            ActionUrl = Url.Action("Details", "Assignment", new { area = "Client", id = a.Id }) ?? ""
        })
        .ToListAsync();
}

// 3. BÀI NỘP CHỜ CHẤM ĐIỂM (CHỈ DÀNH CHO GIÁO VIÊN)
List<ToDoItem> toGradeSubmissions = new();
if (isTeacher)
{
    toGradeSubmissions = await _context.Submissions
        .AsNoTracking()
        .Where(s => s.Score == null)                                                                     // Chưa có điểm
        .Where(s => s.Assignment.Course.TeacherId == userId)                                             // IDOR Lock
        .OrderBy(s => s.Assignment.Deadline)
        .Take(3)
        .Select(s => new ToDoItem
        {
            Kind = ToDoKind.SubmissionToGrade,
            Title = s.Assignment.Title,
            CourseTitle = s.Assignment.Course.Title,
            WhenUtc = DateTime.SpecifyKind(s.Assignment.Deadline, DateTimeKind.Utc),
            IsOverdueSoon = s.Assignment.Deadline < now.AddHours(24),
            ActionUrl = Url.Action("Details", "Assignment", new { area = "Client", id = s.AssignmentId }) ?? ""
        })
        .ToListAsync();
}

// 4. GỘP DANH SÁCH & LẤY TỐI ĐA 5 MỤC SỚM NHẤT
var allTodos = new List<ToDoItem>(upcomingSessions);
if (isTeacher) allTodos.AddRange(toGradeSubmissions);
else allTodos.AddRange(dueAssignments);

vm.ToDo = allTodos.OrderBy(t => t.WhenUtc).Take(5).ToList();
```

---

### 3.3 Chuẩn Hóa Múi Giờ UTC & Xử Lý An Toàn Phép Chia Cho 0

1. **Chuẩn hóa UTC**: Mọi phép so sánh hạn chót (`Deadline > now`, `StartTime > now`) đều sử dụng `DateTime.UtcNow`. Khi bind ra ViewModel, gán rõ `DateTime.SpecifyKind(..., DateTimeKind.Utc)` để tương thích hoàn toàn với kiểu `timestamptz` của Npgsql (PostgreSQL). Trên giao diện Razor, gọi `.ToLocalTime()` để hiển thị đúng múi giờ người dùng.
2. **Guard Divide-By-Zero**: Khi tính điểm trung bình học tập của học viên, nếu học viên chưa có bài nào được chấm, LINQ `AverageAsync()` sẽ ném ngoại lệ nếu tập hợp rỗng. Hệ thống sử dụng chốt chặn an toàn:
   ```csharp
   var gradedQuery = _context.Submissions
       .AsNoTracking()
       .Where(s => s.StudentId == userId && s.Score != null);

   if (await gradedQuery.AnyAsync())
   {
       var avg = await gradedQuery.AverageAsync(s => s.Score!.Value);
       vm.AverageScore = Math.Round(avg, 1);
   }
   else
   {
       vm.AverageScore = 0.0;
   }
   ```

---

## 4. Đặc tả Giao diện & Hệ Thống Token (CSS System)

Giao diện tuân thủ tuyệt đối **Tầng NHẸ (Light Tier)** theo `Docs/Design_Spec.md`:

### 4.1 Bố cục Client Dashboard (`Areas/Client/Views/Dashboard/Index.cshtml`)
- **Khối Banner (`.dash-banner`)**: Lời chào thân mật + Họ tên định dạng font viết tay FZ Caveat.
- **Nút CTA Duy Nhất**: 
  - Giáo viên: Nút đỏ phác thảo `+ Tạo khóa học mới` (`.btn.btn-cta`).
  - Học sinh: Nút viền mực `Khám phá khóa học` (`.btn.btn-cta-ghost`).
- **Hàng Thẻ Thống Kê (`.dash-stat-row`)**:
  - Giáo viên (5 thẻ): Khóa học, Học viên, Bài tập, Chờ chấm điểm, Buổi học sắp tới.
  - Học sinh (4 thẻ): Khóa đang học, Bài tập cần nộp, Bài đã chấm, Điểm trung bình.
- **Hai Cột Nội Dung**:
  - Cột trái (col-lg-8): Danh sách khóa học gần đây dạng lưới thẻ `.card-course` có ghim tròn đỏ (pin tack).
  - Cột phải (col-lg-4): Widget post-it `.todo-widget` chứa danh sách `.todo-item` với icon theo loại (`bi-calendar-event`, `bi-clock-history`, `bi-pencil-square`) và nhãn cảnh báo `.pill-overdue`.

### 4.2 Thẻ Cảnh Báo Cận Hạn (`.pill-overdue`)
Khác với các giao diện thông thường dùng chữ đỏ kích thước nhỏ vi phạm contrast law, Bulb.edu sử dụng nhãn viền mực kết hợp chấm tròn đỏ nổi bật:

```css
/* Trích xuất từ wwwroot/css/site.css */
.pill-overdue {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 1px 7px;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--cm-ink);
  background: var(--cm-surface);
  border: 1px solid var(--cm-ink);
  border-radius: 999px;
  line-height: 1.4;
}

.pill-overdue-dot {
  width: 7px;
  height: 7px;
  background-color: var(--cm-accent);
  border-radius: 50%;
  display: inline-block;
}
```

---

## 5. Kịch Bản & Kết Quả Kiểm Thử Toàn Diện (6 Bước P8)

Kịch bản kiểm thử tự động đã được thực thi thông qua bộ test `test_suite_p8.py` và Subagent kiểm thử trình duyệt thực tế, lưu ảnh tại [Docs/assets/](file:///C:/Users/E/source/web/WebHocTap(SaaS)/Docs/assets):

| Bước | Mục tiêu kiểm thử | Dữ liệu kiểm thử | Kết quả quan sát thực tế | Trạng thái |
|:---:|---|---|---|:---:|
| **1** | **Teacher Dashboard** | `teacher@class.com` | Thấy đầy đủ 5 thẻ số liệu, 1 session sắp tới, 1 bài tập chờ chấm; F5 liên tục không lỗi truy vấn. | ✅ ĐẠT |
| **2** | **Student Dashboard** | `student@class.com` | Thấy 4 thẻ số liệu học sinh, bài tập cận hạn kèm pill đỏ "Sắp hết hạn", không thấy tác vụ chấm bài. | ✅ ĐẠT |
| **3** | **Admin Dashboard & 403** | `admin@class.com` | Đếm chuẩn 12 người dùng, 5 khóa học, 3 bài nộp. Học sinh vào `/Admin/Dashboard` bị chặn 403. | ✅ ĐẠT |
| **4** | **Empty State User Mới** | `new_student@class.com` | Hiển thị khung `.empty-state` khám phá khóa học; AverageScore = `0.0`, không bị crash phép chia cho 0. | ✅ ĐẠT |
| **5** | **IDOR Isolation** | `teacher_b@class.com` | Thầy Bình chỉ thấy khóa *Giải tích 1* và buổi học của mình, tuyệt đối không thấy dữ liệu của Cô Minh Anh. | ✅ ĐẠT |
| **6** | **So sánh hạn chót UTC** | Bài tập hạn 12h vs 4 ngày | Bài tập hạn 12h kích hoạt pill đỏ `Sắp hết hạn`; bài tập hạn 4 ngày hiển thị nhãn bình thường. | ✅ ĐẠT |

---

## 6. Danh Mục Tệp Tin Triển Khai (Phần 7 + 8)

| Phân vùng | Tệp tin | Vai trò kỹ thuật |
|---|---|---|
| **Models & EF Core** | `Models/Course.cs` | Thêm thuộc tính `IsHidden` |
| | `Migrations/20261015152339_AddCourseIsHidden.cs` | Migration cập nhật schema bảng `Courses` |
| **ViewModels** | `Models/ViewModels/AdminUserViewModel.cs` | Chứa dữ liệu bảng quản lý tài khoản |
| | `Models/ViewModels/AdminCourseViewModel.cs` | Chứa dữ liệu bảng giám sát khóa học |
| | `Models/ViewModels/ProfileViewModel.cs` | Dữ liệu form cập nhật hồ sơ & đổi mật khẩu |
| | `Models/ViewModels/DashboardViewModels.cs` | Dữ liệu Client/Admin Dashboard & ToDoItem |
| **Controllers** | `Areas/Admin/Controllers/UserController.cs` | Xử lý khóa/mở khóa tài khoản & Lockout |
| | `Areas/Admin/Controllers/CourseController.cs` | Xử lý đóng/mở & ẩn/hiện khóa học |
| | `Areas/Admin/Controllers/DashboardController.cs` | Tính toán số liệu thống kê toàn hệ thống |
| | `Areas/Client/Controllers/ProfileController.cs` | Cập nhật thông tin & đổi mật khẩu người dùng |
| | `Areas/Client/Controllers/DashboardController.cs` | Thống kê 2 chiều & tổng hợp To-Do IDOR-locked |
| **Views** | `Areas/Admin/Views/User/Index.cshtml` | Giao diện quản lý tài khoản người dùng |
| | `Areas/Admin/Views/Course/Index.cshtml` | Giao diện giám sát khóa học & modal xác nhận |
| | `Areas/Admin/Views/Dashboard/Index.cshtml` | Giao diện tổng quan hệ thống Admin |
| | `Areas/Client/Views/Profile/Index.cshtml` | Giao diện hồ sơ cá nhân (2 cột responsive) |
| | `Areas/Client/Views/Dashboard/Index.cshtml` | Giao diện Dashboard Học sinh & Giáo viên |
| **Styles** | `wwwroot/css/site.css` | Bổ sung class `.card-stat`, `.todo-widget`, `.pill-overdue` |
| **Data Seed** | `Data/DbSeeder.cs` | Tự chữa lành tài khoản demo & dữ liệu mẫu P8 |

---

## 7. Ánh Xạ Báo Cáo Khóa Luận & Phụ Lục Z

Tài liệu này được biên soạn để trích dẫn trực tiếp vào các chương của báo cáo tốt nghiệp:

1. **Chương 3 (Thiết kế Nghiệp vụ & Giao diện Ứng dụng)**:
   - Mục 3.4: Module Quản trị viên (Khóa/Mở khóa tài khoản, Quản lý khóa học vi phạm).
   - Mục 3.5: Phân hệ Hồ sơ cá nhân và Cơ chế bảo mật xác thực danh tính.
   - Mục 3.6: Bảng điều khiển trung tâm (Dashboard) đa chiều và Widget việc cần làm liên khóa học.
   - Mục 3.2.a: Hồ sơ đối chiếu giữa 8 màn hình Mockup Figma và giao diện thực tế.
2. **Chương 4 (Giải pháp Kỹ thuật & Tối ưu Hiệu năng)**:
   - Chiến lược đếm dữ liệu `COUNT(*)` phía cơ sở dữ liệu thay vì load in-memory.
   - Kỹ thuật siết chặt IDOR cho các truy vấn tổng hợp đa khóa học.
   - Xử lý đồng bộ múi giờ UTC trong kiến trúc Cloud SaaS.
   - Thiết kế hệ thống nhãn cảnh báo tuân thủ quy chuẩn tương phản màu sắc (Contrast Law).
3. **Phụ lục Z (Tuyên bố Sử dụng AI)**:
   - Trích dẫn cam kết AUR.01–AUR.03: Sinh viên sử dụng AI hỗ trợ sinh mã giao diện Razor và tối ưu hóa biểu thức LINQ. Sinh viên trực tiếp thẩm định tính toàn vẹn dữ liệu (0 migration ở P8), siết chặt IDOR, kiểm soát rủi ro bảo mật và nghiệm thu toàn diện kịch bản kiểm thử.
