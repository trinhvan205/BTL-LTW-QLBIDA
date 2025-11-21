using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_QLBIDA.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = null!;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool GhiNho { get; set; }

        // Loại chức năng được chọn: "banhang" hoặc "quantri"
        public string ChucNang { get; set; } = "ThuNgan";
    }
}