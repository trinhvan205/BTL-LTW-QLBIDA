using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BTL_LTW_QLBIDA.Models
{
    public partial class Loaidichvu
    {
        [DisplayName("Mã loại dịch vụ")]
        [Required(ErrorMessage = "Vui lòng nhập mã loại dịch vụ.")]
        [StringLength(50, ErrorMessage = "Mã loại dịch vụ không được vượt quá 50 ký tự.")]
        public string Idloai { get; set; } = null!;


        [DisplayName("Tên loại dịch vụ")]
        [Required(ErrorMessage = "Tên loại dịch vụ không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên loại dịch vụ tối đa 100 ký tự.")]
        public string? Tenloai { get; set; }


        // Navigation
        public virtual ICollection<Dichvu> Dichvus { get; set; } = new List<Dichvu>();
    }
}
