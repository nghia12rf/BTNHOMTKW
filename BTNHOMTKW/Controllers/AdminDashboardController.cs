using System;
using System.Collections.Generic;
using System.Data.Entity; 
using System.Linq;
using System.Web.Mvc;
using BTNHOMTKW.Models;

public class AdminDashboardController : BaseAdminController
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

    // GET: AdminDashboard
    public ActionResult Index()
    {
        // 1. Thống kê đơn hàng mới
        ViewBag.DonHangMoi = db.DonHangs.Count(d => d.TrangThai == "Đang xử lý");

        // 2. Thống kê khách hàng
        ViewBag.SoKhachHang = db.KhachHangs.Count();

        // 3. Doanh thu tháng này
        var now = DateTime.Now;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

        ViewBag.DoanhThuThang = db.DonHangs
            .Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat >= firstDayOfMonth)
            .Sum(d => (decimal?)d.ThanhTien) ?? 0m;

        // 4. Doanh thu tổng
        ViewBag.DoanhThuTong = db.DonHangs
            .Where(d => d.TrangThai == "Hoàn thành")
            .Sum(d => (decimal?)d.ThanhTien) ?? 0m;

        // 5. Tổng số sản phẩm
        ViewBag.SanPham = db.SanPhams.Count();

        // 6. Sản phẩm sắp hết hàng 
        // (SỬA LỖI: CSDL dùng 'SoLuongTon', không phải 'SoLuong')
        ViewBag.SapHetHang = db.SanPhams.Count(p => p.SoLuongTon < 10);

        // 7. Đơn hàng chờ xử lý (Theo constraint CSDL là 'Đang xử lý')
        ViewBag.DonHangCanXuLy = db.DonHangs.Count(d => d.TrangThai == "Đang xử lý");

        // 8. Dữ liệu biểu đồ (Gọi hàm riêng)
        ViewBag.DoanhThuTheoThang = GetDoanhThuTheoThang();
        ViewBag.TopSanPham = GetTopSanPham();
        ViewBag.TrangThaiDonHang = GetThongKeTrangThaiDonHang();

        // 9. Lấy 8 đơn hàng mới nhất
        var donHangMoiNhat = db.DonHangs
            .Include(d => d.KhachHang) // Dùng lambda an toàn hơn string
            .OrderByDescending(d => d.NgayDat)
            .Take(8)
            .ToList();

        return View(donHangMoiNhat);
    }

    // API trả về JSON để vẽ biểu đồ bằng JS (Chart.js, ApexCharts...)
    [HttpGet]
    public JsonResult GetChartData()
    {
        var data = new
        {
            doanhThuTheoThang = GetDoanhThuTheoThang(),
            topSanPham = GetTopSanPham(5),
            trangThaiDonHang = GetThongKeTrangThaiDonHang()
        };

        return Json(data, JsonRequestBehavior.AllowGet);
    }

    // === CÁC HÀM HỖ TRỢ (PRIVATE) ===

    private object GetDoanhThuTheoThang()
    {
        var currentYear = DateTime.Now.Year;

        // Cách tối ưu: GroupBy trong SQL 1 lần thay vì gọi DB 12 lần
        var dataDB = db.DonHangs
            .Where(d => d.TrangThai == "Hoàn thành" && d.NgayDat.Year == currentYear)
            .GroupBy(d => d.NgayDat.Month)
            .Select(g => new {
                Thang = g.Key,
                DoanhThu = g.Sum(x => (decimal?)x.ThanhTien) ?? 0m
            })
            .ToList();

        // Map dữ liệu DB vào danh sách đủ 12 tháng (để biểu đồ không bị khuyết)
        var result = new List<object>();
        for (int i = 1; i <= 12; i++)
        {
            var monthData = dataDB.FirstOrDefault(x => x.Thang == i);
            result.Add(new { Thang = i, DoanhThu = monthData != null ? monthData.DoanhThu : 0 });
        }

        return result;
    }

    private List<object> GetTopSanPham(int take = 5)
    {
        
        // Phải GroupBy theo Key (MaSP) và các thuộc tính cần lấy (TenSP)
        var topSanPham = db.ChiTietDonHangs
            .GroupBy(ct => new { ct.MaSP, ct.SanPham.TenSP })
            .Select(g => new
            {
                TenSanPham = g.Key.TenSP,
                SoLuongBan = g.Sum(ct => ct.SoLuong),
                DoanhThu = g.Sum(ct => ct.SoLuong * ct.DonGia)
            })
            .OrderByDescending(x => x.SoLuongBan)
            .Take(take)
            .ToList();

        // Cast sang object để trả về View/JSON
        return topSanPham.Cast<object>().ToList();
    }

    private List<object> GetThongKeTrangThaiDonHang()
    {
        var thongKe = db.DonHangs
            .GroupBy(d => d.TrangThai)
            .Select(g => new
            {
                TrangThai = g.Key,
                SoLuong = g.Count()
            })
            .ToList();

        return thongKe.Cast<object>().ToList();
    }

    // Giải phóng bộ nhớ kết nối CSDL
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            db.Dispose();
        }
        base.Dispose(disposing);
    }
}