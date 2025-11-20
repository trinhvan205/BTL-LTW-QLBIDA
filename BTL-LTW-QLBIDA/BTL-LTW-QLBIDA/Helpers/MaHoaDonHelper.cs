using BTL_LTW_QLBIDA.Models;

namespace BTL_LTW_QLBIDA.Helpers
{
    public static class MaHoaDonHelper
    {
        /// <summary>
        /// Tạo mã hóa đơn: HDddMMyyyy001
        /// </summary>
        public static string TaoMaHoaDon(QlquanBilliardLtw2Context context)
        {
            string ngayHienTai = DateTime.Now.ToString("ddMMyyyy");
            string prefix = $"HD{ngayHienTai}";

            var hoaDonCuoi = context.Hoadons
                .Where(h => h.Idhd.StartsWith(prefix))
                .OrderByDescending(h => h.Idhd)
                .FirstOrDefault();

            int stt = 1;
            if (hoaDonCuoi != null)
            {
                string sttStr = hoaDonCuoi.Idhd.Substring(prefix.Length);
                if (int.TryParse(sttStr, out int sttCu))
                {
                    stt = sttCu + 1;
                }
            }

            return $"{prefix}{stt:D6}";
        }

        /// <summary>
        /// Tạo mã phiên chơi: PCddMMyyyy001
        /// </summary>
        public static string TaoMaPhienChoi(QlquanBilliardLtw2Context context)
        {
            string ngayHienTai = DateTime.Now.ToString("ddMMyyyy");
            string prefix = $"PC{ngayHienTai}";

            var phienCuoi = context.Phienchois
                .Where(p => p.Idphien.StartsWith(prefix))
                .OrderByDescending(p => p.Idphien)
                .FirstOrDefault();

            int stt = 1;
            if (phienCuoi != null)
            {
                string sttStr = phienCuoi.Idphien.Substring(prefix.Length);
                if (int.TryParse(sttStr, out int sttCu))
                {
                    stt = sttCu + 1;
                }
            }

            return $"{prefix}{stt:D6}";
        }
    }
}