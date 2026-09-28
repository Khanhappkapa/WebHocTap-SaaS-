# PHẦN 3: AUTHENTICATION, PHÂN QUYỀN & SEED DATA - Chi tiết chức năng

> **Dự án**: Website SaaS Quản Lý Lớp Học Trực Tuyến  
> **Ngày tạo**: 28/09/2026  
> **Phần trước**: Phần 2 (Areas + Layout + Responsive)

---

## 1. Sơ đồ luồng Register / Login

### 1.1 Luồng Register

```
User nhập form Register
       │
       ▼
┌─────────────────────────────────────────┐
│ model.UserRole == "Admin"?              │
│   → YES: ModelState.AddModelError       │──→ Trả về form + lỗi
│          (chặn cứng, chống leo thang)   │    "Không được phép đăng ký Admin"
│   → NO:  tiếp tục ↓                    │
└─────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ UserManager.CreateAsync(user, password) │
│ Tạo AppUser với:                        │
│   • UserName = Email                    │
│   • FullName = model.FullName           │
│   • UserRole = model.UserRole ← ★ Đồng bộ điểm 1: cột DB    │
│   • IsActive = true                     │
└─────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ ★ Đồng bộ điểm 2:                      │
│ AddToRoleAsync(user, model.UserRole)    │
│ → Ghi vào bảng AspNetUserRoles          │
│ → [Authorize(Roles="Teacher")] hoạt động│
└─────────────────────────────────────────┘
       │
       ▼
  SignInAsync → Redirect /Client/Dashboard
```

### 1.2 Luồng Login

```
User nhập Email + Password
       │
       ▼
┌─────────────────────────────────────────┐
│ FindByEmailAsync(email)                 │
│   → null? → Lỗi chung: "Email hoặc     │
│              mật khẩu không đúng"       │
│              (KHÔNG nói "email sai"     │
│              → tránh dò danh sách)      │
└─────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ user.IsActive == false?                 │
│   → "Tài khoản đã bị khóa"             │
│   (Admin set IsActive=false → chặn)     │
└─────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ PasswordSignInAsync(user, password,     │
│   rememberMe, lockoutOnFailure: false)  │
│   → Sai password → "Email hoặc mật     │
│     khẩu không đúng" (lỗi chung)       │
└─────────────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ Redirect theo role:                     │
│   • UserRole == "Admin"                 │
│     → /Admin/Dashboard                  │
│   • Còn lại (Teacher/Student)           │
│     → /Client/Dashboard                 │
└─────────────────────────────────────────┘
```

---

## 2. Base Controller Pattern

### Cách hoạt động

```csharp
// Gắn 1 chỗ
[Area("Admin")]
[Authorize(Roles = "Admin")]
public abstract class BaseAdminController : Controller { }

// Mọi controller kế thừa → tự động có [Area] + [Authorize]
public class DashboardController : BaseAdminController { ... }
public class UserController : BaseAdminController { ... }
public class CourseController : BaseAdminController { ... }
```

### So sánh 2 cách tiếp cận

| Tiêu chí | Gắn rải rác từng controller | Base Controller Pattern |
|----------|---------------------------|----------------------|
| **Số lần viết** | Mỗi controller 2 dòng attribute | 1 lần duy nhất trong base |
| **Rủi ro quên** | CAO — thêm controller mới dễ quên [Authorize] → lỗ hổng bảo mật | THẤP — kế thừa là tự có |
| **Thay đổi role** | Sửa N files | Sửa 1 file base |
| **Code review** | Khó kiểm tra đầy đủ | Chỉ cần kiểm tra 1 file |
| **Nhược điểm** | — | Controller muốn public (không cần auth) phải dùng [AllowAnonymous] |

---

## 3. Bảng cấu hình Cookie Authentication

| Thuộc tính | Giá trị | Ý nghĩa |
|-----------|---------|---------|
| `LoginPath` | `/Account/Login` | Khi chưa đăng nhập mà truy cập [Authorize] → redirect tới đây |
| `AccessDeniedPath` | `/Account/AccessDenied` | Khi đã đăng nhập nhưng không đủ quyền → redirect tới đây |
| `ExpireTimeSpan` | 4 giờ | Cookie hết hạn sau 4 giờ kể từ lần tạo/gia hạn cuối |
| `SlidingExpiration` | `true` | Tự động gia hạn cookie nếu user hoạt động liên tục (quá nửa thời gian) |

