using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Phienchoi
{
    public string Idphien { get; set; } = null!;

    public string? Idban { get; set; }

    public DateTime? Giobatdau { get; set; }

    public DateTime? Gioketthuc { get; set; }

    public virtual ICollection<Hoadon> Hoadons { get; set; } = [];

    public virtual Ban? IdbanNavigation { get; set; }
}
