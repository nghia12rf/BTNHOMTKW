using BTNHOMTKW.Models; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BTNHOMTKW.Controllers
{
    public class HomeController : Controller
    {
        private DoAn_ShopEntities db = new DoAn_ShopEntities();

        public ActionResult Index()
        {
            // 1. NEW ARRIVAL 
            var newArrivals = db.SanPhams.Where(x => x.Active).OrderByDescending(x => x.NgayTao).Take(8).ToList();

            // 2. BEST SELLER (Sản phẩm bán chạy)
            
            ViewBag.BestSellers = db.SanPhams.Where(x => x.Active).OrderBy(x => Guid.NewGuid()).Take(8).ToList();

            return View(newArrivals);
        }

        // ---  GỌI MENU ---
        [ChildActionOnly]
        public ActionResult _MenuDanhMuc()
        {
            // Lấy tất cả danh mục để View tự phân chia cha/con
            var danhMucs = db.DanhMucs.ToList();
            return PartialView("_MenuDanhMuc", danhMucs);
        }
        public ActionResult About()
        {
            // 1. Truyền dữ liệu văn bản từ Controller
            ViewBag.Title = "Về Chúng Tôi";
            ViewBag.Message = "PUREAR - Điểm đến thời trang tin cậy của giới trẻ.";
            ViewBag.Description = "Chúng tôi cam kết mang đến những sản phẩm chất lượng nhất với giá thành hợp lý nhất.";

            // 2. Truyền dữ liệu thống kê thực tế từ Database 
            // Đếm số lượng đang có trong CSDL
            ViewBag.TotalProducts = db.SanPhams.Count(x => x.Active == true); // Tổng sản phẩm đang bán
            ViewBag.TotalUsers = db.KhachHangs.Count();                       // Tổng khách hàng thành viên
            ViewBag.TotalOrders = db.DonHangs.Count();                        // Tổng đơn hàng đã phục vụ

            return View();
        }
        // 1. Tạo Model phụ để chứa thông tin cửa hàng (Viết ngay trong Controller hoặc tạo file riêng đều được)
        public class StoreInfo
        {
            public string TenCuaHang { get; set; }
            public string DiaChi { get; set; }
            public string SDT { get; set; }
            public string GioMoCua { get; set; }
            public string MapEmbedUrl { get; set; } // Link nhúng bản đồ Google
        }

        // 2. Action Contact
        public ActionResult Contact()
        {
            // Tạo dữ liệu giả lập 
            var stores = new List<StoreInfo>
            {
                new StoreInfo
                {
                    TenCuaHang = "Trường Đại học Công Thương TP. Hồ Chí Minh",
                    DiaChi = "140 Lê Trọng Tấn, Tây Thạnh, Tân Phú, Thành phố Hồ Chí Minh",
                    SDT = "070 291 9822",
                    GioMoCua = "7h00 - 22h00",
                    // Link embed lấy từ Google Maps (Share -> Embed a map)
                    MapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d1001.4526629080776!2d106.628019895199!3d10.805833843416844!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752be27d8b4f4d%3A0x92dcba2950430867!2zVHLGsOG7nW5nIMSQ4bqhaSBo4buNYyBDw7RuZyBUaMawxqFuZyBUUC4gSOG7kyBDaMOtIE1pbmggKEhVSVQp!5e1!3m2!1svi!2s!4v1763807504428!5m2!1svi!2s"
                },
                new StoreInfo
                {
                    TenCuaHang = "Trung tâm Thí nghiệm – Thực hành",
                    DiaChi = "93 Tân Kỳ Tân Quý, P. Tân Quý, Q. Tân Phú, Tp. Hồ Chí Minh.",
                    SDT = "(08)3810052",
                    GioMoCua = "07h00 - 22h00",
                    MapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d1030.8046782939311!2d106.63253455769622!3d10.803731158019904!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752bfc7b183e1d%3A0xb73baab34fba50b9!2zVHJ1bmcgdMOibSBUaMOtIG5naGnhu4dtIFRo4buxYyBow6BuaCBUcsaw4budbmcgxJDhuqFpIGjhu41jIEPDtG5nIFRoxrDGoW5nIFRQLkhDTQ!5e1!3m2!1svi!2s!4v1763807946194!5m2!1svi!2s"
                },
                new StoreInfo
                {
                    TenCuaHang = "Khoa Giáo dục Thể chất và Quốc phòng – An ninh",
                    DiaChi = "73/1 Nguyễn Đỗ Cung, P. Tây Thạnh, Q. Tân Phú, TP.HCM",
                    SDT = "093 9999 9999",
                    GioMoCua = "07h00 - 22h00",
                    MapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d433.3918767427219!2d106.62939021979157!3d10.809343039428546!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x3175296248f9ebbb%3A0xd524aff8d2ea432f!2zS2hvYSBHacOhbyBk4bulYyB0aOG7gyBjaOG6pXQgdsOgIFF14buRYyBwaMOybmcgLSBBbiBuaW5oIFRyxrDhu51uZyDEkOG6oWkgaOG7jWMgQ8O0bmcgVGjGsMahbmcgVFBIQ00!5e1!3m2!1svi!2s!4v1763807864886!5m2!1svi!2s"
                }
            };

            return View(stores);
        }
    }
}