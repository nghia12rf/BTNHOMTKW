using BTNHOMTKW.Models; 
using System;
using System.Collections.Generic;
using System.Data.Entity; 
using System.Linq;
using System.Web.Mvc;

namespace BTNHOMTKW.Controllers
{
    public class ProductController : Controller
    {
        
        private DoAn_ShopEntities db = new DoAn_ShopEntities();

        // 1. DANH SÁCH SẢN PHẨM
        // Thêm tham số searchString để nhận dữ liệu từ thanh tìm kiếm trên Layout
        // Controllers/ProductController.cs

        // Thêm tham số priceRange vào hàm Index
        public ActionResult Index(string slug, string searchString, string sortOrder, string priceRange)
        {
            var query = db.SanPhams.Where(p => p.Active);

            // 1. TÌM KIẾM
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.TenSP.Contains(searchString));
                ViewBag.Keyword = searchString;
            }

            // 2. LỌC DANH MỤC (SỬA LỖI KHÔNG HIỆN SP DANH MỤC CHA)
            if (!string.IsNullOrEmpty(slug))
            {
                // Bước A: Tìm danh mục hiện tại theo Slug
                var category = db.DanhMucs.FirstOrDefault(d => d.Slug == slug);

                if (category != null)
                {
                    string tenDanhMuc = category.TenDM; // Lấy tên có dấu (VD: Thời trang Nam)

                    // Bước B: Lọc sản phẩm thuộc danh mục đó HOẶC là con của danh mục đó
                    // Lưu ý: Trong CSDL, ParentDM lưu "Tên" chứ không phải "Slug"
                    query = query.Where(p => p.DanhMucTen == tenDanhMuc || p.DanhMuc.ParentDM == tenDanhMuc);

                    ViewBag.CategoryName = tenDanhMuc;
                }
            }
            else
            {
                ViewBag.CategoryName = "Tất cả sản phẩm";
            }

            // 3. LỌC THEO GIÁ (MỚI THÊM)
            if (!string.IsNullOrEmpty(priceRange))
            {
                ViewBag.PriceRange = priceRange; // Lưu lại để hiện trên View
                switch (priceRange)
                {
                    case "1": // Dưới 200k
                        query = query.Where(p => p.GiaBan < 200000);
                        break;
                    case "2": // 200k - 500k
                        query = query.Where(p => p.GiaBan >= 200000 && p.GiaBan <= 500000);
                        break;
                    case "3": // Trên 500k
                        query = query.Where(p => p.GiaBan > 500000);
                        break;
                }
            }

            // 4. SẮP XẾP
            ViewBag.SortOrder = sortOrder;
            switch (sortOrder)
            {
                case "price_asc": query = query.OrderBy(p => p.GiaBan); break;
                case "price_desc": query = query.OrderByDescending(p => p.GiaBan); break;
                case "name_asc": query = query.OrderBy(p => p.TenSP); break;
                default: query = query.OrderByDescending(p => p.NgayTao); break;
            }

            return View(query.ToList());
        }

        // 2. CHI TIẾT SẢN PHẨM
        // GET: /Product/Detail/id
        public ActionResult Detail(string id)
        {
            if (id == null) return HttpNotFound();

            // 1. Lấy thông tin sản phẩm (Kèm ảnh, đánh giá, danh mục)
            var product = db.SanPhams
                            .Include(p => p.AnhSanPhams)
                            .Include(p => p.DanhGias)
                            .Include(p => p.DanhMuc)
                            .FirstOrDefault(p => p.MaSP == id);

            if (product == null) return HttpNotFound();

            // 2. --- LOGIC LỌC SIZE THÔNG MINH ---
            // Lấy tất cả size từ DB về bộ nhớ trước
            var allSizes = db.Sizes.ToList();
            List<BTNHOMTKW.Models.Size> filteredSizes = new List<BTNHOMTKW.Models.Size>();

            // Lấy tên danh mục cha và con chuyển về chữ thường để so sánh
            string tenDM = product.DanhMucTen.ToLower();
            string parentDM = product.DanhMuc.ParentDM != null ? product.DanhMuc.ParentDM.ToLower() : "";

            // A. Nếu là ÁO (Kiểm tra tên danh mục chứa chữ 'áo')
            if (tenDM.Contains("áo") || parentDM.Contains("áo"))
            {
                // Lấy size chữ: S, M, L, XL... (Ký tự đầu không phải là số)
                filteredSizes = allSizes.Where(s => !char.IsDigit(s.TenSize[0])).ToList();
            }
            // B. Nếu là QUẦN
            else if (tenDM.Contains("quần") || parentDM.Contains("quần"))
            {
                // Lấy size số nhỏ (thường từ 26 đến 34)
                // Logic: Ký tự đầu là số VÀ giá trị < 35
                filteredSizes = allSizes.Where(s => char.IsDigit(s.TenSize[0])
                                                 && int.Parse(s.TenSize) <= 34).ToList();
            }
            // C. Nếu là GIÀY / DÉP / BOOTS
            else if (tenDM.Contains("giày") || tenDM.Contains("boots") || parentDM.Contains("giày"))
            {
                // Lấy size số lớn (thường từ 35 đến 45)
                // Logic: Ký tự đầu là số VÀ giá trị >= 35
                filteredSizes = allSizes.Where(s => char.IsDigit(s.TenSize[0])
                                                 && int.Parse(s.TenSize) >= 35).ToList();
            }
            else
            {
                // Trường hợp khác: Lấy tất cả
                filteredSizes = allSizes;
            }

            // Đẩy Size đã lọc sang View
            ViewBag.Sizes = new SelectList(filteredSizes, "TenSize", "TenSize");

            // Màu sắc thì lấy hết
            ViewBag.Maus = new SelectList(db.Maus, "TenMau", "TenMau");

            // 3. Sản phẩm liên quan
            ViewBag.Related = db.SanPhams
                                .Where(p => p.DanhMucTen == product.DanhMucTen && p.MaSP != id)
                                .OrderBy(r => Guid.NewGuid())
                                .Take(4).ToList();

            return View(product);
        }

        // 3. GỬI ĐÁNH GIÁ
        [HttpPost]
        [Authorize] // Yêu cầu đăng nhập mới được đánh giá
        [ValidateAntiForgeryToken] // Bảo mật form
        public ActionResult SubmitReview(string maSP, int soSao, string binhLuan)
        {
            if (string.IsNullOrEmpty(binhLuan))
            {
                // Nếu bình luận trống, quay lại trang chi tiết
                return RedirectToAction("Detail", new { id = maSP });
            }

            try
            {
                var dg = new DanhGia
                {
                    MaSP = maSP,
                    KhachHangEmail = User.Identity.Name, // Lấy email user đang đăng nhập
                    SoSao = (byte)soSao,
                    BinhLuan = binhLuan,
                    NgayDG = DateTime.Now
                };

                db.DanhGias.Add(dg);
                db.SaveChanges();

                TempData["Success"] = "Cảm ơn bạn đã đánh giá!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi gửi đánh giá.";
            }

            // Quay lại trang chi tiết sản phẩm đó
            return RedirectToAction("Detail", new { id = maSP });
        }

        // 4. GIẢI PHÓNG TÀI NGUYÊN 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}