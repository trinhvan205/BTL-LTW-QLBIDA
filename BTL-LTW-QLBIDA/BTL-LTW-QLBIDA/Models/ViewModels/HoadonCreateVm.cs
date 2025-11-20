using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_QLBIDA.Models.ViewModels
{
    // 1 dòng dịch vụ trong hóa đơn
    public class HoadonDvItemVm
    {
        [Required]
        public string Iddv { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải >= 1")]
        public int Soluong { get; set; } = 1;
    }

    // ViewModel dùng cho form tạo hóa đơn
    public class HoadonCreateVm
    {
        [Display(Name = "Khách hàng")]
        public string? Idkh { get; set; }

        [Display(Name = "Nhân viên")]
        public string? Idnv { get; set; }

        [Display(Name = "Phiên chơi")]
        public string? Idph { get; set; }

        // [MỚI] Thêm trường Phương thức thanh toán
        [Display(Name = "Phương thức thanh toán")]
        public string? Idpttt { get; set; }

        public List<HoadonDvItemVm> Items { get; set; } = [];
    }
}