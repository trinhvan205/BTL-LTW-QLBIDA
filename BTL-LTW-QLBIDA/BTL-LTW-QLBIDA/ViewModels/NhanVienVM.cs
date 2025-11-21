using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_QLBIDA.ViewModels;

public partial class NhanVienVM
{
    public string Idnv { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [StringLength(50, ErrorMessage = "Họ tên không được vượt quá 50 ký tự")]
    [Display(Name = "Họ và tên")]
    public string? Hotennv { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày sinh")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime? Ngaysinh { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn giới tính")]
    [Display(Name = "Giới tính")]
    public bool? Gioitinh { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số CCCD")]
    [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "CCCD phải đúng 12 chữ số")]
    [StringLength(12, MinimumLength = 12, ErrorMessage = "CCCD phải đúng 12 chữ số")]
    [Display(Name = "Số CCCD")]
    public string? Cccd { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$",
        ErrorMessage = "Số điện thoại không đúng định dạng (VD: 0901234567)")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải đúng 10 số")]
    [Display(Name = "Số điện thoại")]
    public string? Sodt { get; set; }

    [Display(Name = "Mật khẩu")]
    public string? Matkhau { get; set; }

    [Display(Name = "Quyền quản trị")]
    public bool? Quyenadmin { get; set; }

    [Display(Name = "Tên đăng nhập")]
    public string? Tendangnhap { get; set; }

    [Display(Name = "Hiển thị")]
    public bool? Hienthi { get; set; }

    [Display(Name = "Nghỉ việc")]
    public bool Nghiviec { get; set; }
}