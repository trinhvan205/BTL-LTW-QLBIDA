using BTL_LTW_QLBIDA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Controllers
{
    public class LoaidichvusController(QlquanBilliardLtw2Context context) : Controller
    {
        private readonly QlquanBilliardLtw2Context _context = context;

        // =====================================================
        // AUTO ID: LDV001, LDV002, ...
        // =====================================================
        private async Task<string> GenerateNextIdLoaiAsync()
        {
            var last = await _context.Loaidichvus
                .OrderByDescending(l => l.Idloai)
                .Select(l => l.Idloai)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(last))
                return "LDV001";

            string digits = new([.. last.Where(char.IsDigit)]);
            int number = int.TryParse(digits, out int num) ? num : 0;

            return $"LDV{(number + 1):D3}";
        }

        // =====================================================
        // AJAX: CREATE CATEGORY
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromForm] string tenLoai)
        {
            if (string.IsNullOrWhiteSpace(tenLoai))
                return Json(new { success = false, message = "Tên loại không được để trống!" });

            // ❗ FIX LỖI StringComparison
            bool exists = await _context.Loaidichvus
                .AnyAsync(l => l.Tenloai.ToLower() == tenLoai.ToLower());

            if (exists)
                return Json(new { success = false, message = "Tên loại dịch vụ này đã tồn tại!" });

            try
            {
                var newLoai = new Loaidichvu
                {
                    Idloai = await GenerateNextIdLoaiAsync(),
                    Tenloai = tenLoai
                };

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
