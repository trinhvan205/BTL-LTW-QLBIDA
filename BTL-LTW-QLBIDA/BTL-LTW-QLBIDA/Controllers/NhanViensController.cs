using BTL_LTW_QLBIDA.Filters;
using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Models.ViewModels; // Nhớ using namespace chứa NhanVienScrollVm
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BTL_LTW_QLBIDA.Controllers
{
    [AdminAuthorize]
    public class NhanviensController : Controller
    {
        private readonly QlquanBilliardLtw2Context _context;

        public NhanviensController(QlquanBilliardLtw2Context context)
        {
            _context = context;
        }

        // 1. VIEW CHÍNH (CONTAINER)
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("QuyenAdmin") != "1")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // 2. LOAD TABLE (INFINITE SCROLL AJAX)
        public async Task<IActionResult> LoadTable(string search, string trangthai, int page = 1)
        {
            int pageSize = 10; // Số dòng mỗi lần tải
            var query = _context.Nhanviens.AsQueryable();

            // Filter Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(nv =>
                    nv.Hotennv.Contains(search) ||
                    nv.Idnv.Contains(search) ||
                    nv.Sodt.Contains(search) ||
                    nv.Cccd.Contains(search));
            }

            // Filter Trạng thái
            if (!string.IsNullOrEmpty(trangthai))
            {
                if (trangthai == "false") // Đang làm (Nghiviec = false)
                    query = query.Where(nv => nv.Nghiviec == false);
                else if (trangthai == "true") // Đã nghỉ (Nghiviec = true)
                    query = query.Where(nv => nv.Nghiviec == true);
            }
            else
            {
                // Mặc định chỉ hiện đang làm nếu không chọn gì (tuỳ chọn)
                // query = query.Where(nv => nv.Nghiviec == false);
            }

            // Sắp xếp (Sort logic cũ của bạn: Lấy phần số sau 'NV' để sort)
            var listAll = await query.ToListAsync();
            var sortedList = listAll.OrderBy(nv =>
            {
                if (nv.Idnv.Length > 2 && int.TryParse(nv.Idnv.Substring(2), out int idNum))
                {
                    return idNum; // NV001 -> 1
                }
                return 999999;
            }).ToList();

            int total = sortedList.Count;

            var items = sortedList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            bool hasMore = (page * pageSize) < total;

            return PartialView("_NhanVienRows", new NhanVienScrollVm
            {
                Items = items,
                HasMore = hasMore
            });
        }

        // 3. CREATE (MODAL)
        // 3. CREATE (MODAL) - PHIÊN BẢN AN TOÀN
        // GET: Nhanviens/CreatePartial
        public async Task<IActionResult> CreatePartial()
        {
            // BƯỚC 1: Lấy toàn bộ mã NV về trước (chỉ lấy cột ID để nhẹ)
            // LƯU Ý: Không dùng .Substring() trong câu lệnh này để tránh lỗi 500
            var allIdnvs = await _context.Nhanviens
                .Where(nv => nv.Idnv.StartsWith("NV"))
                .Select(nv => nv.Idnv)
                .ToListAsync();

            // BƯỚC 2: Xử lý tìm số lớn nhất trên RAM (C#)
            int maxNumber = 0;
            foreach (var id in allIdnvs)
            {
                // Kiểm tra độ dài > 2 (để tránh lỗi nếu có mã lạ như "NV")
                if (id.Length > 2 && int.TryParse(id.Substring(2), out int number))
                {
                    if (number > maxNumber) maxNumber = number;
                }
            }

            // BƯỚC 3: Tạo mã mới
            // Dùng format D3 để luôn có 3 số: NV001, NV010, NV100...
            string newIdnv = $"NV{maxNumber + 1:D3}";

            // BƯỚC 4: Trả về Partial View
            var nv = new Nhanvien { Idnv = newIdnv };
            return PartialView("_CreateModal", nv);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax(Nhanvien model)
        {
            // Validation
            if (await _context.Nhanviens.AnyAsync(n => n.Idnv == model.Idnv))
                return Json(new { success = false, message = "Mã nhân viên đã tồn tại!" });

            if (!string.IsNullOrEmpty(model.Cccd) && await _context.Nhanviens.AnyAsync(n => n.Cccd == model.Cccd))
                return Json(new { success = false, message = "CCCD đã tồn tại!" });

            if (!string.IsNullOrEmpty(model.Tendangnhap) && await _context.Nhanviens.AnyAsync(n => n.Tendangnhap == model.Tendangnhap))
                return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });

            // Set default values
            model.Hienthi = true;
            model.Nghiviec = false;

            if (string.IsNullOrEmpty(model.Matkhau)) model.Matkhau = null;
            // Lưu ý: Nên mã hoá mật khẩu ở đây trước khi lưu (MD5/BCrypt)

            _context.Add(model);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // 4. EDIT (MODAL)
        public async Task<IActionResult> EditPartial(string id)
        {
            var nv = await _context.Nhanviens.FindAsync(id);
            if (nv == null) return Content("<div class='p-3 text-danger'>Không tìm thấy nhân viên!</div>");
            return PartialView("_EditModal", nv);
        }

        [HttpPost]
        public async Task<IActionResult> EditAjax(Nhanvien model)
        {
            var nv = await _context.Nhanviens.FindAsync(model.Idnv);
            if (nv == null) return Json(new { success = false, message = "Không tìm thấy nhân viên!" });

            // Check trùng (trừ chính nó)
            if (!string.IsNullOrEmpty(model.Cccd) && await _context.Nhanviens.AnyAsync(n => n.Cccd == model.Cccd && n.Idnv != model.Idnv))
                return Json(new { success = false, message = "CCCD đã thuộc về người khác!" });

            if (!string.IsNullOrEmpty(model.Tendangnhap) && await _context.Nhanviens.AnyAsync(n => n.Tendangnhap == model.Tendangnhap && n.Idnv != model.Idnv))
                return Json(new { success = false, message = "Tên đăng nhập đã có người dùng!" });

            // Cập nhật thông tin
            nv.Hotennv = model.Hotennv;
            nv.Ngaysinh = model.Ngaysinh;
            nv.Gioitinh = model.Gioitinh;
            nv.Sodt = model.Sodt;
            nv.Cccd = model.Cccd;
            nv.Quyenadmin = model.Quyenadmin;
            nv.Tendangnhap = model.Tendangnhap;

            // Logic mật khẩu: Nếu nhập mới thì đổi, không thì giữ nguyên
            if (!string.IsNullOrEmpty(model.Matkhau))
            {
                nv.Matkhau = model.Matkhau; // Nên mã hoá
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 5. DETAILS (MODAL)
        public async Task<IActionResult> DetailsPartial(string id)
        {
            var nv = await _context.Nhanviens.FindAsync(id);
            if (nv == null) return Content("<div class='p-3 text-danger'>Không tìm thấy!</div>");
            return PartialView("_DetailsModal", nv);
        }

        // 6. DELETE & TOGGLE
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            var nv = await _context.Nhanviens.FindAsync(id);
            if (nv == null) return Json(new { success = false, message = "Không tìm thấy!" });

            // Logic an toàn: Không xoá người có tài khoản
            if (!string.IsNullOrEmpty(nv.Tendangnhap) || !string.IsNullOrEmpty(nv.Matkhau))
                return Json(new { success = false, message = "Không thể xóa nhân viên có tài khoản! Hãy chuyển sang 'Nghỉ việc'." });

            _context.Nhanviens.Remove(nv);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatusAjax(string id)
        {
            var nv = await _context.Nhanviens.FindAsync(id);
            if (nv != null)
            {
                nv.Nghiviec = !nv.Nghiviec; // Đảo trạng thái
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}