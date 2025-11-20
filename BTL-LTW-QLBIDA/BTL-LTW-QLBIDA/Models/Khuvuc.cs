using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Khuvuc
{
    public string Idkhu { get; set; } = null!;
    public string? Tenkhu { get; set; }

    // === THÊM DÒNG NÀY VÀO ===
    public string? Ghichu { get; set; }

    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();
}
