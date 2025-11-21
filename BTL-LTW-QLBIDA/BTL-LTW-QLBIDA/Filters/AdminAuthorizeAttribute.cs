using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_LTW_QLBIDA.Filters
{
    /// <summary>
    /// Filter kiểm tra quyền Admin
    /// Chỉ cho phép user có QuyenAdmin = 1 truy cập
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var tenDangNhap = session.GetString("TenDangNhap");
            var quyenAdmin = session.GetString("QuyenAdmin");

            // 1. Kiểm tra đăng nhập
            if (string.IsNullOrEmpty(tenDangNhap))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // 2. Kiểm tra quyền Admin
            if (quyenAdmin != "1")
            {
                // Không có quyền Admin → Chuyển về trang Thu Ngân

                // ✅ Sửa lỗi cú pháp tại đây: Khai báo biến controller
                var controller = context.Controller as Controller;

                if (controller != null)
                {
                    // ✅ Sửa lỗi cú pháp TempData: Dùng controller.TempData["Key"]
                    controller.TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                }

                context.Result = new RedirectToActionResult("Index", "ThuNgan", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}