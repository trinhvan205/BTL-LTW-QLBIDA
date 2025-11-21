using System; // Import System để sử dụng Math
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BTL_LTW_QLBIDA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BTL_LTW_QLBIDA.Filters;


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

        // 🟢 HÀM HỖ TRỢ: TÌM ID BÀN TIẾP THEO
        private string GetNextBanId()
        {
            // Lọc các Id có thể chuyển thành số (ví dụ: B010)
            var maxId = _context.Bans
                .Select(b => b.Idban)
                .AsEnumerable()
                .Where(id => id != null && id.StartsWith("B") && int.TryParse(id.Substring(1), out _))
                .Select(id => int.Parse(id.Substring(1)))
                .DefaultIfEmpty(0) // Nếu không có bàn nào, bắt đầu từ 0
                .Max();

            // Tăng lên 1 và định dạng lại (ví dụ: 11 thành B011)
            return $"B{(maxId + 1):D3}";
        }

        // ====================================================================
        // I. QUẢN LÝ BÀN (Bàn)
        // ====================================================================

        // [HÀM 1] HIỂN THỊ TRANG CHÍNH (Giữ nguyên)
        public async Task<IActionResult> Index()
        {
            int page = 1;
            int pageSize = 10;

            var banQuery = _context.Bans
                                        .Include(b => b.IdkhuNavigation)
                                        .Where(b => b.Trangthai == true)
                                        .AsQueryable();

            var totalItems = await banQuery.CountAsync();

            var pagedItems = await banQuery
                                                .Skip((page - 1) * pageSize)
                                                .Take(pageSize)
                                                .ToListAsync();

            var pagedResult = new PagedResult<Ban>
            {
                Items = pagedItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            var khuVucList = await _context.Khuvucs
                                             .Select(k => new { k.Idkhu, k.Tenkhu })
                                             .ToListAsync();

            var viewModel = new PhongBanIndexViewModel
            {
                PagedBans = pagedResult,
                KhuVucs = new SelectList(khuVucList, "Idkhu", "Tenkhu"),
                SelectedKhuVuc = "",
                SearchString = "",
                SelectedTrangThai = true
            };

            return View(viewModel);
        }

        // [HÀM 2] LỌC BÀN (AJAX/Partial View) - Giữ nguyên logic phân trang
        [HttpGet]
        public async Task<IActionResult> FilterBan(string khuVuc, string trangThai, string timKiem, int pageSize = 10, int page = 1)
        {
            var banQuery = _context.Bans
                                         .Include(b => b.IdkhuNavigation)
                                         .AsQueryable();

            bool? selectedTrangThai = null;
            if (trangThai == "true") selectedTrangThai = true;
            else if (trangThai == "false") selectedTrangThai = false;

            if (selectedTrangThai.HasValue)
            {
                banQuery = banQuery.Where(b => b.Trangthai == selectedTrangThai.Value);
            }

            if (!string.IsNullOrEmpty(khuVuc))
            {
                banQuery = banQuery.Where(b => b.Idkhu != null && b.Idkhu.Trim() == khuVuc.Trim());
            }

            if (!string.IsNullOrEmpty(timKiem))
            {
                banQuery = banQuery.Where(b => b.Idban.Contains(timKiem));
            }

            var totalItems = await banQuery.CountAsync();

            // Tính toán và điều chỉnh số trang
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Min(page, Math.Max(1, totalPages));

            if (totalItems > 0 && page < 1) page = 1;
            if (totalItems == 0) page = 1;


            var pagedItems = await banQuery
                                                 .Skip((page - 1) * pageSize)
                                                 .Take(pageSize)
                                                 .ToListAsync();

            var viewModel = new PagedResult<Ban>
            {
                Items = pagedItems,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return PartialView("_BanTablePartial", viewModel);
        }

        // [HÀM 3] GET FORM THÊM BÀN (Partial View) - 🟢 ĐÃ SỬA: Lấy ID bàn tự động
        public async Task<IActionResult> Create()
        {
            ViewData["KhuVucList"] = new SelectList(await _context.Khuvucs.ToListAsync(), "Idkhu", "Tenkhu");

            // 🟢 Thêm ID bàn tiếp theo vào ViewBag
            ViewBag.NextBanId = GetNextBanId();

            return PartialView("_CreatePartial");
        }

        // [HÀM 4] POST THÊM BÀN (ĐÃ SỬA: Thêm tham số page và pageSize)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idban,Idkhu,Giatien")] Ban ban, [FromForm] int page, [FromForm] int pageSize)
        {
            ban.Trangthai = false;

            if (ModelState.IsValid)
            {
                _context.Add(ban);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm bàn thành công!" });
            }

            // Nếu thất bại, trả về ID tiếp theo (nếu chưa có) hoặc giữ ID cũ (nếu có)
            ViewData["KhuVucList"] = new SelectList(await _context.Khuvucs.ToListAsync(), "Idkhu", "Tenkhu", ban.Idkhu);
            ViewBag.NextBanId = ban.Idban ?? GetNextBanId();

            return PartialView("_CreatePartial", ban);
        }

        // [HÀM 9] LẤY CHI TIẾT BÀN & LỊCH SỬ (Partial View) - Giữ nguyên
        [HttpGet]
        public async Task<IActionResult> GetBanDetail(string id)
        {
            var ban = await _context.Bans
                                         .Include(b => b.IdkhuNavigation)
                                         .FirstOrDefaultAsync(b => b.Idban == id);

            if (ban == null) return NotFound();

            var history = await _context.Hoadons
                                                 .Include(h => h.IdphienNavigation)
                                                 .Where(h => h.IdphienNavigation.Idban == id)
                                                 .OrderByDescending(h => h.Ngaylap)
                                                 .Take(10)
                                                 .Include(h => h.IdnvNavigation)
                                                 .ToListAsync();

            ViewBag.History = history;

            return PartialView("_BanDetailTabsPartial", ban);
        }

        // [HÀM 10] LẤY DỮ LIỆU BÀN ĐỂ SỬA (GET JSON) - Giữ nguyên
        [HttpGet]
        public async Task<IActionResult> GetBanForEdit(string id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return NotFound();

            var khuVucList = new SelectList(
                await _context.Khuvucs.ToListAsync(),
                "Idkhu",
                "Tenkhu",
                ban.Idkhu
            );

            return Json(new
            {
                id = ban.Idban,
                idKhu = ban.Idkhu,
                giaTien = ban.Giatien,
                trangThai = ban.Trangthai,
                khuVucList = khuVucList.Select(x => new { value = x.Value, text = x.Text, selected = x.Selected })
            });
        }

        // [HÀM 11] CẬP NHẬT BÀN (POST) - ĐÃ SỬA: Thêm tham số page và pageSize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBan([Bind("Idban,Idkhu,Giatien")] Ban updatedBan, [FromForm] int page, [FromForm] int pageSize)
        {
            var ban = await _context.Bans.FindAsync(updatedBan.Idban);

            if (ban == null) return Json(new { success = false, message = "Không tìm thấy bàn." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                                         .ToDictionary(
                                             kvp => kvp.Key,
                                             kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                                         );

                return Json(new { success = false, message = "Dữ liệu nhập không hợp lệ.", errors = errors });
            }

            // ⚠️ KHÔNG CHO CẬP NHẬT BÀN ĐANG HOẠT ĐỘNG (Logic bảo mật)
            if (ban.Trangthai == true)
            {
                return Json(new { success = false, message = "Không thể cập nhật thông tin bàn đang hoạt động." });
            }

            ban.Idkhu = updatedBan.Idkhu;
            ban.Giatien = updatedBan.Giatien;

            try
            {
                _context.Update(ban);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Lỗi đồng bộ dữ liệu. Vui lòng thử lại." });
            }

            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // [HÀM 12] XÓA BÀN (POST) - ĐÃ SỬA: Thêm tham số page và pageSize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBan([FromForm] string id, [FromForm] int page, [FromForm] int pageSize)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy bàn." });

            if (ban.Trangthai == true)
            {
                return Json(new { success = false, message = "Không thể xóa bàn đang hoạt động. Vui lòng chuyển bàn sang trạng thái 'Trống' trước khi xóa." });
            }

            // XÓA TẦNG (Cascading Delete logic) - Giữ nguyên
            var phienChoiToDelete = await _context.Phienchois.Where(p => p.Idban == id).ToListAsync();
            foreach (var phien in phienChoiToDelete)
            {
                var hoaDonToDelete = await _context.Hoadons.Where(h => h.Idphien == phien.Idphien).ToListAsync();
                foreach (var hoaDon in hoaDonToDelete)
                {
                    var hoaDonDv = await _context.Hoadondvs.Where(hdv => hdv.Idhd == hoaDon.Idhd).ToListAsync();
                    _context.Hoadondvs.RemoveRange(hoaDonDv);
                }
                _context.Hoadons.RemoveRange(hoaDonToDelete);
            }
            _context.Phienchois.RemoveRange(phienChoiToDelete);

            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa bàn thành công!" });
        }

        // [HÀM 13] ĐỔI TRẠNG THÁI BÀN (Toggle) - ĐÃ SỬA: Thêm tham số page và pageSize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusBan([FromForm] string id, [FromForm] int page, [FromForm] int pageSize)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy bàn." });

            ban.Trangthai = !(ban.Trangthai ?? false);

            _context.Update(ban);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật dữ liệu thành công" });
        }

        // ====================================================================
        // II. QUẢN LÝ KHU VỰC - Sửa để giữ nguyên trang bàn sau khi thêm/sửa/xóa khu vực
        // ====================================================================

        // [HÀM 5] CREATE KHU VỰC (ĐÃ SỬA: Thêm tham số page và pageSize)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKhuVuc([FromForm] string tenKhuVuc, [FromForm] string? ghiChu, [FromForm] int page, [FromForm] int pageSize)
        {
            string newIdKhu = tenKhuVuc.Trim();
            if (string.IsNullOrEmpty(newIdKhu))
            {
                return Json(new { success = false, message = "Tên khu vực không được để trống." });
            }
            var idDaTonTai = await _context.Khuvucs.AnyAsync(k => k.Idkhu == newIdKhu);
            if (idDaTonTai)
            {
                return Json(new { success = false, message = "Tên khu vực này đã tồn tại." });
            }

            var newKhu = new Khuvuc
            {
                Idkhu = newIdKhu,
                Tenkhu = tenKhuVuc,
                Ghichu = ghiChu
            };
            _context.Khuvucs.Add(newKhu);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Thêm khu vực thành công!",
                newKhuVuc = new { id = newKhu.Idkhu, ten = newKhu.Tenkhu }
            });
        }

        // [HÀM 7] UPDATE KHU VỰC (ĐÃ SỬA: Thêm tham số page và pageSize)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateKhuVuc([FromForm] string editIdKhu, [FromForm] string editTenKhuVuc, [FromForm] string? editGhiChu, [FromForm] int page, [FromForm] int pageSize)
        {
            var khuvuc = await _context.Khuvucs.FindAsync(editIdKhu);
            if (khuvuc == null)
            {
                return Json(new { success = false, message = "Không tìm thấy khu vực." });
            }
            if (khuvuc.Tenkhu != editTenKhuVuc)
            {
                var tenDaTonTai = await _context.Khuvucs.AnyAsync(k => k.Tenkhu == editTenKhuVuc && k.Idkhu != editIdKhu);
                if (tenDaTonTai)
                {
                    return Json(new { success = false, message = "Tên khu vực này đã bị trùng." });
                }
            }
            khuvuc.Tenkhu = editTenKhuVuc;
            khuvuc.Ghichu = editGhiChu;
            _context.Update(khuvuc);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // [HÀM 8] DELETE KHU VỰC (ĐÃ SỬA: Thêm tham số page và pageSize)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteKhuVuc([FromForm] string id, [FromForm] int page, [FromForm] int pageSize)
        {
            try
            {
                var khuvuc = await _context.Khuvucs.FindAsync(id);
                if (khuvuc == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khu vực." });
                }

                var banCount = await _context.Bans.CountAsync(b => b.Idkhu == id);
                if (banCount > 0)
                {
                    return Json(new { success = false, message = $"Không thể xóa khu vực này vì vẫn còn {banCount} bàn thuộc về nó. Vui lòng xóa hoặc chuyển bàn trước." });
                }

                _context.Khuvucs.Remove(khuvuc);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa khu vực thành công!" });
            }
            catch (DbUpdateException ex)
            {
                return Json(new { success = false, message = "Lỗi CSDL: Không thể xóa. " + ex.InnerException?.Message });
            }
        }
    }
}