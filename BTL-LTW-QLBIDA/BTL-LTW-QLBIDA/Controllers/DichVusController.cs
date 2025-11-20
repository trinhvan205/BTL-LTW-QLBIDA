using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace BTL_LTW_QLBIDA.Controllers
{
    public class DichvusController(QlquanBilliardLtwContext context, IWebHostEnvironment env) : Controller
    {
        private readonly QlquanBilliardLtwContext _context = context;
        private readonly IWebHostEnvironment _env = env;

        // =====================================================
        // INDEX – Hiển thị giao diện (AJAX sẽ load bảng)
        // =====================================================
        public async Task<IActionResult> Index()
        {
            ViewBag.ListLoai = await _context.Loaidichvus
                .OrderBy(l => l.Tenloai)
                .ToListAsync();

            return View();
        }

        // =====================================================
        // SINH MÃ TỰ ĐỘNG DV
        // =====================================================
        private async Task<string> GenerateNextIddvAsync()
        {
            var last = await _context.Dichvus
                .OrderByDescending(d => d.Iddv)
                .Select(d => d.Iddv)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(last))
                return "DV001";

            string digits = new([.. last.Where(char.IsDigit)]);
            int number = int.TryParse(digits, out int num) ? num : 0;

            return $"DV{(number + 1):D3}";
        }

        [HttpGet]
        public async Task<IActionResult> GetNextId()
        {
            string nextId = await GenerateNextIddvAsync();
            return Json(nextId);
        }

        // =====================================================
        // LOAD TABLE + FILTER + INFINITE SCROLL (AJAX)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> LoadTable(
            string? keyword,
            string? loaiId,
            bool? status,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            const int pageSize = 10;

            var query = _context.Dichvus
                .Include(d => d.IdloaiNavigation)
                .OrderBy(d => d.Tendv)
                .AsQueryable();

            // Bộ lọc
            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(d => d.Tendv.Contains(keyword));

            if (!string.IsNullOrWhiteSpace(loaiId))
                query = query.Where(d => d.Idloai == loaiId);

            if (status.HasValue)
                query = query.Where(d => d.Hienthi == status.Value);

            if (minPrice.HasValue)
                query = query.Where(d => d.Giatien >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(d => d.Giatien <= maxPrice.Value);

            // Phân trang
            int total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            bool hasMore = total > page * pageSize;

            return PartialView("_DichvuRows", new DichvuScrollVm
            {
                Items = items,
                HasMore = hasMore
            });
        }

        // =====================================================
        // XỬ LÝ ẢNH
        // =====================================================
        private string? SaveImage(string id, IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();
            string[] allowExt = [".jpg", ".jpeg", ".png", ".webp"];
            if (!allowExt.Contains(ext)) return null;

            string folder = Path.Combine(_env.WebRootPath, "images/dichvu");
            Directory.CreateDirectory(folder);

            string newName = $"{id}_{Guid.NewGuid()}{ext}";
            string path = Path.Combine(folder, newName);

            using var stream = new FileStream(path, FileMode.Create);
            file.CopyTo(stream);

            return "/images/dichvu/" + newName;
        }

        private void DeleteOldImage(string? imgPath)
        {
            if (string.IsNullOrEmpty(imgPath)) return;

            string full = Path.Combine(_env.WebRootPath, imgPath.TrimStart('/'));
            if (System.IO.File.Exists(full))
                System.IO.File.Delete(full);
        }


        // =====================================================
        // CREATE AJAX
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> CreateAjax(Dichvu dv, IFormFile? imageFile)
        {
            dv.Iddv = await GenerateNextIddvAsync();

            if (string.IsNullOrWhiteSpace(dv.Tendv))
                return Json(new { success = false, message = "Tên dịch vụ không được để trống!" });
            if (string.IsNullOrEmpty(dv.Idloai))
                return Json(new { success = false, message = "Vui lòng chọn loại dịch vụ!" });
            if (dv.Giatien <= 0)
                return Json(new { success = false, message = "Giá bán phải lớn hơn 0!" });
            if (dv.Soluong < 0)
                return Json(new { success = false, message = "Số lượng không hợp lệ!" });

            if (imageFile != null)
            {
                string? img = SaveImage(dv.Iddv, imageFile);
                if (img == null)
                    return Json(new { success = false, message = "Ảnh không hợp lệ!" });
                dv.Imgpath = img;
            }
            else
                dv.Imgpath = "/images/no-image.png";

            _context.Add(dv);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =====================================================
        // DETAIL PARTIAL
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> DetailPartial(string id)
        {
            var dv = await _context.Dichvus
                .Include(d => d.IdloaiNavigation)
                .FirstOrDefaultAsync(d => d.Iddv == id);

            if (dv == null)
                return Content("<p class='text-danger p-3'>Không tìm thấy dịch vụ!</p>");

            return PartialView("_DichvuDetailModal", dv);
        }


        // =====================================================
        // EDIT FORM (MODAL)
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetEditForm(string id)
        {
            var dv = await _context.Dichvus.FindAsync(id);
            if (dv == null) return NotFound();

            ViewBag.ListLoai = await _context.Loaidichvus.OrderBy(l => l.Tenloai).ToListAsync();

            return PartialView("_DichvuEditPartial", dv);
        }

        // =====================================================
        // UPDATE AJAX
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> UpdateAjax(Dichvu dv, IFormFile? imageFile)
        {
            var old = await _context.Dichvus.FindAsync(dv.Iddv);
            if (old == null)
                return Json(new { success = false, message = "Không tìm thấy dịch vụ!" });

            old.Tendv = dv.Tendv;
            old.Idloai = dv.Idloai;
            old.Giatien = dv.Giatien;
            old.Soluong = dv.Soluong;
            old.Hienthi = dv.Hienthi;

            if (imageFile != null)
            {
                DeleteOldImage(old.Imgpath);

                string? img = SaveImage(old.Iddv, imageFile);
                if (img == null)
                    return Json(new { success = false, message = "Ảnh không hợp lệ!" });

                old.Imgpath = img;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =====================================================
        // DELETE AJAX
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            var dv = await _context.Dichvus.FindAsync(id);
            if (dv == null)
                return Json(new { success = false, message = "Không tìm thấy!" });

            bool used = await _context.Hoadondvs.AnyAsync(h => h.Iddv == id);
            if (used)
                return Json(new { success = false, message = "Không thể xoá, dịch vụ đang có trong hóa đơn!" });

            DeleteOldImage(dv.Imgpath);

            _context.Dichvus.Remove(dv);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =====================================================
        // TOGGLE STATUS
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var dv = await _context.Dichvus.FindAsync(id);
            if (dv == null)
                return Json(new { success = false });

            // Nếu null thì coi như false
            dv.Hienthi = !(dv.Hienthi ?? false);

            await _context.SaveChangesAsync();

            return Json(new { success = true, newStatus = dv.Hienthi });
        }


        // =====================================================
        // EXPORT EXCEL THEO FILTER
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel(
            string? keyword,
            string? loaiId,
            bool? status,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var query = _context.Dichvus
                .Include(d => d.IdloaiNavigation)
                .OrderBy(d => d.Tendv)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(d => d.Tendv.Contains(keyword));
            if (!string.IsNullOrWhiteSpace(loaiId))
                query = query.Where(d => d.Idloai == loaiId);
            if (status.HasValue)
                query = query.Where(d => d.Hienthi == status.Value);
            if (minPrice.HasValue)
                query = query.Where(d => d.Giatien >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(d => d.Giatien <= maxPrice.Value);

            var data = await query.ToListAsync();

            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("DichVu");

            string[] headers = ["Mã DV", "Tên DV", "Loại", "Giá", "Tồn", "Trạng thái"];

            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[1, i + 1].Value = headers[i];
                ws.Cells[1, i + 1].Style.Font.Bold = true;
                ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            }

            int row = 2;
            foreach (var dv in data)
            {
                ws.Cells[row, 1].Value = dv.Iddv;
                ws.Cells[row, 2].Value = dv.Tendv;
                ws.Cells[row, 3].Value = dv.IdloaiNavigation?.Tenloai ?? "Chưa phân loại";
                ws.Cells[row, 4].Value = (double)(dv.Giatien ?? 0);
                ws.Cells[row, 5].Value = dv.Soluong ?? 0;
                ws.Cells[row, 6].Value = dv.Hienthi == true ? "Đang KD" : "Ngưng";
                row++;
            }

            ws.Cells.AutoFitColumns();

            var bytes = package.GetAsByteArray();
            string fileName = $"DichVu_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        public async Task<IActionResult> Details(string id)
        {
            var dv = await _context.Dichvus
                .Include(d => d.IdloaiNavigation)
                .FirstOrDefaultAsync(x => x.Iddv == id);

            if (dv == null) return NotFound();

            return View(dv);
        }

    }
}
