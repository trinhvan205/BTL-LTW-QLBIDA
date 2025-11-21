using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Helpers; // ← THÊM
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Services; // ← THÊM
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Controllers
{
    [AuthorizeSession]
    public class ThuNganController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;
        private readonly PdfService _pdfService; // ← THÊM

        public ThuNganController(QlquanBilliardLtw2Context context, PdfService pdfService) // ← THÊM
        {
            _context = context;
            _pdfService = pdfService; // ← THÊM
        }

        // GET: ThuNgan - Màn hình chính
        public IActionResult Index()
        {
            // Load danh sách khu vực để hiển thị tabs
            ViewBag.KhuVucs = _context.Khuvucs.ToList();

            // ← THÊM: Load danh sách loại dịch vụ
            ViewBag.LoaiDichVus = _context.Loaidichvus.ToList();

            // ← THÊM: Load phương thức thanh toán
            ViewBag.PhuongThucThanhToans = _context.Phuongthucthanhtoans
                .Where(p => p.Hienthi == true)
                .ToList();

            // ✅ THÊM: Truyền thông tin nhân viên vào ViewBag
            ViewBag.TenNhanVien = HttpContext.Session.GetString("HoTenNV") ?? "Nhân viên";
            ViewBag.IdNhanVien = HttpContext.Session.GetString("IdNV") ?? "";

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

        // POST: ThuNgan/BatDauChoi
        [HttpPost]
        public IActionResult BatDauChoi(string idBan)
        {
            try
            {
                var ban = _context.Bans.FirstOrDefault(b => b.Idban == idBan);
                if (ban == null)
                    return Json(new { success = false, message = "Không tìm thấy bàn" });

                if (ban.Trangthai == true)
                    return Json(new { success = false, message = "Bàn đang được sử dụng" });

                // ← GỌI hàm static để tạo mã
                string maPhienChoi = MaHoaDonHelper.TaoMaPhienChoi(_context);

                var phienChoi = new Phienchoi
                {
                    Idphien = maPhienChoi,
                    Idban = idBan,
                    Giobatdau = DateTime.Now,
                    Gioketthuc = null
                };
                _context.Phienchois.Add(phienChoi);

                // ← GỌI hàm static để tạo mã
                string maHoaDon = MaHoaDonHelper.TaoMaHoaDon(_context);

                //string? idNhanVien = HttpContext.Session.GetString("UserId");
                // ✅ LẤY ID NHÂN VIÊN TỪ SESSION
                string? idNhanVien = HttpContext.Session.GetString("IdNV");

                if (string.IsNullOrEmpty(idNhanVien))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn!" });
                }

                var hoaDon = new Hoadon
                {
                    Idhd = maHoaDon,
                    Idphien = maPhienChoi,
                    Idnv = idNhanVien,
                    Ngaylap = DateTime.Now,
                    Tongtien = 0,
                    Trangthai = false,
                    Idpttt = "PTTT001"
                };
                _context.Hoadons.Add(hoaDon);

                ban.Trangthai = true;
                _context.SaveChanges();

                return Json(new { success = true, message = "Đã bắt đầu tính giờ" });
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

        // POST: ThuNgan/ThanhToan - Thanh toán hóa đơn & tạo PDF tạm
        [HttpPost]
        public IActionResult ThanhToan(string idHoaDon, string phuongThucThanhToan = "PTTT001")
        {
            try
            {
                var hoaDon = _context.Hoadons
                    .Include(h => h.IdphienNavigation)
                        .ThenInclude(p => p.IdbanNavigation)
                    .Include(h => h.Hoadondvs)
                        .ThenInclude(hd => hd.IddvNavigation)
                    .Include(h => h.IdnvNavigation)      // ← Load để hiển thị trong modal
                    .Include(h => h.IdkhNavigation)      // ← Load khách hàng
                    .FirstOrDefault(h => h.Idhd == idHoaDon);

                if (hoaDon == null)
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn!" });

                var phien = hoaDon.IdphienNavigation;
                var ban = phien.IdbanNavigation;

                // Tính tiền giờ
                TimeSpan gioChoi = DateTime.Now - phien.Giobatdau.GetValueOrDefault();
                int tongPhut = (int)gioChoi.TotalMinutes;
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

                // ✅✅✅ QUAN TRỌNG: GÁN ID NHÂN VIÊN TỪ SESSION
                var idNhanVien = HttpContext.Session.GetString("IdNV");
                if (!string.IsNullOrEmpty(idNhanVien))
                {
                    hoaDon.Idnv = idNhanVien;
                    Console.WriteLine($"✅ Đã gán IdNV: {idNhanVien}");
                }
                else
                {
                    Console.WriteLine("⚠️ CẢNH BÁO: Không có IdNV trong session!");
                }

                // ← THANH TOÁN & ĐÓNG BÀN
                hoaDon.Tongtien = tongTien;
                hoaDon.Trangthai = true;
                hoaDon.Idpttt = phuongThucThanhToan;
                hoaDon.Ngaylap = DateTime.Now;  // ← Cập nhật thời gian thanh toán
                phien.Gioketthuc = DateTime.Now;
                ban.Trangthai = false;

                _context.SaveChanges();
                Console.WriteLine("✅ Đã SaveChanges");

                // ✅ SAU KHI SAVE, LOAD LẠI ĐỂ CÓ NAVIGATION PROPERTIES
                hoaDon = _context.Hoadons
                    .Include(h => h.IdphienNavigation)
                        .ThenInclude(p => p.IdbanNavigation)
                    .Include(h => h.Hoadondvs)
                        .ThenInclude(hd => hd.IddvNavigation)
                    .Include(h => h.IdnvNavigation)      // ← QUAN TRỌNG: Load nhân viên
                    .Include(h => h.IdkhNavigation)      // ← QUAN TRỌNG: Load khách hàng
                    .FirstOrDefault(h => h.Idhd == idHoaDon);

                Console.WriteLine($"✅ Reload hóa đơn - IdNV: {hoaDon.Idnv}, TenNV: {hoaDon.IdnvNavigation?.Hotennv}");

                // ← Tạo PDF TẠM để preview
                string pdfUrlTemp = _pdfService.TaoHoaDonPdfTemp(hoaDon, ban);

                return Json(new
                {
                    success = true,
                    message = "Thanh toán thành công!",
                    tongTien = tongTien,
                    pdfUrl = pdfUrlTemp,
                    idHoaDon = idHoaDon
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi ThanhToan: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ThuNgan/XacNhanIn - Lưu PDF chính thức
        [HttpPost]
        public IActionResult XacNhanIn(string idHoaDon)
        {
            try
            {
                if (string.IsNullOrEmpty(idHoaDon))
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                // Lưu PDF chính thức (copy từ temp sang invoices)
                string pdfUrl = _pdfService.LuuHoaDonPdfChinhThuc(idHoaDon);

                return Json(new
                {
                    success = true,
                    message = "Đã lưu hóa đơn PDF",
                    pdfUrl = pdfUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ThuNgan/HuyIn - Xóa PDF tạm (không lưu)
        [HttpPost]
        public IActionResult HuyIn(string idHoaDon)
        {
            try
            {
                if (string.IsNullOrEmpty(idHoaDon))
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });

                // Xóa PDF tạm
                _pdfService.XoaPdfTemp(idHoaDon);

                return Json(new
                {
                    success = true,
                    message = "Đã hủy lưu PDF"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        // ===========================
        // TÌM KIẾM KHÁCH HÀNG
        // ===========================

        [HttpGet]
        public IActionResult SearchKhachHang(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Json(new { success = false, message = "Vui lòng nhập từ khóa" });
                }

                // Tìm kiếm theo SĐT HOẶC Họ tên
                var khachHangs = _context.Khachhangs
                    .Where(kh =>
                        (kh.Sodt != null && kh.Sodt.Contains(keyword)) ||
                        (kh.Hoten != null && kh.Hoten.Contains(keyword))
                    )
                    .OrderBy(kh => kh.Hoten)
                    .Take(10)
                    .Select(kh => new
                    {
                        idKh = kh.Idkh,
                        tenKh = kh.Hoten ?? "Khách hàng",
                        sdt = kh.Sodt ?? "",
                        soLuotMua = kh.Hoadons.Count()
                    })
                    .ToList();

                return Json(new { success = true, data = khachHangs });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        // Gán khách hàng vào hóa đơn
        [HttpPost]
        public IActionResult GanKhachHang(string idHoaDon, string idKhachHang)
        {
            try
            {
                // Tìm hóa đơn
                var hoaDon = _context.Hoadons.Find(idHoaDon);
                if (hoaDon == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
                }

                // Kiểm tra khách hàng tồn tại
                var khachHang = _context.Khachhangs.Find(idKhachHang);
                if (khachHang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng" });
                }

                // Gán khách hàng vào hóa đơn
                hoaDon.Idkh = idKhachHang;
                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Đã gán khách hàng",
                    khachHang = new
                    {
                        idKh = khachHang.Idkh,
                        tenKh = khachHang.Hoten, // ← Đổi từ Tenkh thành Hoten
                        sdt = khachHang.Sodt     // ← Đổi từ Sdt thành Sodt
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // Thêm khách hàng mới nhanh
        [HttpPost]
        public IActionResult ThemKhachHangNhanh(string tenKh, string sdt, string diaChi)
        {
            try
            {
                // Kiểm tra SĐT đã tồn tại
                var exists = _context.Khachhangs.Any(kh => kh.Sodt == sdt);
                if (exists)
                {
                    return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });
                }

                // Tạo ID mới
                var maxId = _context.Khachhangs
                    .Select(kh => kh.Idkh)
                    .ToList()
                    .Select(id => int.Parse(id.Substring(2)))
                    .DefaultIfEmpty(0)
                    .Max();

                var newId = $"KH{(maxId + 1):D3}";

                // Tạo khách hàng mới
                var khachHang = new Khachhang
                {
                    Idkh = newId,
                    Hoten = tenKh,
                    Sodt = sdt,
                    Dchi = diaChi
                };

                _context.Khachhangs.Add(khachHang);
                _context.SaveChanges();

                // ✅ THÊM: Trả về thông tin khách hàng
                return Json(new
                {
                    success = true,
                    message = "Đã thêm khách hàng thành công!",
                    khachHang = new
                    {
                        idKh = khachHang.Idkh,
                        tenKh = khachHang.Hoten,
                        sdt = khachHang.Sodt
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
