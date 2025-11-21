using BTL_LTW_QLBIDA.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using BTL_LTW_QLBIDA.Services;
using QuestPDF.Infrastructure; // ← THÊM

// ← THÊM: Community License
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<PdfService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
// Sửa <> thành <QlquanBilliardLtwContext>
builder.Services.AddDbContext<QlquanBilliardLtw2Context>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== THÊM SESSION =====
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ← THÊM: Đăng ký PdfService
builder.Services.AddScoped<PdfService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ===== THÊM SESSION MIDDLEWARE =====
app.UseSession();

app.UseAuthorization();

// ===== ĐỔI DEFAULT CONTROLLER THÀNH ACCOUNT =====
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
