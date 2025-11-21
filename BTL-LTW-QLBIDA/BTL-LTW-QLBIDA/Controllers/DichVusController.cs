using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_LTW_QLBIDA.Controllers
{
    [AdminAuthorize]
    public class DichvusController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;
        private readonly IWebHostEnvironment _env;

        public DichvusController(QlquanBilliardLtw2Context context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 1. VIEW CHÍNH
        public async Task<IActionResult> Index()
        {
            ViewBag.LoaiDichVu = await _context.Loaidichvus.ToListAsync();
            return View();
        }

        // 2. LOAD TABLE
        public async Task<IActionResult> LoadTable(string search, string loai, string trangthai, int page = 1)
        {
            int pageSize = 10;
            var query = _context.Dichvus.Include(d => d.IdloaiNavigation).AsQueryable();

            // Filter
            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Tendv.Contains(search) || d.Iddv.Contains(search));

            if (!string.IsNullOrEmpty(loai))
                query = query.Where(d => d.Idloai == loai);

            if (!string.IsNullOrEmpty(trangthai))
            {
                bool isHienThi = trangthai == "true";
                query = query.Where(d => d.Hienthi == isHienThi);
            }

            // Sắp xếp
            var listAll = await query.OrderBy(d => d.Tendv).ToListAsync();

            int total = listAll.Count;
            var items = listAll.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            bool hasMore = (page * pageSize) < total;

            return PartialView("_DichvuRows", new DichvuScrollVm { Items = items, HasMore = hasMore });
        }

        // 3. CREATE
        public async Task<IActionResult> CreatePartial()
        {
            // Sinh mã tự động DV001
            var lastId = await _context.Dichvus
                .OrderByDescending(d => d.Iddv)
                .Select(d => d.Iddv)
                .FirstOrDefaultAsync();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastId) && lastId.Length > 2 && int.TryParse(lastId.Substring(2), out int n))
            {
                nextNum = n + 1;
            }

            ViewBag.LoaiDichVu = await _context.Loaidichvus.ToListAsync();
            var dv = new Dichvu { Iddv = $"DV{nextNum:D3}", Hienthi = true };
            return PartialView("_CreateModal", dv);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax(Dichvu model, IFormFile? imageFile)
        {
            if (await _context.Dichvus.AnyAsync(d => d.Iddv == model.Iddv))
                return Json(new { success = false, message = "Mã dịch vụ đã tồn tại!" });

            // Xử lý ảnh
            if (imageFile != null)
            {
                string uploadDir = Path.Combine(_env.WebRootPath, "images/dichvu");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                string fileName = $"{model.Iddv}_{DateTime.Now.Ticks}{Path.GetExtension(imageFile.FileName)}";
                string filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                model.Imgpath = $"/images/dichvu/{fileName}";
            }
            else
            {
                model.Imgpath = "/images/default-product.png"; // Ảnh mặc định
            }

            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 4. EDIT
        public async Task<IActionResult> EditPartial(string id)
        {
            var dv = await _context.Dichvus.FindAsync(id);
            if (dv == null) return Content("Không tìm thấy!");

            ViewBag.LoaiDichVu = await _context.Loaidichvus.ToListAsync();
            return PartialView("_EditModal", dv);
        }

        [HttpPost]
        public async Task<IActionResult> EditAjax(Dichvu model, IFormFile? imageFile)
        {
            var dv = await _context.Dichvus.FindAsync(model.Iddv);
            if (dv == null) return Json(new { success = false, message = "Không tìm thấy!" });

            dv.Tendv = model.Tendv;
            dv.Idloai = model.Idloai;
            dv.Giatien = model.Giatien;
            dv.Soluong = model.Soluong;
            dv.Hienthi = model.Hienthi;

            // Cập nhật ảnh nếu có upload mới
            if (imageFile != null)
            {
                string uploadDir = Path.Combine(_env.WebRootPath, "images/dichvu");
                string fileName = $"{model.Iddv}_{DateTime.Now.Ticks}{Path.GetExtension(imageFile.FileName)}";
                string filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                dv.Imgpath = $"/images/dichvu/{fileName}";
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 5. DETAILS
        public async Task<IActionResult> DetailsPartial(string id)
        {
            var dv = await _context.Dichvus
                .Include(d => d.IdloaiNavigation)
                .FirstOrDefaultAsync(d => d.Iddv == id);
            return PartialView("_DetailsModal", dv);
        }

        // 6. DELETE
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            var dv = await _context.Dichvus.FindAsync(id);
            if (dv == null) return Json(new { success = false, message = "Lỗi ID" });

            // Kiểm tra ràng buộc
            bool used = await _context.Hoadondvs.AnyAsync(h => h.Iddv == id);
            if (used) return Json(new { success = false, message = "Dịch vụ đã có trong hóa đơn, không thể xóa!" });

            _context.Dichvus.Remove(dv);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}