using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BTL_LTW_QLBIDA.Models;

namespace BTL_LTW_QLBIDA.Services
{
    public class PdfService
    {
        private readonly string _outputFolder;
        private readonly string _tempFolder;

        public PdfService(IWebHostEnvironment env)
        {
            _outputFolder = Path.Combine(env.WebRootPath, "invoices");
            _tempFolder = Path.Combine(env.WebRootPath, "invoices", "temp");

            if (!Directory.Exists(_outputFolder))
            {
                Directory.CreateDirectory(_outputFolder);
            }

            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }
        }

        /// <summary>
        /// Tạo PDF tạm để preview (chưa lưu chính thức)
        /// </summary>
        public string TaoHoaDonPdfTemp(Hoadon hoaDon, Ban ban)
        {
            string fileName = $"{hoaDon.Idhd}_temp.pdf";
            string filePath = Path.Combine(_tempFolder, fileName);

            GeneratePdf(hoaDon, ban, filePath);

            return $"/invoices/temp/{fileName}";
        }

        /// <summary>
        /// Lưu PDF chính thức (sau khi user click In)
        /// </summary>
        public string LuuHoaDonPdfChinhThuc(string idHoaDon)
        {
            string tempFileName = $"{idHoaDon}_temp.pdf";
            string finalFileName = $"{idHoaDon}.pdf";

            string tempPath = Path.Combine(_tempFolder, tempFileName);
            string finalPath = Path.Combine(_outputFolder, finalFileName);

            // Copy từ temp sang final
            if (File.Exists(tempPath))
            {
                File.Copy(tempPath, finalPath, overwrite: true);
                File.Delete(tempPath); // Xóa file temp
            }

            return $"/invoices/{finalFileName}";
        }

