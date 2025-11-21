using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace BTL_LTW_QLBIDA.Controllers
{
    [AdminAuthorize]
    public class KhachhangsController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;

        public KhachhangsController(QlquanBilliardLtw2Context context)
        {
            _context = context;
        }

        // GET: Khachhangs
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("TenDangNhap") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập!";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // API: Lấy danh sách khách hàng với filter
        // File: KhachHangController.cs

        // SỬA: Thêm tham số page (mặc định 1) và pageSize (mặc định 10)
        [HttpGet]
        public async Task<IActionResult> GetKhachhangs(string? searchString, string? sortBy, int page = 1, int pageSize = 10)
        {
            if (HttpContext.Session.GetString("TenDangNhap") == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            var khachhangs = _context.Khachhangs
                .Include(kh => kh.Hoadons)
                .AsQueryable();

            // 1. Tìm kiếm (Filter)
            if (!string.IsNullOrEmpty(searchString))
            {
                khachhangs = khachhangs.Where(kh =>
                    kh.Idkh.Contains(searchString) ||
                    kh.Hoten!.Contains(searchString) ||
                    kh.Sodt!.Contains(searchString));
            }

            // 2. Tải về Memory và Sắp xếp (Sort)
            var khachhangList = await khachhangs.ToListAsync();

            khachhangList = sortBy switch
            {
                "id_asc" => khachhangList.OrderBy(kh => int.Parse(kh.Idkh.Substring(4))).ToList(),
                "id_desc" => khachhangList.OrderByDescending(kh => int.Parse(kh.Idkh.Substring(4))).ToList(),
                "name_asc" => khachhangList.OrderBy(kh => kh.Hoten).ToList(),
                "name_desc" => khachhangList.OrderByDescending(kh => kh.Hoten).ToList(),
                _ => khachhangList.OrderBy(kh => int.Parse(kh.Idkh.Substring(4))).ToList() // Mặc định
            };

            // ⭐ 3. PHÂN TRANG (PAGINATION)
            var totalItems = khachhangList.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Đảm bảo trang hiện tại hợp lệ
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            // Lấy dữ liệu theo trang bằng Skip và Take
            var paginatedKhachhangs = khachhangList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4. Tạo kết quả trả về
            var items = paginatedKhachhangs
                .Select(kh => new
                {
                    idkh = kh.Idkh,
                    hoten = kh.Hoten,
                    sodt = kh.Sodt,
                    dchi = kh.Dchi,
                    tongHoaDon = kh.Hoadons.Count,
                    tongTien = kh.Hoadons.Sum(hd => hd.Tongtien) ?? 0
                })
                .ToList();

            // ⭐ Trả về dữ liệu cùng với thông tin phân trang
            return Json(new
            {
                success = true,
                data = new
                {
                    items = items, // Danh sách khách hàng trên trang hiện tại
                    totalItems = totalItems, // Tổng số khách hàng
                    currentPage = page,
                    totalPages = totalPages,
                    pageSize = pageSize
                }
            });
        }
        // GET: Khachhangs/Details/5
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

            var khachhang = await _context.Khachhangs
                .Include(kh => kh.Hoadons)
                .FirstOrDefaultAsync(m => m.Idkh == id);

            if (khachhang == null)
            {
                return NotFound();
            }

            return View(khachhang);
        }

        // GET: Khachhangs/Create
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            // Lấy tất cả mã KH và tìm số lớn nhất
            var allKhachhangs = await _context.Khachhangs
                .Where(kh => kh.Idkh.StartsWith("KH"))
                .Select(kh => kh.Idkh.Substring(2)) // Lấy phần số (bỏ "KH")
                .ToListAsync();

            int maxNumber = 0;

            // Tìm số lớn nhất trong database
            foreach (var numberStr in allKhachhangs)
            {
                if (int.TryParse(numberStr, out int number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            // Mã mới = số lớn nhất + 1
            int newNumber = maxNumber + 1;

            // Format: Luôn giữ KH00 + số tăng dần
            string newIdkh = $"KH00{newNumber}";

            var khachhangvm = new KhachHangVM { Idkh = newIdkh };
            return View(khachhangvm);
        }

        // POST: Khachhangs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHangVM khachhangvm)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra trùng mã
            if (await _context.Khachhangs.AnyAsync(kh => kh.Idkh == khachhangvm.Idkh))
            {
                ModelState.AddModelError("Idkh", "Mã khách hàng đã tồn tại!");
            }

            // Kiểm tra trùng SĐT
            if (!string.IsNullOrEmpty(khachhangvm.Sodt) &&
                await _context.Khachhangs.AnyAsync(kh => kh.Sodt == khachhangvm.Sodt))
            {
                ModelState.AddModelError("Sodt", "Số điện thoại đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                // ✅ ĐÚNG - Tạo Entity từ ViewModel
                var khachhang = new Khachhang
                {
                    Idkh = khachhangvm.Idkh,
                    Hoten = khachhangvm.Hoten,
                    Sodt = khachhangvm.Sodt,
                    Dchi = khachhangvm.Dchi
                };

                _context.Add(khachhang);  // ✅ Add Entity, không phải ViewModel
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(khachhangvm);
        }

        // GET: Khachhangs/Edit/5
        // GET: Khachhangs/Edit/5
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

            var khachhang = await _context.Khachhangs.FindAsync(id); // <-- Lấy Entity Model
            if (khachhang == null)
            {
                return NotFound();
            }

            // ⭐ SỬA: Chuyển sang ViewModel trước khi trả về View
            var khachhangvm = new KhachHangVM
            {
                Idkh = khachhang.Idkh,
                Hoten = khachhang.Hoten,
                Sodt = khachhang.Sodt,
                Dchi = khachhang.Dchi
            };

            return View(khachhangvm); // <-- Truyền ViewModel
        }

        // POST: Khachhangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, KhachHangVM khachhangvm)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != khachhangvm.Idkh)
            {
                return NotFound();
            }

            // Kiểm tra trùng SĐT (trừ chính nó)
            if (!string.IsNullOrEmpty(khachhangvm.Sodt) &&
                await _context.Khachhangs.AnyAsync(kh => kh.Sodt == khachhangvm.Sodt && kh.Idkh != id))
            {
                ModelState.AddModelError("Sodt", "Số điện thoại đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var khachhangCu = await _context.Khachhangs.FindAsync(id);
                    if (khachhangCu == null)
                    {
                        return NotFound();
                    }

                    khachhangCu.Hoten = khachhangvm.Hoten;
                    khachhangCu.Sodt = khachhangvm.Sodt;
                    khachhangCu.Dchi = khachhangvm.Dchi;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachhangExists(khachhangvm.Idkh))
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

            return View(khachhangvm);
        }

        // GET: Khachhangs/Delete/5
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

            var khachhang = await _context.Khachhangs
                .Include(kh => kh.Hoadons)
                .FirstOrDefaultAsync(m => m.Idkh == id);

            if (khachhang == null)
            {
                return NotFound();
            }

            // Kiểm tra có hóa đơn không
            if (khachhang.Hoadons.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng đã có hóa đơn!";
                return RedirectToAction("Details", new { id = id });
            }

            return View(khachhang);
        }

        // POST: Khachhangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            var khachhang = await _context.Khachhangs
                .Include(kh => kh.Hoadons)
                .FirstOrDefaultAsync(kh => kh.Idkh == id);

            if (khachhang != null)
            {
                if (khachhang.Hoadons.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa khách hàng đã có hóa đơn!";
                    return RedirectToAction("Details", new { id = id });
                }

                _context.Khachhangs.Remove(khachhang);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa khách hàng thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool KhachhangExists(string id)
        {
            return _context.Khachhangs.Any(e => e.Idkh == id);
        }
    }
}