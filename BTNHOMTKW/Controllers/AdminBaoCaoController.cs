using BTNHOMTKW.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

public class AdminBaoCaoController : BaseAdminController
{
    private DoAn_ShopEntities db = new DoAn_ShopEntities();

    // GET: AdminBaoCao
    public ActionResult Index(DateTime? fromDate, DateTime? toDate, string reportType = "daily")
    {
        // Mặc định lấy 30 ngày gần nhất
        var start = fromDate ?? DateTime.Now.AddDays(-30).Date; // Lấy đầu ngày
        var end = toDate ?? DateTime.Now.Date;

        // Đảm bảo ngày kết thúc là cuối ngày (23:59:59)
        var queryEnd = end.AddDays(1).AddSeconds(-1);

        var model = GenerateReport(start, queryEnd, reportType);
        ViewBag.ReportType = reportType;

        return View(model);
    }

    // API cho biểu đồ real-time (Ajax)
    [HttpGet]
    public JsonResult GetChartData(DateTime? fromDate, DateTime? toDate, string reportType = "daily")
    {
        var start = fromDate ?? DateTime.Now.AddDays(-30).Date;
        var end = toDate ?? DateTime.Now.Date;
        var queryEnd = end.AddDays(1).AddSeconds(-1);

        var model = GenerateReport(start, queryEnd, reportType);

        var chartData = new
        {
            labels = model.Labels,
            revenues = model.Revenues,
            orderCounts = model.OrderCounts,
            summary = new
            {
                totalRevenue = model.TotalRevenue,
                totalOrders = model.TotalOrders,
                averageOrderValue = model.AverageOrderValue
            }
        };

        return Json(chartData, JsonRequestBehavior.AllowGet);
    }

