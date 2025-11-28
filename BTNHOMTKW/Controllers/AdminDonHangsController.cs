using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BTNHOMTKW.Models;

public class AdminDonHangsController : BaseAdminController
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

    // GET: AdminDonHangs
    public ActionResult Index()
    {
        var donHangs = db.DonHangs.Include(d => d.KhachHang).OrderByDescending(d => d.NgayDat);
        return View(donHangs.ToList());
    }

    // GET: AdminDonHangs/Details/DH001
    public ActionResult Details(string id)
    {
        if (id == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // Lấy đầy đủ thông tin: Đơn hàng, Khách hàng, Chi tiết, và Sản phẩm của từng Chi tiết
        DonHang donHang = db.DonHangs
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham)) // Lấy thông tin sản phẩm
            .FirstOrDefault(d => d.MaDonHang == id);

        if (donHang == null)
        {
            return HttpNotFound();
        }

        // Gửi danh sách các trạng thái cho View (để làm dropdown)
        ViewBag.TrangThaiList = new SelectList(
            new[] { "Đang xử lý", "Đã xác nhận", "Đang giao", "Hoàn thành", "Đã hủy" },
            donHang.TrangThai
        );

        return View(donHang);
    }

    // POST: AdminDonHangs/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult UpdateStatus(string maDonHang, string trangThai)
    {
        if (maDonHang == null || trangThai == null)
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        DonHang donHang = db.DonHangs.Find(maDonHang);
        if (donHang == null)
        {
            return HttpNotFound();
        }

        // Cập nhật trạng thái
        donHang.TrangThai = trangThai;

        // TODO: Thêm một bản ghi vào bảng LichSuDonHang
        // db.LichSuDonHangs.Add(new LichSuDonHang { MaDonHang = maDonHang, TrangThai = trangThai, ThoiGian = DateTime.Now });

        db.Entry(donHang).State = EntityState.Modified;
        db.SaveChanges();

        TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
        return RedirectToAction("Details", new { id = maDonHang });
    }
}