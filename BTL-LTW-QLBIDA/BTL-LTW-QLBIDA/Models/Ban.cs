using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_QLBIDA.Models;

public partial class Ban
{
    // Idban có lẽ đóng vai trò là Tên Bàn.
    [Required(ErrorMessage = "Vui lòng nhập Tên bàn.")]
    [DisplayName("Tên bàn")]
    public string Idban { get; set; } = null!;

    // Idkhu (ID Khu vực)
    [Required(ErrorMessage = "Vui lòng chọn Khu vực.")]
    [DisplayName("Khu vực")]
    public string? Idkhu { get; set; }

    // Giatien
    

    // 🟢 ĐÃ SỬA: Cho phép Giá tiền là số thực lớn hơn hoặc bằng 0
    [DataType(DataType.Currency, ErrorMessage = "Giá tiền phải là một con số hợp lệ.")]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá tiền phải là một con số hợp lệ và lớn hơn hoặc bằng 0.")]
    [DisplayName("Giá tiền")]
    [Required(ErrorMessage = "Vui lòng nhập Giá tiền.")]
    public decimal? Giatien { get; set; }

    // Trangthai và Ghichu không bắt buộc (Optional)
    public bool? Trangthai { get; set; }
    public string? Ghichu { get; set; }

    // Navigation properties
    public virtual Khuvuc? IdkhuNavigation { get; set; }
    public virtual ICollection<Phienchoi> Phienchois { get; set; } = new List<Phienchoi>();
}