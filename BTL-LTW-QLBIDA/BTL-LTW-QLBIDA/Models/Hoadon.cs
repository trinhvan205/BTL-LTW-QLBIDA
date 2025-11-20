using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Hoadon
{
    public string Idhd { get; set; } = null!;

    public string? Idphien { get; set; }

    public string? Idkh { get; set; }

    public string? Idnv { get; set; }

    public DateTime? Ngaylap { get; set; }

    public decimal? Tongtien { get; set; }

    public bool? Trangthai { get; set; }

    // ← THÊM MỚI
    public string? Idpttt { get; set; }

    // Navigation properties
    public virtual ICollection<Hoadondv> Hoadondvs { get; set; } = [];
    public virtual Khachhang? IdkhNavigation { get; set; }
    public virtual Nhanvien? IdnvNavigation { get; set; }
    public virtual Phienchoi? IdphienNavigation { get; set; }

    // ← THÊM MỚI
    public virtual Phuongthucthanhtoan? IdptttNavigation { get; set; }
}