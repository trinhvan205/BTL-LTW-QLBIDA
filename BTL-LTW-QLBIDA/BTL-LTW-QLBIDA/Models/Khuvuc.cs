using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Khuvuc
{
    public string Idkhu { get; set; } = null!;

    public string? Tenkhu { get; set; }

    public virtual ICollection<Ban> Bans { get; set; } = [];
}
