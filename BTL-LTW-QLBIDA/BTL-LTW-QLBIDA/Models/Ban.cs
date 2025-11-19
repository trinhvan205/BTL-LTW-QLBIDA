using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Ban
{
    public string Idban { get; set; } = null!;

    public string? Idkhu { get; set; }

    public decimal? Giatien { get; set; }

    public bool? Trangthai { get; set; }
    public string? Ghichu { get; set; }

    public virtual Khuvuc? IdkhuNavigation { get; set; }


    public virtual ICollection<Phienchoi> Phienchois { get; set; } = new List<Phienchoi>();
}
