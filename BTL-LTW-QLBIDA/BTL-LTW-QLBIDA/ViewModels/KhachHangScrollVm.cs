using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models.ViewModels
{
    public class KhachHangScrollVm
    {
        public IEnumerable<Khachhang> Items { get; set; }
        public bool HasMore { get; set; }
    }
}