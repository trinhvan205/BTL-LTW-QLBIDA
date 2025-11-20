using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Dichvu
{
    public string Iddv { get; set; } = null!;

    public string? Tendv { get; set; }

    public string? Idloai { get; set; }

    public decimal? Giatien { get; set; }

    public int? Soluong { get; set; }

    public bool? Hienthi { get; set; }
    public string? Imgpath { get; set; }
    public virtual ICollection<Hoadondv> Hoadondvs { get; set; } = [];

    public virtual Loaidichvu? IdloaiNavigation { get; set; }
}
