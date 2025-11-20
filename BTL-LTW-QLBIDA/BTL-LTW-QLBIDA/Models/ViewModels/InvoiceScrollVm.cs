using System.Collections.Generic;
using BTL_LTW_QLBIDA.Models;

namespace BTL_LTW_QLBIDA.Models.ViewModels
{
    public class InvoiceScrollVm
    {
        public List<Hoadon> Items { get; set; } = [];
        public bool HasMore { get; set; }
    }
}
