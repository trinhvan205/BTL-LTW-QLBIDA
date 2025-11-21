using System.Drawing;
using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BTL_LTW_QLBIDA.Controllers
{
    [AdminAuthorize]
    public class DichvusController(QlquanBilliardLtw2Context context, IWebHostEnvironment env) : Controller
    {
        private readonly QlquanBilliardLtw2Context _context = context;
        private readonly IWebHostEnvironment _env = env;

        // =====================================================
        // INDEX
        // =====================================================
        public async Task<IActionResult> Index()
        {
            ViewBag.ListLoai = await _context.Loaidichvus
                .OrderBy(l => l.Tenloai)
                .ToListAsync();

            return View();
        }

        // =====================================================
        // SINH MÃ DV
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
            return Json(await GenerateNextIddvAsync());
        }

        // =====================================================
        // LOAD TABLE + FILTER
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
                .Select(d => new Dichvu
                {
                    Iddv = d.Iddv,
                    Tendv = d.Tendv ?? "(Không tên)",
                    Idloai = d.Idloai,
                    Giatien = d.Giatien ?? 0,
                    Soluong = d.Soluong ?? 0,
                    Hienthi = d.Hienthi ?? false,
                    Imgpath = string.IsNullOrEmpty(d.Imgpath)
                        ? "/images/no-image.png"
                        : "/" + d.Imgpath.TrimStart('/'),   // ⭐ Sửa 100% đúng
                    IdloaiNavigation = d.IdloaiNavigation
                })
                .OrderBy(d => d.Tendv)
                .AsQueryable();

            // FILTER
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

            // PHÂN TRANG
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
        // XỬ LÝ ẢNH — ĐÃ SỬA ĐÚNG CHUẨN
        // =====================================================
        private string? SaveImage(string id, IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();
            string[] allowExt = [".jpg", ".jpeg", ".png", ".webp"];
            if (!allowExt.Contains(ext)) return null;

            string folder = Path.Combine(_env.WebRootPath, "images/dichvu");
            Directory.CreateDirectory(folder);

            string fileName = file.FileName;
            string path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            file.CopyTo(stream);

            // ⭐ Đường dẫn CHUẨN webroot
            return "/images/dichvu/" + fileName;
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

            dv.Giatien ??= 0;
            dv.Soluong ??= 0;
            dv.Hienthi ??= true;

            if (imageFile != null)
            {
                string? img = SaveImage(dv.Iddv, imageFile);
                if (img == null)
                    return Json(new { success = false, message = "Ảnh không hợp lệ!" });

                dv.Imgpath = img;
            }
            else
            {
                dv.Imgpath = null;
            }

            _context.Add(dv);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // =====================================================
        // DETAILS
        // =====================================================
        public async Task<IActionResult> DetailPartial(string id)
        {
            var dv = await _context.Dichvus
                .Include(d => d.IdloaiNavigation)
                .Select(d => new Dichvu
                {
                    Iddv = d.Iddv,
                    Tendv = d.Tendv ?? "(Không tên)",
                    Idloai = d.Idloai,
                    Giatien = d.Giatien ?? 0,
                    Soluong = d.Soluong ?? 0,
                    Hienthi = d.Hienthi ?? false,
                    Imgpath = string.IsNullOrEmpty(d.Imgpath)
                        ? "/images/no-image.png"
                        : "/" + d.Imgpath.TrimStart('/'),
                    IdloaiNavigation = d.IdloaiNavigation
                })
                .FirstOrDefaultAsync(d => d.Iddv == id);

            if (dv == null)
                return Content("<p class='text-danger p-3'>Không tìm thấy dịch vụ!</p>");

            return PartialView("_DichvuDetailModal", dv);
        }

        // =====================================================
        // EDIT
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

            old.Tendv = string.IsNullOrWhiteSpace(dv.Tendv) ? "(Không tên)" : dv.Tendv;
            old.Idloai = dv.Idloai;
            old.Giatien = dv.Giatien ?? 0;
            old.Soluong = dv.Soluong ?? 0;
            old.Hienthi = dv.Hienthi ?? false;

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

            dv.Hienthi = !(dv.Hienthi ?? false);

            await _context.SaveChangesAsync();

            return Json(new { success = true, newStatus = dv.Hienthi });
        }
    }
}