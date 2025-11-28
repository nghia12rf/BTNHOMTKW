using BTNHOMTKW.Models; 
using System;
using System.Linq;
using System.Web.Mvc;

namespace BTNHOMTKW.Controllers
{
    public class DanhMucApiController : Controller
    {
        private DoAn_ShopEntities db = new DoAn_ShopEntities();

        // 1. API HIỂN THỊ (GET): Trả về danh sách JSON
        // URL truy cập: /DanhMucApi/GetList
        [HttpGet]
        public JsonResult GetList()
        {
            // Lưu ý: Phải dùng .Select để tạo object mới, tránh lỗi vòng lặp (Circular Reference) của Entity Framework
            var dsDanhMuc = db.DanhMucs.Select(x => new
            {
                TenDM = x.TenDM,
                Slug = x.Slug,
                ParentDM = x.ParentDM
            }).ToList();

            return Json(new { success = true, data = dsDanhMuc }, JsonRequestBehavior.AllowGet);
        }

        // 2. API THÊM MỚI (POST)
        // Dùng Postman hoặc Ajax để test
        [HttpPost]
        public JsonResult Add(string tenDM, string slug, string parentDM)
        {
            try
            {
                var dm = new DanhMuc();
                dm.TenDM = tenDM;
                dm.Slug = slug;
                dm.ParentDM = parentDM; // Có thể null

                db.DanhMucs.Add(dm);
                db.SaveChanges();

                return Json(new { success = true, message = "Thêm thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 3. API SỬA (POST/PUT)
        [HttpPost]
        public JsonResult Edit(string tenDMcu, string tenDMmoi, string slug)
        {
            try
            {
                var dm = db.DanhMucs.Find(tenDMcu);
                if (dm == null) return Json(new { success = false, message = "Không tìm thấy" });

                
                dm.Slug = slug;
                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 4. API XÓA (POST/DELETE)
        [HttpPost]
        public JsonResult Delete(string tenDM)
        {
            try
            {
                var dm = db.DanhMucs.Find(tenDM);
                if (dm == null) return Json(new { success = false, message = "Không tìm thấy" });

                db.DanhMucs.Remove(dm);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi xóa: Có thể danh mục này đang chứa sản phẩm." });
            }
        }
    }
}