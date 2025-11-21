using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BTL_LTW_QLBIDA.Models
{
    public partial class Dichvu
    {
        [DisplayName("Mã dịch vụ")]
        [Required(ErrorMessage = "Vui lòng nhập mã dịch vụ.")]
        [StringLength(50, ErrorMessage = "Mã dịch vụ không được vượt quá 50 ký tự.")]
        public string Iddv { get; set; } = null!;


        [DisplayName("Tên dịch vụ")]
        [Required(ErrorMessage = "Tên dịch vụ không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ tối đa 100 ký tự.")]
        public string? Tendv { get; set; }


        [DisplayName("Loại dịch vụ")]
        [Required(ErrorMessage = "Vui lòng chọn loại dịch vụ.")]
        public string? Idloai { get; set; }


        [DisplayName("Giá tiền")]
        [Required(ErrorMessage = "Giá tiền không được để trống.")]
        [Range(1, 1000000000, ErrorMessage = "Giá tiền phải lớn hơn 0.")]
        public decimal? Giatien { get; set; }


        [DisplayName("Số lượng tồn kho")]
        [Required(ErrorMessage = "Số lượng không được để trống.")]
        [Range(0, 1000000, ErrorMessage = "Số lượng phải từ 0 đến 1,000,000.")]
        public int? Soluong { get; set; }


        [DisplayName("Hiển thị")]
        public bool? Hienthi { get; set; }


        [DisplayName("Đường dẫn ảnh")]
        [StringLength(255, ErrorMessage = "Đường dẫn ảnh tối đa 255 ký tự.")]
        public string? Imgpath { get; set; }


        // Relationships
        public virtual ICollection<Hoadondv> Hoadondvs { get; set; } = new List<Hoadondv>();

        public virtual Loaidichvu? IdloaiNavigation { get; set; }
    }
}
