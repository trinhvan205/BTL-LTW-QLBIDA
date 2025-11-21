using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models.ViewModels
{
    public class BanScrollVm
    {
        public IEnumerable<Ban> Items { get; set; }
        public bool HasMore { get; set; }
    }
}