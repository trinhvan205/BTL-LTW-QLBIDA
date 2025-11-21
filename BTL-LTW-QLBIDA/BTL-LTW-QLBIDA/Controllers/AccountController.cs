using System;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;

namespace BTL_LTW_QLBIDA.Controllers
{
    public class AccountController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;

        public AccountController(QlquanBilliardLtw2Context context)
        {
            _context = context;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            
            // Mỗi lần vào màn hình Login thì logout session
            HttpContext.Session.Clear();
            // ✅ XÓA LUÔN TEMPDATA
            TempData.Clear();

            // Nếu đã đăng nhập rồi thì chuyển về trang chủ
            if (HttpContext.Session.GetString("TenDangNhap") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm nhân viên với tên đăng nhập và mật khẩu
                var nhanVien = await _context.Nhanviens
                    .FirstOrDefaultAsync(nv =>
                        nv.Tendangnhap == model.TenDangNhap &&
                        nv.Matkhau == model.MatKhau &&
                        nv.Nghiviec == false &&
                        nv.Hienthi == true);

                if (nhanVien != null)
                {
                    // Kiểm tra quyền truy cập chức năng Quản trị
                    if (model.ChucNang == "quantri" && nhanVien.Quyenadmin != true)
                    {
                        ModelState.AddModelError("", "Bạn không có quyền truy cập chức năng Quản trị!");
                        return View(model);
                    }

                    // Lưu thông tin vào Session
                    HttpContext.Session.SetString("IdNV", nhanVien.Idnv);
                    HttpContext.Session.SetString("TenDangNhap", nhanVien.Tendangnhap!);
                    HttpContext.Session.SetString("HoTenNV", nhanVien.Hotennv ?? "Nhân viên");
                    HttpContext.Session.SetString("QuyenAdmin", nhanVien.Quyenadmin == true ? "1" : "0");
                    HttpContext.Session.SetString("ChucNang", model.ChucNang);

                    TempData["SuccessMessage"] = "Đăng nhập thành công!";

                    // Chuyển hướng theo chức năng được chọn
                    if (model.ChucNang == "quantri")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else // banhang
                    {
                        return RedirectToAction("Index", "ThuNgan");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                }
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        // GET: Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("TenDangNhap") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            if (HttpContext.Session.GetString("TenDangNhap") == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(matKhauCu) || string.IsNullOrEmpty(matKhauMoi))
            {
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin!");
                return View();
            }

            if (matKhauMoi != xacNhanMatKhau)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận không khớp!");
                return View();
            }

            var idNV = HttpContext.Session.GetString("IdNV");
            var nhanVien = await _context.Nhanviens.FindAsync(idNV);

            if (nhanVien == null)
            {
                return RedirectToAction("Login");
            }

            if (nhanVien.Matkhau != matKhauCu)
            {
                ModelState.AddModelError("", "Mật khẩu cũ không đúng!");
                return View();
            }

            // Cập nhật mật khẩu mới
            nhanVien.Matkhau = matKhauMoi;
            _context.Update(nhanVien);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Home");
        }
    }
}