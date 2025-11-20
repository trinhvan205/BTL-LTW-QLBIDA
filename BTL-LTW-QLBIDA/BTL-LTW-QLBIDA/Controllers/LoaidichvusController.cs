using BTL_LTW_QLBIDA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Controllers
{
    public class LoaidichvusController(QlquanBilliardLtwContext context) : Controller
    {
        private readonly QlquanBilliardLtwContext _context = context;

        // =====================================================
        // HELPER: Generate Auto ID (LDVxxx)
        // =====================================================
        private async Task<string> GenerateNextIdLoaiAsync()
        {
            var last = await _context.Loaidichvus
                .OrderByDescending(l => l.Idloai)
                .Select(l => l.Idloai)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(last))
                return "LDV001";

            // Extract numbers from "LDV001" -> "001"
            string digits = new([.. last.Where(char.IsDigit)]);
            int number = int.TryParse(digits, out int num) ? num : 0;

            return $"LDV{(number + 1):D3}";
        }

        // =====================================================
        // AJAX POST: Create New Category
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> CreateAjax(string tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai))
                return Json(new { success = false, message = "Tên loại không được để trống!" });

            // 1. Check for duplicates
            bool exists = await _context.Loaidichvus.AnyAsync(l => l.Tenloai.Equals(tenLoai, StringComparison.CurrentCultureIgnoreCase));
            if (exists)
                return Json(new { success = false, message = "Tên loại dịch vụ này đã tồn tại!" });

            try
            {
                // 2. Create Object
                var newLoai = new Loaidichvu
                {
                    Idloai = await GenerateNextIdLoaiAsync(),
                    Tenloai = tenLoai
                };

                // 3. Save to DB
                _context.Add(newLoai);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm loại dịch vụ thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
    }
}