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
            // ✅ XÓA LUÔN 
            

            // Nếu đã đăng nhập rồi thì chuyển về trang chủ
            if (HttpContext.Session.GetString("TenDangNhap") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            

            return View();
        }

        // POST: Account/Login
        //   [HttpPost]
        //   [ValidateAntiForgeryToken]
        //   public async Task<IActionResult> Login(LoginViewModel model)
        //   {
        //       if (ModelState.IsValid)
        //       {
        //           // Tìm nhân viên với tên đăng nhập và mật khẩu
        //           var nhanVien = await _context.Nhanviens
        //               .FirstOrDefaultAsync(nv =>
        //                   nv.Tendangnhap == model.TenDangNhap &&
        //                   nv.Matkhau == model.MatKhau &&
        //                   nv.Nghiviec == false &&
        //                   nv.Hienthi == true);

        //           if (nhanVien != null)
        //           {
        //               // Kiểm tra quyền truy cập chức năng Quản trị
        //               if (model.ChucNang == "quantri" && nhanVien.Quyenadmin != true)
        //               {
        //                   ModelState.AddModelError("", "Bạn không có quyền truy cập chức năng Quản trị!");
        //                   return View(model);
        //               }

        //               // Lưu thông tin vào Session
        //               HttpContext.Session.SetString("IdNV", nhanVien.Idnv);
        //               HttpContext.Session.SetString("TenDangNhap", nhanVien.Tendangnhap!);
        //               HttpContext.Session.SetString("HoTenNV", nhanVien.Hotennv ?? "Nhân viên");
        //               HttpContext.Session.SetString("QuyenAdmin", nhanVien.Quyenadmin == true ? "1" : "0");
        //               HttpContext.Session.SetString("ChucNang", model.ChucNang);

        //// ✅ THAY ĐỔI: Đặt nội dung thông báo chào mừng chính xác
        //TempData["SuccessMessage"] = $"Chào mừng {nhanVien.Hotennv}! Đăng nhập thành công.";

        //// Chuyển hướng theo chức năng được chọn
        //if (model.ChucNang == "quantri")
        //               {
        //                   return RedirectToAction("Index", "Home");
        //               }
        //               else // banhang
        //               {
        //                   return RedirectToAction("Index", "ThuNgan");
        //               }
        //           }
        //           else
        //           {
        //               ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
        //           }
        //       }

        //       return View(model);
        //   }


        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm nhân viên theo tên đăng nhập (không quan tâm mật khẩu và trạng thái nghỉ việc lúc đầu)
                var nhanVien = await _context.Nhanviens
                    .FirstOrDefaultAsync(nv =>
                        nv.Tendangnhap == model.TenDangNhap &&
                        nv.Hienthi == true); // Chỉ cần Hienthi là true

                if (nhanVien != null)
                {
                    // 2. Kiểm tra mật khẩu
                    if (nhanVien.Matkhau != model.MatKhau)
                    {
                        ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                        return View(model);
                    }

                    // ⭐ 3. KIỂM TRA TRẠNG THÁI NGHỈ VIỆC
                    if (nhanVien.Nghiviec == true)
                    {
                        ModelState.AddModelError("", "Tài khoản này đã bị ngừng hoạt động (đã nghỉ việc)!");
                        return View(model);
                    }

                    // 4. Kiểm tra quyền truy cập chức năng Quản trị
                    if (model.ChucNang == "quantri" && nhanVien.Quyenadmin != true)
                    {
                        ModelState.AddModelError("", "Bạn không có quyền truy cập chức năng Quản trị!");
                        return View(model);
                    }

                    // 5. Lưu thông tin vào Session
                    HttpContext.Session.SetString("IdNV", nhanVien.Idnv);
                    HttpContext.Session.SetString("TenDangNhap", nhanVien.Tendangnhap!);
                    HttpContext.Session.SetString("HoTenNV", nhanVien.Hotennv ?? "Nhân viên");
                    HttpContext.Session.SetString("QuyenAdmin", nhanVien.Quyenadmin == true ? "1" : "0");
                    HttpContext.Session.SetString("ChucNang", model.ChucNang);

                    // ✅ THAY ĐỔI: Đặt nội dung thông báo chào mừng chính xác
                    TempData["SuccessMessage"] = $"Chào mừng {nhanVien.Hotennv}! Đăng nhập thành công.";

                    // 6. Chuyển hướng theo chức năng được chọn
                    if (model.ChucNang == "quantri")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else // banhang/ThuNgan
                    {
                        return RedirectToAction("Index", "ThuNgan");
                    }
                }
                else
                {
                    // Trường hợp không tìm thấy Tên đăng nhập
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

        
       
    }
}