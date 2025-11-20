using BTL_LTW_QLBIDA.Models;
using BTL_LTW_QLBIDA.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkiaSharp;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace BTL_LTW_QLBIDA.Controllers
{
    public class HoadonsController(QlquanBilliardLtw2Context context) : Controller
    {
        private readonly QlquanBilliardLtw2Context _db = context;

        // ============================
        // INDEX
        // ============================
        public IActionResult Index()
        {
            return View();
        }
        // ============================

        // INFINITY SCROLL ROWS
        // ============================
        //public IActionResult LoadTable(string? ma, string? khach, string? from, string? to, bool? trangthai, int page = 1)
        //{
        //    int pageSize = 10;

        //    var query = _db.Hoadons
        //        .Include(h => h.IdkhNavigation)
        //        .Include(h => h.IdnvNavigation)
        //        .Include(h => h.IdptttNavigation)
        //        .OrderByDescending(h => h.Ngaylap)
        //        .AsQueryable();

        //    // FILTER
        //    if (!string.IsNullOrEmpty(ma))
        //        query = query.Where(h => h.Idhd.Contains(ma));

        //    if (!string.IsNullOrEmpty(khach))
        //        query = query.Where(h => h.IdkhNavigation.Hoten.Contains(khach));

        //    if (!string.IsNullOrEmpty(from))
        //        query = query.Where(h => h.Ngaylap >= DateTime.Parse(from));

        //    if (!string.IsNullOrEmpty(to))
        //        query = query.Where(h => h.Ngaylap < DateTime.Parse(to).AddDays(1));

        //    if (trangthai != null)
        //        query = query.Where(h => h.Trangthai == trangthai);

        //    // total rows
        //    int total = query.Count();

        //    // items for current page
        //    var items = query
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToList();

        //    bool hasMore = (page * pageSize) < total;

        //    return PartialView("_InvoiceRows", new InvoiceScrollVm
        //    {
        //        Items = items,
        //        HasMore = hasMore
        //    });
        //}
        public IActionResult LoadTable(string? ma, string? khach, string? from, string? to, bool? trangthai, int page = 1)
        {
            int pageSize = 10;

            var query = _db.Hoadons
                .Include(h => h.IdkhNavigation)
                .Include(h => h.IdnvNavigation)
                .Include(h => h.IdptttNavigation)
                .Include(h => h.IdphienNavigation).ThenInclude(p => p.IdbanNavigation)
                .Include(h => h.Hoadondvs).ThenInclude(d => d.IddvNavigation)
                .OrderByDescending(h => h.Ngaylap)
                .AsQueryable();

            if (!string.IsNullOrEmpty(ma))
                query = query.Where(h => h.Idhd.Contains(ma));

            if (!string.IsNullOrEmpty(khach))
                query = query.Where(h => h.IdkhNavigation.Hoten.Contains(khach));

            if (!string.IsNullOrEmpty(from))
                query = query.Where(h => h.Ngaylap >= DateTime.Parse(from));

            if (!string.IsNullOrEmpty(to))
                query = query.Where(h => h.Ngaylap < DateTime.Parse(to).AddDays(1));

            if (trangthai != null)
                query = query.Where(h => h.Trangthai == trangthai);

            int total = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 🔥 TÍNH TỔNG TIỀN CHO TỪNG HOÁ ĐƠN
            foreach (var item in items)
            {
                item.Tongtien = TinhTongTien(item);
            }

            bool hasMore = (page * pageSize) < total;

            return PartialView("_InvoiceRows", new InvoiceScrollVm
            {
                Items = items,
                HasMore = hasMore
            });
        }




        // ============================
        // DETAILS (MODAL AJAX)
        // ============================
        public IActionResult DetailsPartial(string id)
        {
            var hd = _db.Hoadons
                .Include(h => h.IdkhNavigation)
                .Include(h => h.IdnvNavigation)
                .Include(h => h.IdptttNavigation)
                .Include(h => h.IdphienNavigation)
                    .ThenInclude(p => p.IdbanNavigation)
                .Include(h => h.Hoadondvs)
                    .ThenInclude(d => d.IddvNavigation)
                .FirstOrDefault(h => h.Idhd == id);

            if (hd == null)
                return Content("<p class='text-danger p-3'>Không tìm thấy hóa đơn!</p>");

            return PartialView("_DetailsModal", hd);
        }


        // ============================
        // EDIT (MODAL AJAX)
        // ============================
        public IActionResult EditPartial(string id)
        {
            var hd = _db.Hoadons.FirstOrDefault(h => h.Idhd == id);
            if (hd == null)
                return Content("<p class='text-danger p-3'>Không tìm thấy hóa đơn!</p>");

            ViewBag.Khachhangs = _db.Khachhangs
                .Select(k => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = k.Idkh,
                    Text = k.Hoten
                }).ToList();

            ViewBag.Nhanviens = _db.Nhanviens
                .Select(n => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = n.Idnv,
                    Text = n.Hotennv
                }).ToList();

            ViewBag.Pttt = _db.Phuongthucthanhtoans
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = p.Idpttt,
                    Text = p.Tenpttt
                }).ToList();

            return PartialView("_EditModal", hd);
        }

        // ============================
        // EDIT AJAX
        // ============================
        [HttpPost]
        public IActionResult EditAjax(string Idhd, string Idkh, string Idnv, bool Trangthai, string Idpttt)
        {
            var hd = _db.Hoadons.FirstOrDefault(h => h.Idhd == Idhd);
            if (hd == null)
                return Json(new { success = false, message = "Không tìm thấy hóa đơn!" });

            hd.Idkh = Idkh;
            hd.Idnv = Idnv;
            hd.Trangthai = Trangthai;
            hd.Idpttt = Idpttt;

            _db.SaveChanges();

            return Json(new { success = true });
        }

        // ============================
        // DELETE AJAX
        // ============================
        [HttpPost]
        public IActionResult DeleteAjax(string id)
        {
            var hd = _db.Hoadons.FirstOrDefault(h => h.Idhd == id);
            if (hd == null)
                return Json(new { success = false, message = "Không tìm thấy hóa đơn!" });

            // ❗ Không cho xóa hóa đơn chưa hoàn thành
            if (hd.Trangthai == false)
                return Json(new { success = false, message = "Chỉ được xóa hóa đơn đã hoàn thành!" });

            var items = _db.Hoadondvs.Where(c => c.Idhd == id).ToList();
            _db.Hoadondvs.RemoveRange(items);
            _db.Hoadons.Remove(hd);
            _db.SaveChanges();

            return Json(new { success = true });
        }


        // ============================
        // TOGGLE STATUS
        // ============================
        [HttpPost]
        public IActionResult ToggleStatusAjax(string id)
        {
            var hd = _db.Hoadons.FirstOrDefault(h => h.Idhd == id);
            if (hd == null)
                return Json(new { success = false });

            hd.Trangthai = !hd.Trangthai;
            _db.SaveChanges();

            return Json(new { success = true });
        }

        // ============================
        // EXPORT EXCEL
        // ============================
        [HttpGet]
        public IActionResult ExportExcel(
    string? ma,
    string? khach,
    string? from,
    string? to,
    bool? trangthai,
    string? cols)
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var query = _db.Hoadons
                .Include(h => h.IdkhNavigation)
                .Include(h => h.IdnvNavigation)
                .OrderByDescending(h => h.Ngaylap)
                .AsQueryable();

            if (!string.IsNullOrEmpty(ma))
                query = query.Where(h => h.Idhd.Contains(ma));

            if (!string.IsNullOrEmpty(khach))
                query = query.Where(h => h.IdkhNavigation.Hoten.Contains(khach));

            if (!string.IsNullOrEmpty(from))
                query = query.Where(h => h.Ngaylap >= DateTime.Parse(from));

            if (!string.IsNullOrEmpty(to))
                query = query.Where(h => h.Ngaylap < DateTime.Parse(to).AddDays(1));

            if (trangthai != null)
                query = query.Where(h => h.Trangthai == trangthai);

            var data = query.ToList();

            // Cột
            List<string> colList = [];
            if (!string.IsNullOrEmpty(cols))
                colList = [.. cols.Split(',')];
            else
                colList = ["mahd", "ngaylap", "khach", "nhanvien", "tongtien", "trangthai"];

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("HoaDon");

            int col = 1;

            if (colList.Contains("mahd")) ws.Cells[1, col++].Value = "Mã HĐ";
            if (colList.Contains("ngaylap")) ws.Cells[1, col++].Value = "Ngày lập";
            if (colList.Contains("khach")) ws.Cells[1, col++].Value = "Khách";
            if (colList.Contains("nhanvien")) ws.Cells[1, col++].Value = "Nhân viên";
            if (colList.Contains("tongtien")) ws.Cells[1, col++].Value = "Tổng tiền";
            if (colList.Contains("trangthai")) ws.Cells[1, col++].Value = "Trạng thái";

            int row = 2;

            foreach (var h in data)
            {
                col = 1;

                if (colList.Contains("mahd")) ws.Cells[row, col++].Value = h.Idhd;
                if (colList.Contains("ngaylap")) ws.Cells[row, col++].Value = h.Ngaylap?.ToString("dd/MM/yyyy HH:mm");
                if (colList.Contains("khach")) ws.Cells[row, col++].Value = h.IdkhNavigation?.Hoten;
                if (colList.Contains("nhanvien")) ws.Cells[row, col++].Value = h.IdnvNavigation?.Hotennv;
                if (colList.Contains("tongtien")) ws.Cells[row, col++].Value = (double)(h.Tongtien ?? 0);
                if (colList.Contains("trangthai")) ws.Cells[row, col++].Value = h.Trangthai == true ? "Hoàn thành" : "Đang xử lý";

                row++;
            }

            ws.Cells.AutoFitColumns();

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HoaDon_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            );
        }


        // ============================
        // PRINT
        // ============================
        public IActionResult Print(string id)
        {
            var hd = _db.Hoadons
                .Include(h => h.IdkhNavigation)
                .Include(h => h.IdnvNavigation)
                .Include(h => h.IdptttNavigation)
                .Include(h => h.IdphienNavigation)
                    .ThenInclude(p => p.IdbanNavigation)
                .Include(h => h.Hoadondvs).ThenInclude(d => d.IddvNavigation)
                .FirstOrDefault(h => h.Idhd == id);

            if (hd == null) return NotFound();

            return View("Print", hd);
        }
        private decimal TinhTongTien(Hoadon hd)
        {
            var phien = hd.IdphienNavigation;

            int tongPhut = 0;

            if (phien?.Giobatdau != null && phien.Gioketthuc != null)
            {
                tongPhut = (int)(phien.Gioketthuc.Value - phien.Giobatdau.Value).TotalMinutes;
            }

            if (tongPhut < 0) tongPhut = 0;

            // block 15 phút
            int soBlock = (tongPhut / 15) + 1;

            int phutTinhTien = soBlock * 15;

            decimal gioTinhTien = phutTinhTien / 60m;

            decimal giaBan = phien?.IdbanNavigation?.Giatien ?? 0;

            decimal tienGio = gioTinhTien * giaBan;

            decimal tienDv = hd.Hoadondvs.Sum(d =>
                (d.IddvNavigation?.Giatien ?? 0) * (d.Soluong ?? 0)
            );

            return tienGio + tienDv;
        }



    }
}
