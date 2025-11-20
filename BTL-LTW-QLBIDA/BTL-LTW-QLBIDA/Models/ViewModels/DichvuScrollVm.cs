using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models.ViewModels
{
    public class DichvuScrollVm
    {
        public List<Dichvu> Items { get; set; } = [];
        public bool HasMore { get; set; }
    }
}
