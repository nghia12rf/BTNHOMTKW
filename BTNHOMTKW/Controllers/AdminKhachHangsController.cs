using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BTNHOMTKW.Models;

public class AdminKhachHangsController : BaseAdminController
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

    // GET: AdminKhachHangs
    public ActionResult Index()
    {
        // Lấy thông tin khách hàng kèm theo tài khoản (để xem email, trạng thái)
        var khachHangs = db.KhachHangs.Include(k => k.TaiKhoan);
        return View(khachHangs.ToList());
    }

    // GET: AdminKhachHangs/Details/user1@gmail.com
    public ActionResult Details(string id) // id ở đây là Email
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // Lấy chi tiết khách hàng: Tài khoản, Địa chỉ, và Lịch sử Đơn hàng
        KhachHang khachHang = db.KhachHangs
            .Include(k => k.TaiKhoan)
            .Include(k => k.DiaChis)
            .Include(k => k.DonHangs)
            .FirstOrDefault(k => k.TaiKhoanEmail == id);

        if (khachHang == null)
        {
            return HttpNotFound();
        }
        return View(khachHang);
    }
}