using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BTL_LTW_QLBIDA.Controllers
{
    [AdminAuthorize]
    public class PhongBanController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;

        public PhongBanController(QlquanBilliardLtw2Context context)
        {
            _context = context;
        }

        // 1. VIEW CHÍNH
        public IActionResult Index()
        {
            // Load danh sách khu vực cho Dropdown lọc
            ViewBag.KhuVucs = new SelectList(_context.Khuvucs.ToList(), "Idkhu", "Tenkhu");
            return View();
        }

        // 2. LOAD TABLE (INFINITE SCROLL)
        public async Task<IActionResult> LoadTable(string search, string khuvuc, string trangthai, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Bans
                .Include(b => b.IdkhuNavigation)
                .AsQueryable();

            // Filter Khu vực
            if (!string.IsNullOrEmpty(khuvuc))
            {
                query = query.Where(b => b.Idkhu == khuvuc);
            }

            // Filter Trạng thái
            if (!string.IsNullOrEmpty(trangthai))
            {
                bool isActive = trangthai == "true";
                query = query.Where(b => b.Trangthai == isActive);
            }

            // Filter Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Idban.Contains(search));
            }

            // Sắp xếp (Mặc định theo ID Bàn số tăng dần)
            var listAll = await query.ToListAsync();
            var sortedList = listAll.OrderBy(b =>
            {
                // Logic parse số từ B001 -> 1
                if (b.Idban.Length > 1 && int.TryParse(b.Idban.Substring(1), out int n)) return n;
                return 999999;
            }).ToList();

            int total = sortedList.Count;
            var items = sortedList.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            bool hasMore = (page * pageSize) < total;

            return PartialView("_BanRows", new BanScrollVm { Items = items, HasMore = hasMore });
        }

        // 3. CREATE (MODAL)
        public async Task<IActionResult> CreatePartial()
        {
            ViewBag.KhuVucs = new SelectList(await _context.Khuvucs.ToListAsync(), "Idkhu", "Tenkhu");

            // Logic sinh mã B001, B002...
            var allIds = await _context.Bans
                .Where(b => b.Idban.StartsWith("B"))
                .Select(b => b.Idban)
                .ToListAsync();

            int max = 0;
            foreach (var id in allIds)
            {
                if (id.Length > 1 && int.TryParse(id.Substring(1), out int n))
                {
                    if (n > max) max = n;
                }
            }
            string newId = $"B{max + 1:D3}";

            var ban = new Ban { Idban = newId, Giatien = 0 };
            return PartialView("_CreateModal", ban);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax(Ban model)
        {
            if (await _context.Bans.AnyAsync(b => b.Idban == model.Idban))
                return Json(new { success = false, message = "Mã bàn đã tồn tại!" });

            model.Trangthai = false; // Mặc định là Trống
            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 4. EDIT (MODAL)
        public async Task<IActionResult> EditPartial(string id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return Content("Không tìm thấy!");

            ViewBag.KhuVucs = new SelectList(await _context.Khuvucs.ToListAsync(), "Idkhu", "Tenkhu", ban.Idkhu);
            return PartialView("_EditModal", ban);
        }

        [HttpPost]
        public async Task<IActionResult> EditAjax(Ban model)
        {
            var ban = await _context.Bans.FindAsync(model.Idban);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy!" });

            if (ban.Trangthai == true)
                return Json(new { success = false, message = "Không thể sửa khi bàn đang có khách!" });

            ban.Idkhu = model.Idkhu;
            ban.Giatien = model.Giatien;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 5. DETAILS (MODAL)
        public async Task<IActionResult> DetailsPartial(string id)
        {
            var ban = await _context.Bans
                .Include(b => b.IdkhuNavigation)
                .FirstOrDefaultAsync(b => b.Idban == id);

            if (ban == null) return Content("Không tìm thấy!");

            // Lấy lịch sử giao dịch (10 cái gần nhất)
            ViewBag.History = await _context.Hoadons
                .Include(h => h.IdphienNavigation)
                .Include(h => h.IdnvNavigation)
                .Where(h => h.IdphienNavigation.Idban == id)
                .OrderByDescending(h => h.Ngaylap)
                .Take(10)
                .ToListAsync();

            return PartialView("_DetailsModal", ban);
        }

        // 6. DELETE
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy!" });

            if (ban.Trangthai == true)
                return Json(new { success = false, message = "Bàn đang hoạt động, không thể xóa!" });

            // Kiểm tra ràng buộc khóa ngoại (Hóa đơn cũ)
            var hasHoadon = await _context.Phienchois.AnyAsync(p => p.Idban == id);
            if (hasHoadon)
                return Json(new { success = false, message = "Bàn này đã có dữ liệu lịch sử, không thể xóa!" });

            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 7. TOGGLE STATUS (Dev only helper - Optional)
        [HttpPost]
        public async Task<IActionResult> ToggleStatusAjax(string id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban != null)
            {
                ban.Trangthai = !(ban.Trangthai ?? false);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}