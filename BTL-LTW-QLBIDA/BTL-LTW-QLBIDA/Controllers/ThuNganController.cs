using BTL_LTW_QLBIDA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Controllers
{
    public class ThuNganController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;

        public ThuNganController(QlquanBilliardLtw2Context context)
        {
            _context = context;
        }

        // GET: ThuNgan - Màn hình chính
        public IActionResult Index()
        {
            // Load danh sách khu vực để hiển thị tabs
            ViewBag.KhuVucs = _context.Khuvucs.ToList();

            // ← THÊM: Load danh sách loại dịch vụ
            ViewBag.LoaiDichVus = _context.Loaidichvus.ToList();
            return View();
        }

        // GET: ThuNgan/GetDanhSachBan - Load danh sách bàn
        public IActionResult GetDanhSachBan(string? khuVucId)
        {
            var bans = _context.Bans
                .Include(b => b.IdkhuNavigation)
                .Include(b => b.Phienchois.Where(p => p.Gioketthuc == null)) // Chỉ lấy phiên đang chơi
                .Where(b => string.IsNullOrEmpty(khuVucId) || b.Idkhu == khuVucId)
                .OrderBy(b => b.Idban)
                .ToList();

            return PartialView("~/Views/Shared/ThuNgan/_DanhSachBan.cshtml", bans);
        }

        // GET: ThuNgan/GetDanhSachDichVu - Load danh sách dịch vụ
        public IActionResult GetDanhSachDichVu(string? loaiDv)
        {
            var dichVus = _context.Dichvus
                .Where(d => d.Hienthi == true && d.Soluong > 0)
                .Where(d => string.IsNullOrEmpty(loaiDv) || d.Idloai == loaiDv)
                .OrderBy(d => d.Tendv)
                .ToList();

            return PartialView("~/Views/Shared/ThuNgan/_DanhSachDichVu.cshtml", dichVus);
        }

        // GET: ThuNgan/GetHoaDonChiTiet - Load hóa đơn chi tiết của bàn
        public IActionResult GetHoaDonChiTiet(string idBan)
        {
            var ban = _context.Bans
                .Include(b => b.IdkhuNavigation)
                .Include(b => b.Phienchois.Where(p => p.Gioketthuc == null))
                    .ThenInclude(p => p.Hoadons.Where(h => h.Trangthai == false))
                    .ThenInclude(h => h.Hoadondvs)
                    .ThenInclude(hd => hd.IddvNavigation)
                .FirstOrDefault(b => b.Idban == idBan);

            if (ban == null)
            {
                return PartialView("~/Views/Shared/ThuNgan/_HoaDonChiTiet.cshtml", null);
            }

            // Tính tiền giờ chơi
            var phienChoi = ban.Phienchois.FirstOrDefault();
            if (phienChoi != null && phienChoi.Giobatdau != null)
            {
                TimeSpan gioChoi = DateTime.Now - phienChoi.Giobatdau.GetValueOrDefault();

                // Tổng số phút đã chơi
                int tongPhut = (int)gioChoi.TotalMinutes;

                // ← TÍNH THEO BLOCK 15 PHÚT (Chỉ cần > 0 phút thì +1 block)
                // VD: 0 phút 1 giây → tongPhut = 0 → 0 / 15 = 0 → + 1 = 1 block → 15 phút
                // VD: 15 phút 1 giây → tongPhut = 15 → 15 / 15 = 1 → + 1 = 2 block → 30 phút
                int soBlock15Phut = (tongPhut / 15) + 1;
                int phutTinhTien = soBlock15Phut * 15;

                // Chuyển đổi sang giờ để tính tiền
                decimal gioTinhTien = phutTinhTien / 60.0m;

                // Gửi thông tin sang View
                ViewBag.SoGio = gioChoi.Hours;
                ViewBag.SoPhut = gioChoi.Minutes;
                ViewBag.SoGiay = gioChoi.Seconds;
                ViewBag.TongPhut = tongPhut;
                ViewBag.PhutTinhTien = phutTinhTien;
                ViewBag.TienGio = gioTinhTien * (ban.Giatien ?? 0);
            }
            else
            {
                ViewBag.SoGio = 0;
                ViewBag.SoPhut = 0;
                ViewBag.SoGiay = 0;
                ViewBag.TongPhut = 0;
                ViewBag.PhutTinhTien = 0;
                ViewBag.TienGio = 0;
            }

            return PartialView("~/Views/Shared/ThuNgan/_HoaDonChiTiet.cshtml", ban);
        }

        // POST: ThuNgan/BatDauChoi - Bắt đầu chơi (mở bàn)
        [HttpPost]
        public IActionResult BatDauChoi(string idBan)
        {
            try
            {
                var ban = _context.Bans.Find(idBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.Trangthai == true)
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });

                // Tạo phiên chơi mới
                var phienChoi = new Phienchoi
                {
                    Idphien = "P" + DateTime.Now.Ticks,
                    Idban = idBan,
                    Giobatdau = DateTime.Now,
                    Gioketthuc = null
                };
                _context.Phienchois.Add(phienChoi);

                // Tạo hóa đơn mới
                var hoaDon = new Hoadon
                {
                    Idhd = "HD" + DateTime.Now.Ticks,
                    Idphien = phienChoi.Idphien,
                    Idnv = "NV001", // TODO: Lấy từ session đăng nhập
                    Ngaylap = DateTime.Now,
                    Tongtien = 0,
                    Trangthai = false
                };
                _context.Hoadons.Add(hoaDon);

                // Đổi trạng thái bàn
                ban.Trangthai = true;

                _context.SaveChanges();

                return Json(new { success = true, message = "Bắt đầu chơi thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ThuNgan/ThemDichVu - Thêm dịch vụ vào hóa đơn
        [HttpPost]
        public IActionResult ThemDichVu(string idBan, string idDv, int soLuong = 1)
        {
            try
            {
                // Lấy hóa đơn chưa thanh toán của bàn
                var hoaDon = _context.Hoadons
                    .Include(h => h.IdphienNavigation)
                    .Where(h => h.IdphienNavigation.Idban == idBan && h.Trangthai == false)
                    .FirstOrDefault();

                if (hoaDon == null)
                    return Json(new { success = false, message = "Bàn chưa được mở" });

                // Kiểm tra dịch vụ đã có trong hóa đơn chưa
                var hoaDonDv = _context.Hoadondvs
                    .FirstOrDefault(hd => hd.Idhd == hoaDon.Idhd && hd.Iddv == idDv);

                if (hoaDonDv != null)
                {
                    // Cập nhật số lượng
                    hoaDonDv.Soluong += soLuong;
                }
                else
                {
                    // Thêm mới
                    hoaDonDv = new Hoadondv
                    {
                        Idhd = hoaDon.Idhd,
                        Iddv = idDv,
                        Soluong = soLuong
                    };
                    _context.Hoadondvs.Add(hoaDonDv);
                }

                // Giảm số lượng dịch vụ trong kho
                var dichVu = _context.Dichvus.Find(idDv);
                if (dichVu != null)
                {
                    dichVu.Soluong -= soLuong;
                }

                _context.SaveChanges();

                return Json(new { success = true, message = "Thêm dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ThuNgan/CapNhatSoLuong - Cập nhật số lượng dịch vụ
        [HttpPost]
        public IActionResult CapNhatSoLuong(string idHoaDon, string idDv, int soLuong)
        {
            try
            {
                var hoaDonDv = _context.Hoadondvs
                    .FirstOrDefault(hd => hd.Idhd == idHoaDon && hd.Iddv == idDv);

                if (hoaDonDv == null)
                    return Json(new { success = false, message = "Không tìm thấy dịch vụ" });

                var soLuongCu = hoaDonDv.Soluong ?? 0;
                var chenhlech = soLuong - soLuongCu;

                if (soLuong <= 0)
                {
                    // Xóa dịch vụ
                    _context.Hoadondvs.Remove(hoaDonDv);
                }
                else
                {
                    hoaDonDv.Soluong = soLuong;
                }

                // Cập nhật kho
                var dichVu = _context.Dichvus.Find(idDv);
                if (dichVu != null)
                {
                    dichVu.Soluong -= chenhlech;
                }

                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ThuNgan/ThanhToan - Thanh toán hóa đơn
        [HttpPost]
        public IActionResult ThanhToan(string idHoaDon)
        {
            try
            {
                var hoaDon = _context.Hoadons
                    .Include(h => h.IdphienNavigation)
                        .ThenInclude(p => p.IdbanNavigation)
                    .Include(h => h.Hoadondvs)
                        .ThenInclude(hd => hd.IddvNavigation)
                    .FirstOrDefault(h => h.Idhd == idHoaDon);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                // Tính tổng tiền
                var phien = hoaDon.IdphienNavigation;
                var ban = phien.IdbanNavigation;

                // Tính tiền giờ
                TimeSpan gioChoi = DateTime.Now - phien.Giobatdau.GetValueOrDefault();
                int tongPhut = (int)gioChoi.TotalMinutes;

                // ← TÍNH THEO BLOCK 15 PHÚT (Chỉ cần > 0 thì +1 block)
                int soBlock15Phut = (tongPhut / 15) + 1;
                int phutTinhTien = soBlock15Phut * 15;
                decimal gioTinhTien = phutTinhTien / 60.0m;
                decimal tienGio = gioTinhTien * (ban.Giatien ?? 0);

                // Tính tiền dịch vụ
                decimal tienDichVu = 0;
                foreach (var item in hoaDon.Hoadondvs)
                {
                    tienDichVu += (item.IddvNavigation?.Giatien ?? 0) * (item.Soluong ?? 0);
                }

                decimal tongTien = tienGio + tienDichVu;

                // Cập nhật hóa đơn
                hoaDon.Tongtien = tongTien;
                hoaDon.Trangthai = true; // Đã thanh toán

                // Kết thúc phiên chơi
                phien.Gioketthuc = DateTime.Now;

                // Đổi trạng thái bàn về trống
                ban.Trangthai = false;

                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Thanh toán thành công",
                    tongTien = tongTien
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
