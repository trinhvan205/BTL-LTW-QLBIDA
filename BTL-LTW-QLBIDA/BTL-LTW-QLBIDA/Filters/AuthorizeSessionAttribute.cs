using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BTL_LTW_QLBIDA.Filters
{
    /// <summary>
    /// Filter kiểm tra đăng nhập
    /// Cho phép tất cả user đã đăng nhập truy cập
    /// </summary>
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var tenDangNhap = session.GetString("TenDangNhap");

            if (string.IsNullOrEmpty(tenDangNhap))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}