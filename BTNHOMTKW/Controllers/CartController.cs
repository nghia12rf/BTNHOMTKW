using BTNHOMTKW.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

public class CartController : Controller
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

    // Lấy SessionKey từ trình duyệt
    private string GetSessionKey()
    {
        if (Session["CartSession"] == null)
        {
            Session["CartSession"] = HttpContext.Session.SessionID;
        }
        return Session["CartSession"].ToString();
    }

    // 1. Thêm vào giỏ
    [HttpPost]
    public ActionResult AddToCart(string maSP, string size, string mau, int soLuong)
    {
        string sessionKey = GetSessionKey();

        // Bước 1: Đảm bảo GioHang tồn tại
        var gioHang = db.GioHangs.FirstOrDefault(g => g.SessionKey == sessionKey);
        if (gioHang == null)
        {
            gioHang = new GioHang
            {
                SessionKey = sessionKey,
                NgayTao = DateTime.Now,
                // Nếu user đã đăng nhập thì lưu email
                KhachHangEmail = User.Identity.IsAuthenticated ? User.Identity.Name : null
            };
            db.GioHangs.Add(gioHang);
            db.SaveChanges();
        }

        // Bước 2: Thêm/Cập nhật GioHangItem
        // Kiểm tra xem SP với Size và Màu đó đã có chưa
        var item = db.GioHangItems.FirstOrDefault(i =>
            i.SessionKey == sessionKey &&
            i.MaSP == maSP &&
            i.TenSize == size &&
            i.TenMau == mau);

        if (item != null)
        {
            item.SoLuong += (short)soLuong; // Cộng dồn
        }
        else
        {
            item = new GioHangItem
            {
                SessionKey = sessionKey,
                MaSP = maSP,
                TenSize = size,
                TenMau = mau,
                SoLuong = (short)soLuong
            };
            db.GioHangItems.Add(item);
        }

        db.SaveChanges();
        if (Request.UrlReferrer != null)
        {
            return Redirect(Request.UrlReferrer.ToString());
        }

        // Phòng hờ nếu không lấy được trang cũ thì mới về trang chủ
        return RedirectToAction("Index", "Home");
    }

    // 2. Xem giỏ hàng
    public ActionResult Index()
    {
        string sessionKey = GetSessionKey();
        var items = db.GioHangItems
                      .Where(i => i.SessionKey == sessionKey)
                      .Include(i => i.SanPham) // Để lấy tên, giá, ảnh
                      .ToList();
        return View(items);
    }

    // 3. Xóa 1 item khỏi giỏ
    public ActionResult Remove(string maSP, string size, string mau)
    {
        string sessionKey = GetSessionKey();
        var item = db.GioHangItems.FirstOrDefault(i =>
            i.SessionKey == sessionKey &&
            i.MaSP == maSP &&
            i.TenSize == size &&
            i.TenMau == mau);

        if (item != null)
        {
            db.GioHangItems.Remove(item);
            db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    // 4. Trang Thanh toán (Checkout)
    public ActionResult Checkout()
    {
        string sessionKey = GetSessionKey();
        var items = db.GioHangItems
                      .Where(i => i.SessionKey == sessionKey)
                      .Include(i => i.SanPham)
                      .ToList();

        if (!items.Any()) return RedirectToAction("Index");

        ViewBag.Total = items.Sum(i => i.SoLuong * i.SanPham.GiaBan);

        KhachHang userModel = null;
        DiaChi defaultAddress = null;

        if (User.Identity.IsAuthenticated)
        {
            userModel = db.KhachHangs.Find(User.Identity.Name);
            defaultAddress = db.DiaChis
                               .FirstOrDefault(d => d.KhachHangEmail == User.Identity.Name && d.MacDinh);
        }

        var checkoutModel = new CheckoutViewModel
        {
            KhachHang = userModel,
            DefaultAddress = defaultAddress
        };

        return View(checkoutModel);
    }


    // 5. Xử lý đặt hàng (Action quan trọng nhất)
    [HttpPost]
    public ActionResult ProcessOrder(string hoTen, string sdt, string diaChi, string ptThanhToan)
    {
        string sessionKey = GetSessionKey();
        var cartItems = db.GioHangItems.Where(i => i.SessionKey == sessionKey).Include(i => i.SanPham).ToList();

        if (cartItems.Count == 0) return RedirectToAction("Index");

        // --- A. Tạo đơn hàng ---
        var order = new DonHang();
        order.MaDonHang = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss"); // Mã tự sinh: DH20231122...

        
        if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

        order.KhachHangEmail = User.Identity.Name;
        order.HoTenNguoiNhan = hoTen;
        order.SDTNguoiNhan = sdt;
        order.DiaChiGiaoHang = diaChi;
        order.PTThanhToan = ptThanhToan;
        order.NgayDat = DateTime.Now;
        order.TrangThai = "Đang xử lý";
        order.TrangThaiThanhToan = "Chưa thanh toán";

        decimal tongTienHang = cartItems.Sum(i => i.SoLuong * i.SanPham.GiaBan);
        order.ThanhTien = tongTienHang;
        order.PhiVanChuyen = 30000; // Phí ship cứng
        order.TongTien = tongTienHang + order.PhiVanChuyen;

        db.DonHangs.Add(order);

        // --- B. Chuyển Giỏ hàng -> Chi tiết đơn hàng ---
        foreach (var item in cartItems)
        {
            var ctdh = new ChiTietDonHang
            {
                MaDonHang = order.MaDonHang,
                MaSP = item.MaSP,
                TenSize = item.TenSize,
                TenMau = item.TenMau,
                SoLuong = item.SoLuong,
                DonGia = item.SanPham.GiaBan
            };
            db.ChiTietDonHangs.Add(ctdh);
        }

        // --- C. Tạo lịch sử đơn hàng ---
        var history = new LichSuDonHang
        {
            MaDonHang = order.MaDonHang,
            TrangThai = "Đang xử lý",
            ThoiGian = DateTime.Now
        };
        db.LichSuDonHangs.Add(history);

        // --- D. Xóa giỏ hàng ---
        db.GioHangItems.RemoveRange(cartItems);

        db.SaveChanges();

        return View("OrderSuccess", model: order.MaDonHang);
    }
    [ChildActionOnly] // Chỉ được gọi từ View, không chạy trực tiếp qua URL
    public ActionResult BagCart()
    {
        // 1. Lấy SessionKey hiện tại
        string sessionKey = GetSessionKey();

        // 2. Tính tổng số lượng sản phẩm trong giỏ
        
        int totalQty = db.GioHangItems
                         .Where(x => x.SessionKey == sessionKey)
                         .Sum(x => (int?)x.SoLuong) ?? 0;

        // 3. Trả về số lượng cho Partial View
        return PartialView("_BagCart", totalQty);
    }
    // Trong CartController.cs

    [HttpPost]
    public ActionResult UpdateQuantity(string maSP, string size, string mau, int newQuantity)
    {
        // 1. Lấy SessionKey
        string sessionKey = GetSessionKey();

        // 2. Tìm sản phẩm trong giỏ
        var item = db.GioHangItems.FirstOrDefault(i =>
            i.SessionKey == sessionKey &&
            i.MaSP == maSP &&
            i.TenSize == size &&
            i.TenMau == mau);

        if (item != null)
        {
            // 3. Kiểm tra số lượng tồn kho (Logic an toàn)
            if (newQuantity > item.SanPham.SoLuongTon)
            {
                return Json(new { success = false, message = "Không đủ hàng trong kho!" });
            }

            // 4. Cập nhật số lượng mới
            if (newQuantity > 0)
            {
                item.SoLuong = (short)newQuantity;
            }
            else
            {
                // Nếu số lượng <= 0 thì xóa luôn
                db.GioHangItems.Remove(item);
            }

            db.SaveChanges();

            // 5. Tính lại tổng tiền giỏ hàng để cập nhật giao diện
            var cartItems = db.GioHangItems.Where(x => x.SessionKey == sessionKey).ToList();
            decimal totalCart = cartItems.Sum(x => x.SoLuong * x.SanPham.GiaBan);
            decimal itemTotal = item.SoLuong * item.SanPham.GiaBan;

            return Json(new
            {
                success = true,
                itemTotal = itemTotal.ToString("#,##0"), // Thành tiền của dòng đó
                totalCart = totalCart.ToString("#,##0")  // Tổng tiền cả giỏ
            });
        }

        return Json(new { success = false, message = "Lỗi sản phẩm" });
    }
}