**Cách SlidingExpiration hoạt động:**
- Cookie tạo lúc 8:00, hết hạn 12:00
- User hoạt động lúc 10:01 (quá nửa = 2h) → cookie được gia hạn thêm 4h → hết hạn 14:01
- User KHÔNG hoạt động sau 10:01 → cookie hết hạn lúc 14:01 → phải login lại

---

## 4. Trade-off Password Policy

| Thuộc tính | Giá trị | Lý do |
|-----------|---------|-------|
| `RequiredLength` | 6 | **GIỮ CHẶT** — tối thiểu 6 ký tự, cân bằng bảo mật + thuận tiện |
| `RequireDigit` | `true` | **GIỮ CHẶT** — bắt buộc có chữ số, chống password quá đơn giản |
| `RequireLowercase` | `true` | **GIỮ CHẶT** — bắt buộc chữ thường, để đảm bảo "123456" bị từ chối do thiếu chữ cái |
| `RequireUppercase` | `false` | **NỚI** — không bắt buộc chữ hoa |
| `RequireNonAlphanumeric` | `false` | **NỚI** — không bắt buộc ký tự đặc biệt (@, #, !) |

**Đây là quyết định có chủ đích:**
- Mục tiêu: đồ án demo, cần balance giữa bảo mật vừa đủ và trải nghiệm người dùng
- Giữ: độ dài (6), chữ số, chữ thường → đảm bảo password ngây ngô "123456" bị từ chối. Mật khẩu như "abc123", "Admin@123" được chấp nhận.
- Nới: uppercase/special char → người dùng demo không bị annoyed vì password bị reject liên tục.
- **Trong production**: nên bật đầy đủ + RequiredLength ≥ 8

---

## 5. Tính Idempotent & Tự chữa lành của DbSeeder

**Idempotent** = gọi nhiều lần, kết quả vẫn giống hệt như gọi 1 lần:

```csharp
// Lần 1: chưa có role "Admin" → TẠO MỚI
// Lần 2: đã có role "Admin" → BỎ QUA (RoleExistsAsync == true)
if (!await roleManager.RoleExistsAsync(role))
    await roleManager.CreateAsync(new IdentityRole(role));

// Lần 1: chưa có user admin@class.com → TẠO MỚI
// Lần 2: đã có user → BỎ QUA TẠO
if (user == null) {
    // Tạo user
}
// -----------------------------
// Tự chữa lành (Self-healing): luôn kiểm tra role dù user cũ hay mới
if (!await userManager.IsInRoleAsync(user, role)) {
    await userManager.AddToRoleAsync(user, role);
}
```

| Lần chạy | Roles | Users | Kết quả |
|----------|-------|-------|---------|
| Lần 1 (DB trống) | Tạo Admin, Teacher, Student | Tạo 3 user demo | ✅ |
| Lần 2 (đã có) | Bỏ qua (đã tồn tại) | Bỏ qua (đã tồn tại) | ✅ Không crash |
| Lần N | Bỏ qua | Bỏ qua | ✅ Không crash |

**Tại sao quan trọng?** → Program.cs gọi `DbSeeder.SeedAsync()` mỗi lần app khởi động. Nếu seed KHÔNG idempotent → restart app = crash vì duplicate key.

---

## 6. Ba điểm đồng bộ UserRole ↔ AspNetUserRoles

Hệ thống có **2 nguồn sự thật** về vai trò:

| Nguồn | Vị trí | Ai đọc |
|-------|--------|--------|
| Cột `UserRole` (string) | Bảng `AspNetUsers` | Code query / hiển thị nhanh (VD: `user.UserRole`) |
| Bảng `AspNetUserRoles` | Bảng riêng của Identity | `[Authorize(Roles="...")]` |

**3 điểm đồng bộ bắt buộc:**

| # | Vị trí | Hành động | Nếu thiếu |
|---|--------|-----------|-----------|
| 1 | `AccountController.Register()` | `user.UserRole = model.UserRole` + `AddToRoleAsync(user, model.UserRole)` | User đăng ký xong → vào /Client/Dashboard bị AccessDenied |
| 2 | `DbSeeder.SeedUserAsync()` | `user.UserRole = role` + `AddToRoleAsync(user, role)` | Seed user không thể đăng nhập vào area đúng |
| 3 | **(Tương lai)** Admin đổi role | Phải cập nhật CẢ cột `UserRole` VÀ gọi `RemoveFromRoleAsync` + `AddToRoleAsync` | Hiển thị khác với phân quyền thực |

---

## 7. Danh sách file đã tạo/sửa

| STT | File | Thao tác | Mô tả |
|-----|------|----------|-------|
| 1 | `Models/ViewModels/RegisterViewModel.cs` | Mới | FullName, Email, Password, ConfirmPassword, UserRole |
| 2 | `Models/ViewModels/LoginViewModel.cs` | Mới | Email, Password, RememberMe |
| 3 | `Controllers/AccountController.cs` | Mới | Register, Login, Logout (POST), AccessDenied |
| 4 | `Data/DbSeeder.cs` | Mới | Seed idempotent: 3 roles + 3 users |
| 5 | `Areas/Admin/Controllers/BaseAdminController.cs` | Mới | [Area("Admin")] [Authorize(Roles="Admin")] |
| 6 | `Areas/Client/Controllers/BaseClientController.cs` | Mới | [Area("Client")] [Authorize(Roles="Teacher,Student")] |
| 7 | `Areas/Admin/Controllers/DashboardController.cs` | Sửa | Kế thừa BaseAdminController |
| 8 | `Areas/Client/Controllers/DashboardController.cs` | Sửa | Kế thừa BaseClientController |
| 9 | `Program.cs` | Sửa | Password policy, cookie 4h+sliding, seed call |
| 10 | `Views/Shared/_Layout.cshtml` | Sửa | Inject SignInManager, hiển thị login/logout có điều kiện |
| 11 | `Areas/Admin/Views/Shared/_AdminLayout.cshtml` | Sửa | Inject UserManager, FullName + badge Admin + form logout |
| 12 | `Areas/Client/Views/Shared/_ClientLayout.cshtml` | Sửa | Inject UserManager, FullName + badge role + form logout |
| 13 | `Views/Account/Register.cshtml` | Mới | Form đăng ký, dropdown chỉ Teacher/Student |
| 14 | `Views/Account/Login.cshtml` | Mới | Form đăng nhập |
| 15 | `Views/Account/AccessDenied.cshtml` | Mới | Trang thông báo không có quyền |

---

## 8. Tài khoản demo đã seed

| Email | Mật khẩu | Role | Area |
|-------|----------|------|------|
| `admin@class.com` | `Admin@123` | Admin | /Admin/Dashboard |
| `teacher@class.com` | `Teacher@123` | Teacher | /Client/Dashboard |
| `student@class.com` | `Student@123` | Student | /Client/Dashboard |

---

## 9. Tăng cường Bảo mật Thông tin (Security Patch 0.3.5)

Vào phiên bản 0.3.5, dự án đã vá 2 lỗ hổng và áp dụng 3 cơ chế nâng cao:

| Cơ chế | Áp dụng | Tác dụng |
|--------|---------|----------|
| **Global Anti-Forgery Token** | `options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())` | Thay vì chỉ có thẻ `<input name="__RequestVerificationToken">` ngầm ẩn trong HTML, MVC giờ đây **chủ động kiểm tra** token này trên mọi request POST/PUT/DELETE trên toàn cục ứng dụng. Lớp bảo vệ CSRF được hoàn thiện 100%. (Lưu ý: Mọi AJAX fetch gọi POST từ nay phải kèm token trong header). |
| **Lockout Protection** | `lockoutOnFailure: true` (AccountController) + Cấu hình trong `Program.cs` | Chống Brute-force: Nếu nhập sai mật khẩu 5 lần, tài khoản tự động khóa 5 phút. |
| **Self-healing Seed Data** | Tách user creation và role assignment trong `DbSeeder` | Nếu user đã tồn tại nhưng mất role trong bảng AspNetUserRoles, tiến trình Seed sẽ tự động dò và cấp lại role, đảm bảo khả năng **tự chữa lành** dữ liệu phân quyền. |
