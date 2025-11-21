using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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

        // 1. VIEW CHÍNH (CONTAINER)
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập!";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. LOAD TABLE (INFINITE SCROLL)
        public async Task<IActionResult> LoadTable(string search, string sortBy, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Khachhangs
                .Include(k => k.Hoadons) // Include để đếm số hóa đơn và tổng tiền
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(kh =>
                    kh.Hoten.Contains(search) ||
                    kh.Idkh.Contains(search) ||
                    kh.Sodt.Contains(search));
            }

            // Tải về memory để sắp xếp an toàn
            var listAll = await query.ToListAsync();

            // Sắp xếp
            listAll = sortBy switch
            {
                "id_desc" => listAll.OrderByDescending(kh => ParseId(kh.Idkh)).ToList(),
                "name_asc" => listAll.OrderBy(kh => kh.Hoten).ToList(),
                "name_desc" => listAll.OrderByDescending(kh => kh.Hoten).ToList(),
                _ => listAll.OrderBy(kh => ParseId(kh.Idkh)).ToList() // Mặc định: id_asc
            };

            int total = listAll.Count;
            var items = listAll.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            bool hasMore = (page * pageSize) < total;

            return PartialView("_KhachHangRows", new KhachHangScrollVm
            {
                Items = items,
                HasMore = hasMore
            });
        }

        // Hàm phụ trợ để parse ID (KH001 -> 1)
        private int ParseId(string id)
        {
            if (id.Length > 2 && int.TryParse(id.Substring(2), out int n)) return n;
            return 999999;
        }

        // 3. CREATE (MODAL) - Logic an toàn
        public async Task<IActionResult> CreatePartial()
        {
            // Lấy list ID chỉ để sinh mã
            var allIds = await _context.Khachhangs
                .Where(k => k.Idkh.StartsWith("KH"))
                .Select(k => k.Idkh)
                .ToListAsync();

            int max = 0;
            foreach (var id in allIds)
            {
                if (id.Length > 2 && int.TryParse(id.Substring(2), out int n))
                {
                    if (n > max) max = n;
                }
            }

            string newId = $"KH{max + 1:D3}"; // KH001, KH010...
            var kh = new Khachhang { Idkh = newId };
            return PartialView("_CreateModal", kh);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax(Khachhang model)
        {
            if (await _context.Khachhangs.AnyAsync(k => k.Idkh == model.Idkh))
                return Json(new { success = false, message = "Mã khách hàng đã tồn tại!" });

            if (!string.IsNullOrEmpty(model.Sodt) && await _context.Khachhangs.AnyAsync(k => k.Sodt == model.Sodt))
                return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });

            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 4. EDIT (MODAL)
        public async Task<IActionResult> EditPartial(string id)
        {
            var kh = await _context.Khachhangs.FindAsync(id);
            if (kh == null) return Content("<div class='p-3 text-danger'>Không tìm thấy!</div>");
            return PartialView("_EditModal", kh);
        }

        [HttpPost]
        public async Task<IActionResult> EditAjax(Khachhang model)
        {
            var kh = await _context.Khachhangs.FindAsync(model.Idkh);
            if (kh == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // Check trùng SĐT (trừ chính nó)
            if (!string.IsNullOrEmpty(model.Sodt) &&
                await _context.Khachhangs.AnyAsync(k => k.Sodt == model.Sodt && k.Idkh != model.Idkh))
            {
                return Json(new { success = false, message = "Số điện thoại đã thuộc về người khác!" });
            }

            kh.Hoten = model.Hoten;
            kh.Sodt = model.Sodt;
            kh.Dchi = model.Dchi;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 5. DETAILS (MODAL)
        public async Task<IActionResult> DetailsPartial(string id)
        {
            var kh = await _context.Khachhangs
                .Include(k => k.Hoadons) // Lấy lịch sử hóa đơn
                .FirstOrDefaultAsync(k => k.Idkh == id);

            if (kh == null) return Content("<div class='p-3 text-danger'>Không tìm thấy!</div>");
            return PartialView("_DetailsModal", kh);
        }

        // 6. DELETE
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            var kh = await _context.Khachhangs
                .Include(k => k.Hoadons)
                .FirstOrDefaultAsync(k => k.Idkh == id);

            if (kh == null) return Json(new { success = false, message = "Không tìm thấy!" });

            if (kh.Hoadons.Any())
                return Json(new { success = false, message = $"Khách này đã có {kh.Hoadons.Count} hóa đơn, không thể xóa!" });

            _context.Khachhangs.Remove(kh);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}