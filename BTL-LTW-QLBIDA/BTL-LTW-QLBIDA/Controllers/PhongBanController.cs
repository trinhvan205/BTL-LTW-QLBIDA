using BTL_LTW_QLBIDA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BTL_LTW_QLBIDA.Controllers
{
    public class PhongBanController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;

        public PhongBanController(QlquanBilliardLtw2Context context)
        {
            _context = context;
        }

        // --- HÀM 1: Index (Giữ nguyên) ---
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

        // --- HÀM 2: FilterBan (Giữ nguyên) ---
        [HttpGet]
        public async Task<IActionResult> FilterBan(string khuVuc, string trangThai, string timKiem, int pageSize = 10, int page = 1)
        {
            var banQuery = _context.Bans
                                .Include(b => b.IdkhuNavigation)
                                .AsQueryable();

            // 1. Lọc Trạng thái
            bool? selectedTrangThai = null;
            if (trangThai == "true") selectedTrangThai = true;
            else if (trangThai == "false") selectedTrangThai = false;

            if (selectedTrangThai.HasValue)
            {
                banQuery = banQuery.Where(b => b.Trangthai == selectedTrangThai.Value);
            }

            // 2. Lọc Khu Vực
            if (!string.IsNullOrEmpty(khuVuc))
            {
                banQuery = banQuery.Where(b => b.Idkhu != null && b.Idkhu.Trim() == khuVuc.Trim());
            }

            // 3. Lọc Tìm kiếm
            if (!string.IsNullOrEmpty(timKiem))
            {
                banQuery = banQuery.Where(b => b.Idban.Contains(timKiem));
            }

            var totalItems = await banQuery.CountAsync();

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


        // --- HÀM 3 & 4: Create (Bàn) (Giữ nguyên) ---
        public async Task<IActionResult> Create()
        {
            ViewData["KhuVucList"] = new SelectList(await _context.Khuvucs.ToListAsync(), "Idkhu", "Tenkhu");
            return PartialView("_CreatePartial");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idban,Idkhu,Giatien")] Ban ban)
        {
            ban.Trangthai = false;
            if (ModelState.IsValid)
            {
                _context.Add(ban);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            ViewData["KhuVucList"] = new SelectList(await _context.Khuvucs.ToListAsync(), "Idkhu", "Tenkhu", ban.Idkhu);
            return PartialView("_CreatePartial", ban);
        }

        // --- HÀM 5: CREATE KHU VỰC (Giữ nguyên) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateKhuVuc([FromForm] string tenKhuVuc, [FromForm] string? ghiChu)
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

        // --- HÀM 6: GET KHU VỰC (Giữ nguyên) ---
        [HttpGet]
        public async Task<IActionResult> GetKhuVucDetails(string id)
        {
            var khuvuc = await _context.Khuvucs
                                 .Where(k => k.Idkhu == id)
                                 .Select(k => new { k.Idkhu, k.Tenkhu, k.Ghichu })
                                 .FirstOrDefaultAsync();
            if (khuvuc == null)
            {
                return NotFound();
            }
            return Json(khuvuc);
        }

        // --- HÀM 7: UPDATE KHU VỰC (Giữ nguyên) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateKhuVuc([FromForm] string editIdKhu, [FromForm] string editTenKhuVuc, [FromForm] string? editGhiChu)
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

        // --- HÀM 8: DELETE KHU VỰC (ĐÃ SỬA LOGIC XÓA TẦNG) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteKhuVuc([FromForm] string id)
        {
            try
            {
                // 1. Tìm khu vực cần xóa
                var khuvuc = await _context.Khuvucs.FindAsync(id);
                if (khuvuc == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khu vực." });
                }

                // 2. Tìm tất cả Bàn liên quan
                var bansToDelete = await _context.Bans.Where(b => b.Idkhu == id).ToListAsync();
                if (bansToDelete.Any())
                {
                    // 3. Với mỗi Bàn, tìm và xóa các Phiên chơi, Hóa đơn... (Xóa tầng)
                    foreach (var ban in bansToDelete)
                    {
                        var phienChoiToDelete = await _context.Phienchois.Where(p => p.Idban == ban.Idban).ToListAsync();
                        if (phienChoiToDelete.Any())
                        {
                            foreach (var phien in phienChoiToDelete)
                            {
                                var hoaDonToDelete = await _context.Hoadons.Where(h => h.Idphien == phien.Idphien).ToListAsync();
                                if (hoaDonToDelete.Any())
                                {
                                    foreach (var hoaDon in hoaDonToDelete)
                                    {
                                        var hoaDonDvToDelete = await _context.Hoadondvs.Where(hdv => hdv.Idhd == hoaDon.Idhd).ToListAsync();
                                        _context.Hoadondvs.RemoveRange(hoaDonDvToDelete);
                                    }
                                    _context.Hoadons.RemoveRange(hoaDonToDelete);
                                }
                            }
                            _context.Phienchois.RemoveRange(phienChoiToDelete);
                        }
                    }
                    // 4. Xóa các Bàn
                    _context.Bans.RemoveRange(bansToDelete);
                }

                // 5. Xóa Khu vực
                _context.Khuvucs.Remove(khuvuc);

                // 6. Lưu tất cả thay đổi
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa khu vực và tất cả dữ liệu liên quan thành công!" });
            }
            catch (DbUpdateException ex)
            {
                // Bắt lỗi nếu vẫn còn ràng buộc nào đó (hiếm khi xảy ra nếu logic trên đúng)
                return Json(new { success = false, message = "Lỗi CSDL: Không thể xóa. " + ex.InnerException?.Message });
            }
        }

        // --- HÀM 9: LẤY CHI TIẾT BÀN & LỊCH SỬ (KHÔNG CẦN SỬA DB) ---
        [HttpGet]
        public async Task<IActionResult> GetBanDetail(string id)
        {
            // 1. Lấy thông tin Bàn
            var ban = await _context.Bans
                                .Include(b => b.IdkhuNavigation)
                                .FirstOrDefaultAsync(b => b.Idban == id);

            if (ban == null) return NotFound();

            // 2. Lấy lịch sử giao dịch (10 hóa đơn gần nhất của bàn này)
            // Logic: Hóa đơn -> Phiên chơi -> Bàn
            var history = await _context.Hoadons
                                    .Include(h => h.IdphienNavigation) // Join với Phiên
                                    .Where(h => h.IdphienNavigation.Idban == id) // Lọc theo Bàn
                                    .OrderByDescending(h => h.Ngaylap)
                                    .Take(10)
                                    .Include(h => h.IdnvNavigation) // Lấy tên nhân viên
                                    .ToListAsync();

            ViewBag.History = history;

            return PartialView("_BanDetailTabsPartial", ban);
        }

        // --- HÀM 10: LẤY DỮ LIỆU BÀN ĐỂ SỬA (GET JSON) ---
        [HttpGet]
        public async Task<IActionResult> GetBanForEdit(string id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return NotFound();

            // Trả về JSON để JavaScript điền vào Modal
            return Json(new
            {
                id = ban.Idban,
                idKhu = ban.Idkhu,
                giaTien = ban.Giatien,
                trangThai = ban.Trangthai
            });
        }

        // --- HÀM 11: CẬP NHẬT BÀN (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBan([FromForm] string editIdBan, [FromForm] string editIdKhuBan, [FromForm] decimal editGiaTien)
        {
            var ban = await _context.Bans.FindAsync(editIdBan);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy bàn." });

            // Cập nhật thông tin
            ban.Idkhu = editIdKhuBan;
            ban.Giatien = editGiaTien;
            // (Nếu có thêm cột Ghi chú, Số ghế thì gán ở đây)

            _context.Update(ban);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật thành công!" });
        }

        // --- HÀM 12: XÓA BÀN (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBan([FromForm] string id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy bàn." });

            // XÓA TẦNG (Cascading Delete): Xóa Phiên -> Hóa đơn -> Chi tiết trước
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

            // Cuối cùng xóa Bàn
            _context.Bans.Remove(ban);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã xóa bàn thành công!" });
        }

        // --- HÀM 13: ĐỔI TRẠNG THÁI BÀN (Toggle) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatusBan([FromForm] string id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban == null) return Json(new { success = false, message = "Không tìm thấy bàn." });

            // Đảo ngược trạng thái (True thành False, False thành True)
            // Lưu ý: Nếu Trangthai là null thì coi như false rồi chuyển thành true
            ban.Trangthai = !(ban.Trangthai ?? false);

            _context.Update(ban);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật dữ liệu thành công" });
        }

    }
}