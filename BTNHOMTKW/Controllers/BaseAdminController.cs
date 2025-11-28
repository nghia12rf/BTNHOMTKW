using BTNHOMTKW.Models; 
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

[Authorize] 
public abstract class BaseAdminController : Controller
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

    // 2. Ghi đè phương thức để kiểm tra vai trò (Role)
    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        // Lấy email từ "vé" đăng nhập
        string email = User.Identity.Name;

        if (!string.IsNullOrEmpty(email))
        {
            var user = db.TaiKhoans.Find(email);

            // Nếu tìm thấy tài khoản VÀ tài khoản đó không phải là Admin
            if (user != null && user.Role != "Admin")
            {
                // Đăng xuất và đá về trang đăng nhập
                FormsAuthentication.SignOut();
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Account",
                        action = "Login"
                    }));
            }
        }

        base.OnActionExecuting(filterContext);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }
}