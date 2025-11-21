using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_QLBIDA.ViewModels;

public partial class KhachHangVM
{
    public string Idkh { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    public string? Hoten { get; set; }

    public string? Dchi { get; set; }

    [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$",
        ErrorMessage = "Số điện thoại không đúng định dạng (VD: 0901234567)")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải đúng 10 số")]
    [Display(Name = "Số điện thoại")]
    public string? Sodt { get; set; }

}
