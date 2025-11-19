using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models
{
    public class PhongBanIndexViewModel
    {
        // Thêm ? để cho phép null
        public PagedResult<Ban>? PagedBans { get; set; }

        // Thêm ? để cho phép null
        public SelectList? KhuVucs { get; set; }

        // Thêm ? để cho phép null
        public string? SelectedKhuVuc { get; set; }

        // Thêm ? cho an toàn
        public string? SearchString { get; set; }

        // Cái này đã là nullable (bool?) nên không cần sửa
        public bool? SelectedTrangThai { get; set; }
    }
}