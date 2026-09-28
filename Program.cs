// ============================================================
// File: Program.cs
// Mô tả: Cấu hình ứng dụng ASP.NET Core MVC
// CHECKPOINT: Đăng ký DbContext (PostgreSQL) + Identity (AppUser, IdentityRole)
// ============================================================

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebHocTap_SaaS_.Data;
using WebHocTap_SaaS_.Models;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// CHECKPOINT: Đăng ký DbContext với PostgreSQL (Supabase)
// Sử dụng Npgsql làm provider cho Entity Framework Core
// Connection string lấy từ appsettings.json → ConnectionStrings:DefaultConnection
// --------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------------------------------
// CHECKPOINT: Đăng ký ASP.NET Core Identity
// Sử dụng AddIdentity<AppUser, IdentityRole> thay vì AddDefaultIdentity<IdentityUser>
// ĐÂY LÀ BẮT BUỘC: Nếu dùng IdentityUser mặc định sẽ bị lỗi cast type khi runtime
// --------------------------------------------------
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu (có thể tùy chỉnh theo yêu cầu)
    options.Password.RequireDigit = true;             // Yêu cầu có chữ số
    options.Password.RequireLowercase = true;          // Yêu cầu có chữ thường
    options.Password.RequireUppercase = false;         // Không bắt buộc chữ hoa
    options.Password.RequireNonAlphanumeric = false;   // Không bắt buộc ký tự đặc biệt
    options.Password.RequiredLength = 6;               // Độ dài tối thiểu 6 ký tự

    // Cấu hình đăng nhập
    options.SignIn.RequireConfirmedAccount = false;     // Không yêu cầu xác nhận email
})
.AddEntityFrameworkStores<ApplicationDbContext>()      // Sử dụng ApplicationDbContext
.AddDefaultTokenProviders();                           // Token cho reset password, email confirm...

// Add services to the container
builder.Services.AddControllersWithViews();

// --------------------------------------------------
// CHECKPOINT: Cấu hình đường dẫn đăng nhập/đăng xuất cho Identity
// Khi chưa đăng nhập mà truy cập trang [Authorize] sẽ redirect tới đây
// --------------------------------------------------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --------------------------------------------------
// CHECKPOINT: Middleware Authentication PHẢI đặt TRƯỚC Authorization
// UseAuthentication() xác thực người dùng (ai đang đăng nhập?)
// UseAuthorization() phân quyền (người đó có quyền truy cập không?)
// --------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();

// --------------------------------------------------
// CHECKPOINT: Area route PHẢI đặt TRƯỚC default route
// Nếu đặt sau, URL /Admin/Dashboard sẽ bị default route nuốt
// (hiểu "Admin" là tên controller) → 404
// --------------------------------------------------
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
