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
        [HttpGet]
        public async Task<IActionResult> GetKhachhangs(string? searchString, string? sortBy)
        {
            if (HttpContext.Session.GetString("TenDangNhap") == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            var khachhangs = _context.Khachhangs
                .Include(kh => kh.Hoadons)   // FIX 1: Include để tránh null
                .AsQueryable();
            //var khachhangs = _context.Khachhangs.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                khachhangs = khachhangs.Where(kh =>
                    kh.Idkh.Contains(searchString) ||
                    kh.Hoten!.Contains(searchString) ||
                    kh.Sodt!.Contains(searchString));
            }

            // ✅ TẢI VỀ MEMORY TRƯỚC KHI SẮP XẾP
            var khachhangList = await khachhangs.ToListAsync();

            // ✅ SẮP XẾP THEO YÊU CẦU
            khachhangList = sortBy switch
            {
                "id_asc" => khachhangList.OrderBy(kh => int.Parse(kh.Idkh.Substring(4))).ToList(),
                "id_desc" => khachhangList.OrderByDescending(kh => int.Parse(kh.Idkh.Substring(4))).ToList(),
                "name_asc" => khachhangList.OrderBy(kh => kh.Hoten).ToList(),
                "name_desc" => khachhangList.OrderByDescending(kh => kh.Hoten).ToList(),
                _ => khachhangList.OrderBy(kh => int.Parse(kh.Idkh.Substring(4))).ToList() // ✅ Mặc định: theo mã KH tăng dần
            };

            var result = khachhangList
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

            return Json(new { success = true, data = result });
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
                _context.Add(khachhangvm);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(khachhangvm);
        }

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

            var khachhang = await _context.Khachhangs.FindAsync(id);
            if (khachhang == null)
            {
                return NotFound();
            }

            return View(khachhang);
        }

        // POST: Khachhangs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Khachhang khachhang)
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != khachhang.Idkh)
            {
                return NotFound();
            }

            // Kiểm tra trùng SĐT (trừ chính nó)
            if (!string.IsNullOrEmpty(khachhang.Sodt) &&
                await _context.Khachhangs.AnyAsync(kh => kh.Sodt == khachhang.Sodt && kh.Idkh != id))
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

                    khachhangCu.Hoten = khachhang.Hoten;
                    khachhangCu.Sodt = khachhang.Sodt;
                    khachhangCu.Dchi = khachhang.Dchi;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KhachhangExists(khachhang.Idkh))
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

            return View(khachhang);
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