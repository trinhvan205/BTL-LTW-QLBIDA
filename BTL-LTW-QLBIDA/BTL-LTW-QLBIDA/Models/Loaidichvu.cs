using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Loaidichvu
{
    public string Idloai { get; set; } = null!;

    public string? Tenloai { get; set; }

    public virtual ICollection<Dichvu> Dichvus { get; set; } = [];
}
