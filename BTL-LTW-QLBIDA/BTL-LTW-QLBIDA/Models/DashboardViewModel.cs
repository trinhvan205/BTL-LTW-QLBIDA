using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models
{
    public class DashboardViewModel
    {
        public int TongBan { get; set; }
        public double TrungBinhDoanhThu { get; set; }
        public double TrungBinhKhach { get; set; }
        public List<HoatDongViewModel> HoatDongGanDay { get; set; }
        public List<SanPhamBanChayViewModel> Top10SanPham { get; set; }
    }

    public class HoatDongViewModel
    {
        public string TenKhachHang { get; set; }
        public string LoaiHoatDong { get; set; }
        public decimal SoTien { get; set; }
        public DateTime ThoiGian { get; set; }
    }

    public class SanPhamBanChayViewModel
    {
        public string TenSanPham { get; set; }
        public int SoLuongBan { get; set; }
    }

    public class ChartData
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
    }
}