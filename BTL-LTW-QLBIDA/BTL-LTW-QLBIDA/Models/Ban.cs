using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_QLBIDA.Models;

public partial class Ban
{
    // Idban có lẽ đóng vai trò là Tên Bàn.
    // Thêm [Required] để bắt buộc nhập.
    [Required(ErrorMessage = "Vui lòng nhập Tên bàn.")]
    [DisplayName("Tên bàn")] // Dùng cho label trong View
    public string Idban { get; set; } = null!;

    // Idkhu (ID Khu vực)
    // Thêm [Required] để bắt buộc chọn Khu vực.
    [Required(ErrorMessage = "Vui lòng chọn Khu vực.")]
    [DisplayName("Khu vực")]
    public string? Idkhu { get; set; }

    // Giatien
    // Thêm [Required] và [Range] để đảm bảo là số dương hợp lệ.
    [Required(ErrorMessage = "Vui lòng nhập Giá tiền.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Giá tiền phải là một con số hợp lệ và lớn hơn 0.")]
    [DisplayName("Giá tiền")]
    public decimal? Giatien { get; set; }

    // Trangthai và Ghichu không bắt buộc (Optional)
    public bool? Trangthai { get; set; }
    public string? Ghichu { get; set; }

    // Navigation properties
    public virtual Khuvuc? IdkhuNavigation { get; set; }
    public virtual ICollection<Phienchoi> Phienchois { get; set; } = new List<Phienchoi>();
}
