using System.Globalization;
using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Controllers
{
    [AdminAuthorize]
    public class NhanviensController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;

        public NhanviensController(QlquanBilliardLtw2Context context)
        {
            _context = context;
        }

        // GET: Nhanviens
        // GET: Nhanviens
        public async Task<IActionResult> Index(string searchString, string trangThai, string sortBy)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("TenDangNhap") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra quyền Admin
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            // Truy vấn danh sách nhân viên
            var nhanviens = from nv in _context.Nhanviens
                            select nv;

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                if (trangThai == "danglam")
                {
                    nhanviens = nhanviens.Where(nv => nv.Nghiviec == false);
                }
                else if (trangThai == "danghhi")
                {
                    nhanviens = nhanviens.Where(nv => nv.Nghiviec == true);
                }
            }
            else
            {
                // Mặc định chỉ hiển thị nhân viên đang làm việc
                nhanviens = nhanviens.Where(nv => nv.Nghiviec == false);
            }

            // Tìm kiếm theo tên, CCCD, SĐT
            if (!string.IsNullOrEmpty(searchString))
            {
                nhanviens = nhanviens.Where(nv =>
                    nv.Hotennv!.Contains(searchString) ||
                    nv.Cccd!.Contains(searchString) ||
                    nv.Sodt!.Contains(searchString) ||
                    nv.Tendangnhap!.Contains(searchString));
            }

            //// ✅ SẮP XẾP
            //nhanviens = sortBy switch
            //{
            //    "id_desc" => nhanviens.OrderByDescending(nv => nv.Idnv),
            //    "name_asc" => nhanviens.OrderBy(nv => nv.Hotennv),
            //    "name_desc" => nhanviens.OrderByDescending(nv => nv.Hotennv),
            //    _ => nhanviens.OrderBy(nv => nv.Idnv) // Mặc định: sắp xếp theo mã tăng dần
            //};

            // ✅ SẮP XẾP - Tải về bộ nhớ trước khi parse
            var nhanvienList = await nhanviens.ToListAsync(); // Tải về memory trước

            nhanvienList = sortBy switch
            {
                "id_desc" => nhanvienList.OrderByDescending(nv =>
                    int.Parse(nv.Idnv.Substring(4))).ToList(),
                "name_asc" => nhanvienList.OrderBy(nv => nv.Hotennv).ToList(),
                "name_desc" => nhanvienList.OrderByDescending(nv => nv.Hotennv).ToList(),
                _ => nhanvienList.OrderBy(nv =>
                    int.Parse(nv.Idnv.Substring(4))).ToList() // Sắp xếp theo số
            };

            ViewBag.SearchString = searchString;
            ViewBag.TrangThai = trangThai;
            ViewBag.SortBy = sortBy;
            ViewBag.TongNhanVien = nhanvienList.Count;

            return View(nhanvienList);
        }

        // API: Lấy danh sách nhân viên với filter, Sắp xếp và Phân trang (dùng cho AJAX)
        [HttpGet]
        public async Task<IActionResult> GetNhanviens(
            string? searchString,
            string? trangThai,
            string? sortBy, // Thêm SortBy nếu cần dùng
            int page = 1, // ⭐ THAM SỐ PHÂN TRANG MỚI
            int pageSize = 10) // ⭐ THAM SỐ PHÂN TRANG MỚI
        {
            if (HttpContext.Session.GetString("TenDangNhap") == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            var nhanviens = _context.Nhanviens.AsQueryable();

            // 1. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(trangThai))
            {
                if (trangThai == "danglam")
                {
                    nhanviens = nhanviens.Where(nv => nv.Nghiviec == false);
                }
                else if (trangThai == "danghhi")
                {
                    nhanviens = nhanviens.Where(nv => nv.Nghiviec == true);
                }
            }
            else
            {
                // Mặc định: chỉ hiển thị nhân viên đang làm
                nhanviens = nhanviens.Where(nv => nv.Nghiviec == false);
            }

            // 2. Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                nhanviens = nhanviens.Where(nv =>
                    nv.Idnv.Contains(searchString) ||
                    nv.Hotennv!.Contains(searchString) ||
                    nv.Cccd!.Contains(searchString) ||
                    nv.Sodt!.Contains(searchString) ||
                    nv.Tendangnhap!.Contains(searchString));
            }

            // Chỉ hiển thị nhân viên có HIENTHI = true
            nhanviens = nhanviens.Where(nv => nv.Hienthi == true);

            // Tải về memory trước khi sắp xếp (Bắt buộc vì sắp xếp dùng int.Parse trên chuỗi ID)
            var nhanvienList = await nhanviens.ToListAsync();

            // 3. Sắp xếp (Theo số trong IDNV tăng dần, giống logic cũ)
            nhanvienList = nhanvienList
                .OrderBy(nv => int.Parse(nv.Idnv.Substring(4)))
                .ToList();

            // ⭐ 4. PHÂN TRANG (PAGINATION)
            var totalItems = nhanvienList.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Đảm bảo trang hiện tại hợp lệ
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Lấy dữ liệu theo trang bằng Skip và Take
            var paginatedNhanviens = nhanvienList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5. Tạo kết quả trả về
            var items = paginatedNhanviens
                .Select(nv => new
                {
                    idnv = nv.Idnv,
                    hotennv = nv.Hotennv,
                    gioitinh = nv.Gioitinh,
                    sodt = nv.Sodt,
                    cccd = nv.Cccd,
                    tendangnhap = nv.Tendangnhap,
                    quyenadmin = nv.Quyenadmin,
                    nghiviec = nv.Nghiviec
                })
                .ToList();

            // ⭐ Trả về dữ liệu cùng với thông tin phân trang
            return Json(new
            {
                success = true,
                data = new
                {
                    items = items, // Danh sách nhân viên trên trang hiện tại
                    totalItems = totalItems, // Tổng số nhân viên
                    currentPage = page,
                    totalPages = totalPages,
                    pageSize = pageSize
                }
            });
        }

        // GET: Nhanviens/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var nhanvien = await _context.Nhanviens
                .FirstOrDefaultAsync(m => m.Idnv == id);

            if (nhanvien == null)
            {
                return NotFound();
            }

            return View(nhanvien);
        }


        // GET: Nhanviens/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy tất cả mã NV và tìm số lớn nhất
            var allNhanviens = await _context.Nhanviens
                .Where(nv => nv.Idnv.StartsWith("NV"))
                .Select(nv => nv.Idnv.Substring(2)) // Lấy phần sau "NV"
                .ToListAsync();

            int maxNumber = 0;

            // Tìm số lớn nhất trong database
            foreach (var numberStr in allNhanviens)
            {
                if (int.TryParse(numberStr, out int number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            // Mã mới = số lớn nhất + 1
            int newNumber = maxNumber + 1;

            // Format: Luôn giữ NV00 + số tăng dần
            string newIdnv = $"NV00{newNumber}";

            var nhanvienvm = new NhanVienVM { Idnv = newIdnv };
            return View(nhanvienvm);
        }

        //// POST: Nhanviens/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Idnv,Hotennv,Ngaysinh,Gioitinh,Cccd,Sodt,Tendangnhap,Matkhau,Quyenadmin")] NhanVienVM nhanvienvm)
        //{
        //    if (HttpContext.Session.GetString("QuyenAdmin") != "1")
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }

        //    // Kiểm tra trùng IDNV
        //    if (await _context.Nhanviens.AnyAsync(nv => nv.Idnv == nhanvienvm.Idnv))
        //    {
        //        ModelState.AddModelError("Idnv", "Mã nhân viên đã tồn tại!");
        //    }

        //    // Kiểm tra trùng CCCD
        //    if (!string.IsNullOrEmpty(nhanvienvm.Cccd) &&
        //        await _context.Nhanviens.AnyAsync(nv => nv.Cccd == nhanvienvm.Cccd))
        //    {
        //        ModelState.AddModelError("Cccd", "CCCD đã tồn tại!");
        //    }

        //    // Kiểm tra trùng Tên đăng nhập
        //    if (!string.IsNullOrEmpty(nhanvienvm.Tendangnhap) &&
        //        await _context.Nhanviens.AnyAsync(nv => nv.Tendangnhap == nhanvienvm.Tendangnhap))
        //    {
        //        ModelState.AddModelError("Tendangnhap", "Tên đăng nhập đã tồn tại!");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        nhanvienvm.Hienthi = true;
        //        nhanvienvm.Nghiviec = false;

        //        // Nếu không nhập mật khẩu, để NULL
        //        if (string.IsNullOrEmpty(nhanvienvm.Matkhau))
        //        {
        //            nhanvienvm.Matkhau = null;
        //        }

        //        _context.Add(nhanvienvm);
        //        await _context.SaveChangesAsync();

        //        ["SuccessMessage"] = "Thêm nhân viên thành công!";
        //        return RedirectToAction(nameof(Index));
        //    }

        //    return View(nhanvienvm);
        //}

        // POST: Nhanviens/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVienVM viewModel)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra trùng IDNV
            if (await _context.Nhanviens.AnyAsync(nv => nv.Idnv == viewModel.Idnv))
            {
                ModelState.AddModelError("Idnv", "Mã nhân viên đã tồn tại!");
            }

            // Kiểm tra trùng CCCD
            if (!string.IsNullOrEmpty(viewModel.Cccd) &&
                await _context.Nhanviens.AnyAsync(nv => nv.Cccd == viewModel.Cccd))
            {
                ModelState.AddModelError("Cccd", "CCCD đã tồn tại!");
            }

            // Kiểm tra trùng Tên đăng nhập
            if (!string.IsNullOrEmpty(viewModel.Tendangnhap) &&
                await _context.Nhanviens.AnyAsync(nv => nv.Tendangnhap == viewModel.Tendangnhap))
            {
                ModelState.AddModelError("Tendangnhap", "Tên đăng nhập đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                // ✅ CHUYỂN TỪ VIEWMODEL SANG MODEL
                var nhanvien = new Nhanvien
                {
                    Idnv = viewModel.Idnv,
                    Hotennv = viewModel.Hotennv,
                    Ngaysinh = viewModel.Ngaysinh,
                    Gioitinh = viewModel.Gioitinh ?? false, // Nếu null thì mặc định false (Nam)
                    Cccd = viewModel.Cccd,
                    Sodt = viewModel.Sodt,
                    Tendangnhap = viewModel.Tendangnhap,
                    Matkhau = string.IsNullOrEmpty(viewModel.Matkhau) ? null : viewModel.Matkhau,
                    Quyenadmin = viewModel.Quyenadmin ?? false,
                    Hienthi = true,
                    Nghiviec = false
                };

                _context.Add(nhanvien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Nhanviens/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var nhanvien = await _context.Nhanviens.FindAsync(id);
            if (nhanvien == null)
            {
                return NotFound();
            }

            return View(nhanvien);
        }

        // POST: Nhanviens/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Nhanvien nhanvien)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != nhanvien.Idnv)
            {
                return NotFound();
            }

            // Kiểm tra trùng CCCD (trừ chính nó)
            if (!string.IsNullOrEmpty(nhanvien.Cccd) &&
                await _context.Nhanviens.AnyAsync(nv => nv.Cccd == nhanvien.Cccd && nv.Idnv != id))
            {
                ModelState.AddModelError("Cccd", "CCCD đã tồn tại!");
            }

            // Kiểm tra trùng Tên đăng nhập (trừ chính nó)
            if (!string.IsNullOrEmpty(nhanvien.Tendangnhap) &&
                await _context.Nhanviens.AnyAsync(nv => nv.Tendangnhap == nhanvien.Tendangnhap && nv.Idnv != id))
            {
                ModelState.AddModelError("Tendangnhap", "Tên đăng nhập đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // LẤY THÔNG TIN NHÂN VIÊN CŨ TỪ DATABASE
                    var nhanvienCu = await _context.Nhanviens.FindAsync(id);

                    if (nhanvienCu == null)
                    {
                        return NotFound();
                    }

                    // CẬP NHẬT CÁC TRƯỜNG
                    nhanvienCu.Hotennv = nhanvien.Hotennv;
                    nhanvienCu.Ngaysinh = nhanvien.Ngaysinh;
                    nhanvienCu.Gioitinh = nhanvien.Gioitinh;
                    nhanvienCu.Cccd = nhanvien.Cccd;
                    nhanvienCu.Sodt = nhanvien.Sodt;
                    nhanvienCu.Tendangnhap = nhanvien.Tendangnhap;
                    nhanvienCu.Quyenadmin = nhanvien.Quyenadmin;
                    nhanvienCu.Hienthi = nhanvien.Hienthi;
                    nhanvienCu.Nghiviec = nhanvien.Nghiviec;

                    // NẾU KHÔNG NHẬP MẬT KHẨU MỚI, GIỮ NGUYÊN MẬT KHẨU CŨ
                    if (!string.IsNullOrWhiteSpace(nhanvien.Matkhau))
                    {
                        nhanvienCu.Matkhau = nhanvien.Matkhau;
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanvienExists(nhanvien.Idnv))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(nhanvien);
        }

        // GET: Nhanviens/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var nhanvien = await _context.Nhanviens
                .FirstOrDefaultAsync(m => m.Idnv == id);

            if (nhanvien == null)
            {
                return NotFound();
            }

            // KIỂM TRA: Không cho xóa nếu nhân viên có tài khoản
            if (!string.IsNullOrEmpty(nhanvien.Tendangnhap) || !string.IsNullOrEmpty(nhanvien.Matkhau))
            {
                TempData["ErrorMessage"] = "Không thể xóa nhân viên đã có tài khoản! Vui lòng chuyển sang trạng thái 'Nghỉ việc' thay vì xóa.";
                return RedirectToAction("Details", new { id = id });
            }

            return View(nhanvien);
        }

        // POST: Nhanviens/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            var nhanvien = await _context.Nhanviens.FindAsync(id);
            if (nhanvien != null)
            {
                // KIỂM TRA LẠI: Không cho xóa nếu nhân viên có tài khoản
                if (!string.IsNullOrEmpty(nhanvien.Tendangnhap) || !string.IsNullOrEmpty(nhanvien.Matkhau))
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhân viên đã có tài khoản! Vui lòng chuyển sang trạng thái 'Nghỉ việc' thay vì xóa.";
                    return RedirectToAction("Details", new { id = id });
                }

                // ✅ XÓA CỨNG - Xóa hẳn khỏi database
                _context.Nhanviens.Remove(nhanvien);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Đổi trạng thái nghỉ việc
        [HttpPost]
        public async Task<IActionResult> ToggleNghiViec(string id)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return Json(new { success = false, message = "Không có quyền!" });
            }

            var nhanvien = await _context.Nhanviens.FindAsync(id);
            if (nhanvien == null)
            {
                return Json(new { success = false, message = "Không tìm thấy nhân viên!" });
            }

            nhanvien.Nghiviec = !nhanvien.Nghiviec;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = nhanvien.Nghiviec ? "Đã chuyển sang nghỉ việc" : "Đã chuyển sang đang làm việc",
                nghiviec = nhanvien.Nghiviec
            });
        }

        private bool NhanvienExists(string id)
        {
            return _context.Nhanviens.Any(e => e.Idnv == id);
        }
    }
}