        /// <summary>
        /// Xóa PDF tạm (khi user click Hủy)
        /// </summary>
        public void XoaPdfTemp(string idHoaDon)
        {
            string tempFileName = $"{idHoaDon}_temp.pdf";
            string tempPath = Path.Combine(_tempFolder, tempFileName);

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        /// <summary>
        /// Hàm chung tạo PDF
        /// </summary>
        private void GeneratePdf(Hoadon hoaDon, Ban ban, string filePath)
        {
            var phienChoi = hoaDon.IdphienNavigation;

            // ← SỬA: Dùng Gioketthuc thay vì DateTime.Now
            DateTime gioKetThuc = phienChoi?.Gioketthuc ?? DateTime.Now;
            DateTime gioBatDau = phienChoi?.Giobatdau ?? DateTime.Now;
            var thoiGianChoi = gioKetThuc - gioBatDau;

            decimal tienGio = 0;
            if (phienChoi?.Giobatdau != null)
            {
                var totalMinutes = (int)thoiGianChoi.TotalMinutes;

                // ← SỬA: Đảm bảo ít nhất 1 block (15 phút)
                var blocks = Math.Max(1, (totalMinutes / 15) + 1);
                var phutTinhTien = blocks * 15;
                tienGio = (phutTinhTien / 60m) * (ban.Giatien ?? 0);
            }

            decimal tienDichVu = hoaDon.Hoadondvs.Sum(x =>
                (x.IddvNavigation?.Giatien ?? 0) * (x.Soluong ?? 0));

            decimal tongTien = tienGio + tienDichVu;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(content => ComposeContent(
                        content, hoaDon, ban, phienChoi, thoiGianChoi, tienGio, tienDichVu, tongTien));
                    page.Footer().Element(ComposeFooter);
                });
            })
            .GeneratePdf(filePath);
        }

        void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("QUÁN BIDA XYZ")
                    .FontSize(16).Bold();

                column.Item().AlignCenter().Text("123 Đường ABC, Hà Nội")
                    .FontSize(9);

                column.Item().AlignCenter().Text("Hotline: 0353703997")
                    .FontSize(9);

                column.Item().PaddingVertical(5).LineHorizontal(1);
            });
        }

        void ComposeContent(
            IContainer container,
            Hoadon hoaDon,
            Ban ban,
            Phienchoi phienChoi,  // ← THÊM
            TimeSpan thoiGianChoi,
            decimal tienGio,
            decimal tienDichVu,
            decimal tongTien)
        {
            container.Column(column =>
            {
                // Thông tin hóa đơn
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text($"Hóa đơn: {hoaDon.Idhd}").Bold();
                        col.Item().Text($"Bàn: {ban.Idban}");

                        // ✅ THÊM: Nhân viên
                        string tenNhanVien = hoaDon.IdnvNavigation?.Hotennv ?? "Nhân viên";
                        col.Item().Text($"Thu ngân: {tenNhanVien}");

                        // ✅ THÊM: Khách hàng
                        string tenKhachHang = hoaDon.IdkhNavigation?.Hoten ?? "Khách lẻ";
                        col.Item().Text($"Khách hàng: {tenKhachHang}");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignRight().Text($"Ngày: {hoaDon.Ngaylap:dd/MM/yyyy HH:mm}");
                        col.Item().AlignRight().Text($"Thời gian chơi: {(int)thoiGianChoi.TotalHours}h {thoiGianChoi.Minutes}p");

                        // ✅ THÊM: SĐT khách hàng (nếu có)
                        string sdtKhachHang = hoaDon.IdkhNavigation?.Sodt ?? "";
                        if (!string.IsNullOrEmpty(sdtKhachHang))
                        {
                            col.Item().AlignRight().Text($"SĐT: {sdtKhachHang}");
                        }
                    });
                });

                column.Item().PaddingVertical(10).LineHorizontal(1);

                // Bảng chi tiết
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background("#A250B2").Padding(5)
                            .Text("TÊN").FontColor("#FFFFFF").Bold();
                        header.Cell().Background("#A250B2").Padding(5).AlignCenter()
                            .Text("SL").FontColor("#FFFFFF").Bold();
                        header.Cell().Background("#A250B2").Padding(5).AlignRight()
                            .Text("ĐƠN GIÁ").FontColor("#FFFFFF").Bold();
                        header.Cell().Background("#A250B2").Padding(5).AlignRight()
                            .Text("THÀNH TIỀN").FontColor("#FFFFFF").Bold();
                    });

                    // Tiền giờ - Tính số lượng giờ
                    decimal soGio = 0;
                    if (phienChoi?.Giobatdau != null)
                    {
                        var totalMinutes = (int)thoiGianChoi.TotalMinutes;
                        var blocks = (totalMinutes / 15) + 1;
                        var phutTinhTien = blocks * 15;
                        soGio = phutTinhTien / 60m; // Ví dụ: 15p = 0.25, 60p = 1
                    }

                    decimal giaBan = ban.Giatien ?? 0;

                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5)
                        .Text("Tiền giờ chơi");
                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5).AlignCenter()
                        .Text(soGio.ToString("0.##")); // 0.25, 1, 1.5
                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5).AlignRight()
                        .Text($"{giaBan:N0}đ/giờ");
                    table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5).AlignRight()
                        .Text($"{tienGio:N0}đ").Bold();

                    // Dịch vụ
                    foreach (var item in hoaDon.Hoadondvs)
                    {
                        var tenDv = item.IddvNavigation?.Tendv ?? "N/A";
                        var sl = item.Soluong ?? 0;
                        var gia = item.IddvNavigation?.Giatien ?? 0;
                        var thanhTien = sl * gia;

                        table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5)
                            .Text(tenDv);
                        table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5).AlignCenter()
                            .Text(sl.ToString());
                        table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5).AlignRight()
                            .Text($"{gia:N0}đ");
                        table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(5).AlignRight()
                            .Text($"{thanhTien:N0}đ").Bold();
                    }
                });

                column.Item().PaddingTop(10);

                // Tổng tiền
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("");
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Tiền giờ:");
                            r.RelativeItem().AlignRight().Text($"{tienGio:N0}đ");
                        });
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Tiền dịch vụ:");
                            r.RelativeItem().AlignRight().Text($"{tienDichVu:N0}đ");
                        });
                        col.Item().PaddingTop(5).LineHorizontal(1);
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("TỔNG CỘNG:").Bold().FontSize(12);
                            r.RelativeItem().AlignRight().Text($"{tongTien:N0}đ")
                                .Bold().FontSize(14).FontColor("#A250B2");
                        });
                    });
                });
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).LineHorizontal(1);
                column.Item().PaddingTop(5).AlignCenter()
                    .Text("Cảm ơn quý khách!").FontSize(11).Bold();
                column.Item().AlignCenter()
                    .Text("Hẹn gặp lại!").FontSize(10);
            });
        }
    }
}