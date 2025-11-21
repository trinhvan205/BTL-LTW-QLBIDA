using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models.ViewModels
{
    public class NhanVienScrollVm
    {
        public IEnumerable<Nhanvien> Items { get; set; }
        public bool HasMore { get; set; }
    }
}