    // Hàm xử lý chính: Tạo báo cáo
    private ReportViewModel GenerateReport(DateTime start, DateTime end, string reportType)
    {
        // 1. Lấy dữ liệu thô từ DB (Chỉ lấy những trường cần thiết để tối ưu)
        var ordersQuery = db.DonHangs
            .Where(d => d.NgayDat >= start && d.NgayDat <= end)
            .Include(d => d.KhachHang)
            .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham.DanhMuc)); // Include sâu để lấy danh mục

        // Thực thi query lấy về RAM để xử lý (tránh lỗi LINQ to Entities)
        var orders = ordersQuery.ToList();

        // Lọc đơn hàng thành công
        var completedOrders = orders.Where(d => d.TrangThai == "Hoàn thành").ToList();

        // 2. Tính toán các chỉ số tổng hợp
        var model = new ReportViewModel
        {
            FromDate = start,
            ToDate = end,
            TotalRevenue = completedOrders.Sum(x => x.ThanhTien),
            TotalOrders = completedOrders.Count,
            // Đếm số khách hàng duy nhất đã mua hàng thành công
            TotalCustomers = completedOrders.Select(o => o.KhachHangEmail).Distinct().Count(),
            // Tính tổng số lượng sản phẩm bán ra
            TotalProductsSold = completedOrders.Sum(o => o.ChiTietDonHangs.Sum(ct => (int?)ct.SoLuong) ?? 0),
            AverageOrderValue = completedOrders.Any() ? completedOrders.Average(x => x.ThanhTien) : 0,

            // Thống kê trạng thái
            PendingOrders = orders.Count(o => o.TrangThai == "Chờ xác nhận" || o.TrangThai == "Đang xử lý"), // Gộp chung cho gọn
            ProcessingOrders = orders.Count(o => o.TrangThai == "Đang giao"),
            CompletedOrders = completedOrders.Count,
            CancelledOrders = orders.Count(o => o.TrangThai == "Đã hủy")
        };

        // 3. Xử lý dữ liệu biểu đồ (Theo thời gian)
        PrepareChartData(model, completedOrders, reportType);

        // 4. Top sản phẩm bán chạy
        // Gom tất cả chi tiết đơn hàng lại thành 1 danh sách phẳng
        var allOrderDetails = completedOrders.SelectMany(o => o.ChiTietDonHangs).ToList();

        model.TopProducts = allOrderDetails
            .GroupBy(ct => new { ct.MaSP, ct.SanPham.TenSP, ct.SanPham.DanhMuc.TenDM })
            .Select(g => new TopProduct
            {
                ProductName = g.Key.TenSP,
                Category = g.Key.TenDM,
                QuantitySold = g.Sum(ct => ct.SoLuong),
                Revenue = g.Sum(ct => ct.SoLuong * ct.DonGia)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(10)
            .ToList();

        // 5. Thống kê theo danh mục
        model.CategoryStatistics = allOrderDetails
            .GroupBy(ct => ct.SanPham.DanhMuc.TenDM) // Group theo tên danh mục
            .Select(g => new CategoryStats
            {
                CategoryName = g.Key,
                ProductCount = g.Select(ct => ct.MaSP).Distinct().Count(),
                QuantitySold = g.Sum(ct => ct.SoLuong),
                Revenue = g.Sum(ct => ct.SoLuong * ct.DonGia)
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        // 6. Thống kê khách hàng VIP
        model.CustomerStatistics = completedOrders
            .GroupBy(o => new { o.KhachHangEmail, o.KhachHang.HoTen })
            .Select(g => new CustomerStats
            {
                CustomerName = g.Key.HoTen,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.ThanhTien),
                LastOrderDate = g.Max(o => o.NgayDat)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(15)
            .ToList();

        return model;
    }

    // Hàm phụ trợ: Xử lý biểu đồ
    private void PrepareChartData(ReportViewModel model, List<DonHang> orders, string reportType)
    {
        model.Labels = new List<string>();
        model.Revenues = new List<decimal>();
        model.OrderCounts = new List<int>();

        switch (reportType)
        {
            case "weekly":
                var weeklyGroups = orders
                    .GroupBy(d => new { Year = d.NgayDat.Year, Week = GetIso8601WeekOfYear(d.NgayDat) })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Week)
                    .ToList();

                foreach (var g in weeklyGroups)
                {
                    model.Labels.Add($"Tuần {g.Key.Week}/{g.Key.Year}");
                    model.Revenues.Add(g.Sum(o => o.ThanhTien));
                    model.OrderCounts.Add(g.Count());
                }
                break;

            case "monthly":
                var monthlyGroups = orders
                    .GroupBy(d => new { d.NgayDat.Year, d.NgayDat.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .ToList();

                foreach (var g in monthlyGroups)
                {
                    model.Labels.Add($"{g.Key.Month}/{g.Key.Year}");
                    model.Revenues.Add(g.Sum(o => o.ThanhTien));
                    model.OrderCounts.Add(g.Count());
                }
                break;

            default: // daily
                var dailyGroups = orders
                    .GroupBy(d => d.NgayDat.Date) // Group theo ngày (bỏ giờ phút giây)
                    .OrderBy(g => g.Key)
                    .ToList();

                foreach (var g in dailyGroups)
                {
                    model.Labels.Add(g.Key.ToString("dd/MM"));
                    model.Revenues.Add(g.Sum(o => o.ThanhTien));
                    model.OrderCounts.Add(g.Count());
                }
                break;
        }
    }

    // Hàm tính số tuần theo chuẩn ISO
    private int GetIso8601WeekOfYear(DateTime time)
    {
        DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
        {
            time = time.AddDays(3);
        }
        return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }

    // Placeholder cho Export 
    public ActionResult ExportReport(DateTime? fromDate, DateTime? toDate, string reportType = "daily", string format = "excel")
    {
        var start = fromDate ?? DateTime.Now.AddDays(-30).Date;
        var end = toDate ?? DateTime.Now.Date;
        var queryEnd = end.AddDays(1).AddSeconds(-1);

        var model = GenerateReport(start, queryEnd, reportType);

        if (format == "excel")
        {
            return ExportToExcel(model, start, end);
        }
        return Content("Chức năng PDF đang phát triển");
    }

    
    private ActionResult ExportToExcel(ReportViewModel model, DateTime start, DateTime end)
    {
        // Tạo stream trong bộ nhớ để lưu file
        using (var stream = new MemoryStream())
        {
            // Khởi tạo ExcelPackage
            using (var package = new ExcelPackage(stream))
            {
                // --- SHEET 1: TỔNG QUAN ---
                var ws = package.Workbook.Worksheets.Add("Tổng Quan");

                // 1. Tiêu đề báo cáo
                ws.Cells["A1:E1"].Merge = true;
                ws.Cells["A1"].Value = "BÁO CÁO DOANH THU";
                ws.Cells["A1"].Style.Font.Size = 16;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["A2:E2"].Merge = true;
                ws.Cells["A2"].Value = $"Thời gian: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";
                ws.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // 2. Số liệu tổng hợp (Header)
                ws.Cells["A4"].Value = "Tổng doanh thu";
                ws.Cells["B4"].Value = "Tổng đơn hàng";
                ws.Cells["C4"].Value = "Đã bán (SP)";
                ws.Cells["D4"].Value = "Khách hàng";
                ws.Cells["E4"].Value = "Giá trị TB/Đơn";
                ws.Cells["A4:E4"].Style.Font.Bold = true;
                ws.Cells["A4:E4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A4:E4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // 2. Số liệu tổng hợp (Value)
                ws.Cells["A5"].Value = model.TotalRevenue;
                ws.Cells["A5"].Style.Numberformat.Format = "#,##0"; // Định dạng số tiền
                ws.Cells["B5"].Value = model.TotalOrders;
                ws.Cells["C5"].Value = model.TotalProductsSold;
                ws.Cells["D5"].Value = model.TotalCustomers;
                ws.Cells["E5"].Value = model.AverageOrderValue;
                ws.Cells["E5"].Style.Numberformat.Format = "#,##0";

                // 3. Bảng chi tiết theo thời gian (Daily/Weekly/Monthly)
                ws.Cells["A8"].Value = "CHI TIẾT THEO THỜI GIAN";
                ws.Cells["A8"].Style.Font.Bold = true;

                ws.Cells["A9"].Value = "Thời gian";
                ws.Cells["B9"].Value = "Số đơn hàng";
                ws.Cells["C9"].Value = "Doanh thu";
                ws.Cells["A9:C9"].Style.Font.Bold = true;
                ws.Cells["A9:C9"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Đổ dữ liệu từ list Labels/Revenues/OrderCounts
                int row = 10;
                for (int i = 0; i < model.Labels.Count; i++)
                {
                    ws.Cells[row, 1].Value = model.Labels[i];
                    ws.Cells[row, 2].Value = model.OrderCounts[i];
                    ws.Cells[row, 3].Value = model.Revenues[i];
                    ws.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                    row++;
                }

                ws.Cells.AutoFitColumns(); // Tự động giãn cột cho đẹp

                // --- SHEET 2: TOP SẢN PHẨM ---
                var ws2 = package.Workbook.Worksheets.Add("Top Sản Phẩm");

                ws2.Cells["A1"].Value = "TOP SẢN PHẨM BÁN CHẠY";
                ws2.Cells["A1:D1"].Merge = true;
                ws2.Cells["A1"].Style.Font.Bold = true;
                ws2.Cells["A1"].Style.Font.Size = 14;

                ws2.Cells["A3"].Value = "Tên Sản Phẩm";
                ws2.Cells["B3"].Value = "Danh Mục";
                ws2.Cells["C3"].Value = "Số Lượng Bán";
                ws2.Cells["D3"].Value = "Doanh Thu";
                ws2.Cells["A3:D3"].Style.Font.Bold = true;
                ws2.Cells["A3:D3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws2.Cells["A3:D3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

                int row2 = 4;
                foreach (var item in model.TopProducts)
                {
                    ws2.Cells[row2, 1].Value = item.ProductName;
                    ws2.Cells[row2, 2].Value = item.Category;
                    ws2.Cells[row2, 3].Value = item.QuantitySold;
                    ws2.Cells[row2, 4].Value = item.Revenue;
                    ws2.Cells[row2, 4].Style.Numberformat.Format = "#,##0";
                    row2++;
                }
                ws2.Cells.AutoFitColumns();

                // --- LƯU FILE ---
                package.Save();
            }

            // Reset stream về đầu để đọc
            stream.Position = 0;
            string excelName = $"BaoCaoDoanhThu_{start:yyyyMMdd}_{end:yyyyMMdd}.xlsx";

            // Trả về file Excel cho người dùng tải xuống
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
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