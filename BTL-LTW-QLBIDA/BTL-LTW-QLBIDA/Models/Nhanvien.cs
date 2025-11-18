using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models;

public partial class Nhanvien
{
    public string Idnv { get; set; } = null!;

    public string? Hotennv { get; set; }

    public DateTime? Ngaysinh { get; set; }

    public bool? Gioitinh { get; set; }

    public string? Cccd { get; set; }

    public string? Sodt { get; set; }

    public string? Matkhau { get; set; }

    public bool? Quyenadmin { get; set; }

    public string? Tendangnhap { get; set; }

    public bool? Hienthi { get; set; }

    public bool Nghiviec { get; set; }

    public virtual ICollection<Hoadon> Hoadons { get; set; } = new List<Hoadon>();
}
