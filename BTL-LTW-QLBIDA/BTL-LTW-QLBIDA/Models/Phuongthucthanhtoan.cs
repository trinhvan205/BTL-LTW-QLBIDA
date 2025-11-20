using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BTL_LTW_QLBIDA.Models
{
    public partial class Phuongthucthanhtoan
    {
        public Phuongthucthanhtoan()
        {
            Hoadons = new HashSet<Hoadon>();
        }

        [Key]
        public string Idpttt { get; set; } = null!;

        public string Tenpttt { get; set; } = null!;

        public bool? Hienthi { get; set; }

        // Navigation property
        public virtual ICollection<Hoadon> Hoadons { get; set; }
    }
}