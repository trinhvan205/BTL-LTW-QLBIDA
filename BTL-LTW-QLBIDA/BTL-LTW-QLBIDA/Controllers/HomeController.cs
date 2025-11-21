using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private readonly QlquanBilliardLtw2Context db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(QlquanBilliardLtw2Context context, ILogger<HomeController> logger)
        {
            db = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // 1. Tính toán số liệu mặc định (7 ngày qua)
            var sevenDaysAgo = DateTime.Today.AddDays(-6);
            var query = db.Hoadons.Where(h => h.Trangthai == true && h.Ngaylap >= sevenDaysAgo);

            // Nhóm theo ngày để tính tổng từng ngày trước khi chia trung bình
            var dataByDay = query
                            .AsEnumerable() // Chuyển về xử lý trên RAM để GroupBy Date chuẩn hơn
                            .GroupBy(h => h.Ngaylap.Value.Date)
                            .Select(g => new
                            {
                                Revenue = g.Sum(h => h.Tongtien ?? 0),
                                Count = g.Count()
                            })
                            .ToList();

            double avgRev = 0;
            double avgCust = 0;

            // Chia cho 7 ngày (để ra năng suất thực tế của tuần)
            if (dataByDay.Count > 0)
            {
                avgRev = dataByDay.Sum(x => (double)x.Revenue) / 7.0;
                avgCust = dataByDay.Sum(x => (double)x.Count) / 7.0;
            }

            // 2. Đổ dữ liệu vào Model
            var viewModel = new DashboardViewModel
            {
                TongBan = db.Bans.Count(),

                // Gán giá trị mới tính
                TrungBinhDoanhThu = avgRev,
                TrungBinhKhach = avgCust,

                // ... (Giữ nguyên phần HoatDongGanDay và Top10SanPham cũ) ...
                HoatDongGanDay = db.Hoadons
                    .Include(h => h.IdkhNavigation)
                    .OrderByDescending(hd => hd.Ngaylap)
                    .Take(10)
                    .Select(hd => new HoatDongViewModel
                    {
                        TenKhachHang = hd.IdkhNavigation != null ? hd.IdkhNavigation.Hoten : "Khách lẻ",
                        LoaiHoatDong = hd.Trangthai == true ? "Đã thanh toán" : "Đang chơi",
                        SoTien = hd.Tongtien ?? 0,
                        ThoiGian = hd.Ngaylap ?? DateTime.Now
                    })
                    .ToList(),

                Top10SanPham = db.Hoadondvs
                    .Include(d => d.IddvNavigation)
                    .GroupBy(hdv => hdv.IddvNavigation.Tendv)
                    .Select(g => new SanPhamBanChayViewModel
                    {
                        TenSanPham = g.Key,
                        SoLuongBan = g.Sum(x => x.Soluong) ?? 0
                    })
                    .OrderByDescending(x => x.SoLuongBan)
                    .Take(10)
                    .ToList()
            };

            return View(viewModel);
        }


        // ============================================================
        // CẬP NHẬT LOGIC API THỐNG KÊ (NGÀY - THÁNG - NĂM)
        // ============================================================
        [HttpGet]
        public JsonResult GetDoanhThuData(string type)
        {
            // Lấy các hóa đơn đã thanh toán và có ngày lập
            var query = db.Hoadons.Where(h => h.Trangthai == true && h.Ngaylap.HasValue);
            List<ChartData> result = new List<ChartData>();

            if (type == "year")
            {
                // --- THỐNG KÊ THEO NĂM ---
                // Group theo năm của NgayLap
                var data = query
                                .AsEnumerable()
                                .GroupBy(h => h.Ngaylap.Value.Year)
                                .Select(g => new ChartData
                                {
                                    Label = "Năm " + g.Key,
                                    Value = g.Sum(h => h.Tongtien ?? 0)
                                })
                                .OrderBy(x => x.Label)
                                .ToList();
                result = data;
            }
            else if (type == "month")
            {
                // --- THỐNG KÊ THEO THÁNG (Của năm hiện tại) ---
                var currentYear = DateTime.Now.Year;
                var data = query.Where(h => h.Ngaylap.Value.Year == currentYear)
                                .AsEnumerable()
                                .GroupBy(h => h.Ngaylap.Value.Month)
                                .Select(g => new ChartData
                                {
                                    Label = "Tháng " + g.Key,
                                    Value = g.Sum(h => h.Tongtien ?? 0)
                                })
                                .OrderBy(x => int.Parse(x.Label.Replace("Tháng ", "")))
                                .ToList();
                result = data;
            }
            else
            {
                // --- THỐNG KÊ THEO NGÀY (Mặc định: 7 ngày gần nhất) ---
                var sevenDaysAgo = DateTime.Today.AddDays(-6);
                var data = query.Where(h => h.Ngaylap >= sevenDaysAgo)
                                .AsEnumerable()
                                .GroupBy(h => h.Ngaylap.Value.Date)
                                .Select(g => new ChartData
                                {
                                    Label = g.Key.ToString("dd/MM"),
                                    Value = g.Sum(h => h.Tongtien ?? 0)
                                })
                                .OrderBy(x => DateTime.ParseExact(x.Label, "dd/MM", CultureInfo.InvariantCulture))
                                .ToList();
                result = data;
            }

            return Json(result);
        }
        // Thêm hàm này vào bên dưới hàm GetDoanhThuData trong HomeController.cs

        [HttpGet]
        public JsonResult GetLuuLuongKhachData(string type)
        {
            // Lấy các hóa đơn đã thanh toán (tương ứng với 1 lượt khách/nhóm khách)
            var query = db.Hoadons.Where(h => h.Trangthai == true && h.Ngaylap.HasValue);
            List<ChartData> result = new List<ChartData>();

            if (type == "year")
            {
                // --- ĐẾM KHÁCH THEO NĂM ---
                var data = query
                                .AsEnumerable()
                                .GroupBy(h => h.Ngaylap.Value.Year)
                                .Select(g => new ChartData
                                {
                                    Label = "Năm " + g.Key,
                                    Value = g.Count() // Đếm số lượng hóa đơn
                                })
                                .OrderBy(x => x.Label)
                                .ToList();
                result = data;
            }
            else if (type == "month")
            {
                // --- ĐẾM KHÁCH THEO THÁNG (Của năm hiện tại) ---
                var currentYear = DateTime.Now.Year;
                var data = query.Where(h => h.Ngaylap.Value.Year == currentYear)
                                .AsEnumerable()
                                .GroupBy(h => h.Ngaylap.Value.Month)
                                .Select(g => new ChartData
                                {
                                    Label = "Tháng " + g.Key,
                                    Value = g.Count()
                                })
                                .OrderBy(x => int.Parse(x.Label.Replace("Tháng ", "")))
                                .ToList();
                result = data;
            }
            else
            {
                // --- ĐẾM KHÁCH THEO NGÀY (7 ngày gần nhất) ---
                var sevenDaysAgo = DateTime.Today.AddDays(-6);
                var data = query.Where(h => h.Ngaylap >= sevenDaysAgo)
                                .AsEnumerable()
                                .GroupBy(h => h.Ngaylap.Value.Date)
                                .Select(g => new ChartData
                                {
                                    Label = g.Key.ToString("dd/MM"),
                                    Value = g.Count()
                                })
                                .OrderBy(x => DateTime.ParseExact(x.Label, "dd/MM", CultureInfo.InvariantCulture))
                                .ToList();
                result = data;
            }

            return Json(result);
        }
        // Thêm vào bên trong class HomeController

        [HttpGet]
        public JsonResult GetGeneralStats(string type)
        {
            // 1. Tổng số bàn (Cố định hoặc đếm số bàn từng được sử dụng nếu muốn, ở đây giữ cố định số lượng bàn của quán)
            int tongSoBan = db.Bans.Count();

            // Chuẩn bị query hóa đơn đã thanh toán
            var query = db.Hoadons.Where(h => h.Trangthai == true && h.Ngaylap.HasValue);

            double trungBinhDoanhThu = 0;
            double trungBinhKhach = 0;

            if (type == "year")
            {
                // --- Theo Năm ---
                // Tính trung bình theo từng năm có dữ liệu
                var dataByYear = query.GroupBy(h => h.Ngaylap.Value.Year)
                                      .Select(g => new {
                                          Year = g.Key,
                                          TotalRev = g.Sum(h => h.Tongtien ?? 0),
                                          TotalCust = g.Count()
                                      }).ToList();

                if (dataByYear.Count > 0)
                {
                    trungBinhDoanhThu = dataByYear.Average(x => (double)x.TotalRev);
                    trungBinhKhach = dataByYear.Average(x => x.TotalCust);
                }
            }
            else if (type == "month")
            {
                // --- Theo Tháng (Trong năm nay) ---
                var currentYear = DateTime.Now.Year;
                var dataByMonth = query.Where(h => h.Ngaylap.Value.Year == currentYear)
                                       .GroupBy(h => h.Ngaylap.Value.Month)
                                       .Select(g => new {
                                           Month = g.Key,
                                           TotalRev = g.Sum(h => h.Tongtien ?? 0),
                                           TotalCust = g.Count()
                                       }).ToList();

                // Chia trung bình cho số tháng đã có dữ liệu (hoặc chia cho 12 nếu muốn chia đều cả năm)
                if (dataByMonth.Count > 0)
                {
                    trungBinhDoanhThu = dataByMonth.Average(x => (double)x.TotalRev);
                    trungBinhKhach = dataByMonth.Average(x => x.TotalCust);
                }
            }
            else
            {
                // --- Theo Ngày (7 ngày gần nhất) ---
                var sevenDaysAgo = DateTime.Today.AddDays(-6);
                var dataByDay = query.Where(h => h.Ngaylap >= sevenDaysAgo)
                                     .GroupBy(h => h.Ngaylap.Value.Date)
                                     .Select(g => new {
                                         Date = g.Key,
                                         TotalRev = g.Sum(h => h.Tongtien ?? 0),
                                         TotalCust = g.Count()
                                     }).ToList();

                // Chia cho 7 ngày (để thấy rõ năng suất trung bình tuần)
                // Hoặc chia cho dataByDay.Count nếu chỉ muốn tính trên ngày có khách
                trungBinhDoanhThu = dataByDay.Sum(x => (double)x.TotalRev) / 7.0;
                trungBinhKhach = dataByDay.Sum(x => (double)x.TotalCust) / 7.0;
            }

            return Json(new
            {
                tongBan = tongSoBan,
                avgDoanhThu = trungBinhDoanhThu,
                avgKhach = trungBinhKhach
            });
        }
    }

}