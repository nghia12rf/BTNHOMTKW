using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTNHOMTKW.Models
{
    public class ReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Dữ liệu tổng hợp
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProductsSold { get; set; }
        public decimal AverageOrderValue { get; set; }

        // Thống kê trạng thái đơn hàng
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Dữ liệu cho biểu đồ
        public List<string> Labels { get; set; }
        public List<decimal> Revenues { get; set; }
        public List<int> OrderCounts { get; set; }

        // Top sản phẩm bán chạy
        public List<TopProduct> TopProducts { get; set; }

        // Thống kê theo danh mục
        public List<CategoryStats> CategoryStatistics { get; set; }

        // Thống kê khách hàng
        public List<CustomerStats> CustomerStatistics { get; set; }
    }

    public class TopProduct
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public string Category { get; set; }
    }

    public class CategoryStats
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CustomerStats
    {
        public string CustomerName { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime LastOrderDate { get; set; }
    }
}