using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using BTNHOMTKW.Models; 

namespace BTNHOMTKW.Controllers
{
    // Kế thừa BaseAdminController để chặn User thường
    public class AdminDanhGiasController : BaseAdminController
    {
        private DoAn_ShopEntities db = new DoAn_ShopEntities();

        // 1. DANH SÁCH ĐÁNH GIÁ
        public ActionResult Index()
        {
            // Eager Loading: Lấy kèm thông tin Sản phẩm và Khách hàng để hiển thị tên
            var danhGias = db.DanhGias.Include(d => d.SanPham).Include(d => d.KhachHang);

            // Sắp xếp mới nhất lên đầu
            return View(danhGias.OrderByDescending(d => d.NgayDG).ToList());
        }

        // 2. XÓA ĐÁNH GIÁ (GET) - Hiển thị xác nhận
        // Lưu ý: Phải nhận vào 2 tham số vì khóa chính là cặp (MaSP, Email)
        public ActionResult Delete(string maSP, string email)
        {
            if (string.IsNullOrEmpty(maSP) || string.IsNullOrEmpty(email))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm đánh giá dựa trên 2 khóa chính
            var danhGia = db.DanhGias.Find(maSP, email);

            if (danhGia == null)
            {
                return HttpNotFound();
            }
            return View(danhGia);
        }

        // 3. XÓA ĐÁNH GIÁ (POST) - Thực hiện xóa
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string maSP, string email)
        {
            var danhGia = db.DanhGias.Find(maSP, email);
            db.DanhGias.Remove(danhGia);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}