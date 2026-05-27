using System;
using System.Collections.Generic;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminDashboardDto
    {
        public DashboardKpis KpiCards { get; set; } = new();
        public List<ChartDataPoint> RevenueOverview { get; set; } = new();
        public List<CategoryRevenueDto> RevenueByCategory { get; set; } = new();
        public List<OrderStatusBreakdownDto> OrderStatusBreakdown { get; set; } = new();
        public List<DashboardStoreDto> TopStores { get; set; } = new();
        public List<ChartDataPoint> UserGrowth { get; set; } = new();
    }

    public class DashboardKpis
    {
        public decimal GrossRevenue { get; set; }
        public decimal RevenueGrowth { get; set; } // Percentage
        public int TotalOrders { get; set; }
        public decimal OrderGrowth { get; set; }
        public int ActiveStores { get; set; }
        public decimal StoreGrowth { get; set; }
        public int NewUsers { get; set; }
        public decimal UserGrowth { get; set; }
        public decimal DisputeRate { get; set; } // Static/Prototype for now
        public decimal DisputeGrowth { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal? SecondaryValue { get; set; } // e.g. Net Revenue or Total Users
    }

    public class CategoryRevenueDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }

    public class OrderStatusBreakdownDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public string ColorClass { get; set; } = string.Empty;
    }

    public class DashboardStoreDto
    {
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
        public double Rating { get; set; }
        public decimal Commission { get; set; }
        public decimal Growth { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }

    public class DashboardQuery
    {
        public string Period { get; set; } = "month"; // today, week, month, year, custom
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public (DateTime Start, DateTime End) GetDateRange()
        {
            var now = DateTime.UtcNow;
            if (Period == "custom" && StartDate.HasValue && EndDate.HasValue)
                return (StartDate.Value, EndDate.Value);

            return Period switch
            {
                "today" => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
                "week" => (now.Date.AddDays(-(int)now.DayOfWeek), now.Date.AddDays(1).AddTicks(-1)),
                "year" => (new DateTime(now.Year, 1, 1), now.Date.AddDays(1).AddTicks(-1)),
                _ => (new DateTime(now.Year, now.Month, 1), now.Date.AddDays(1).AddTicks(-1)) // Month as default
            };
        }
        
        public (DateTime Start, DateTime End) GetPreviousDateRange()
        {
            var current = GetDateRange();
            var duration = current.End - current.Start;
            return (current.Start - duration, current.Start.AddTicks(-1));
        }
    }
}
