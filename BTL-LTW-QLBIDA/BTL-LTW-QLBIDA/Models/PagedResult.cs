using System;
using System.Collections.Generic;

namespace BTL_LTW_QLBIDA.Models
{
    // Lớp này dùng chung, có thể chứa Ban, KhachHang, hoặc bất cứ thứ gì
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        // Tự động tính tổng số trang
        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        // Các thuộc tính helper
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResult()
        {
            Items = new List<T>();
        }
    }
}