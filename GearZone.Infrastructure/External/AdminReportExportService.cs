using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GearZone.Infrastructure.External;

public sealed class AdminReportExportService : IAdminReportExportService
{
    private readonly IAdminReportService _reports;

    public AdminReportExportService(IAdminReportService reports)
    {
        _reports = reports;
    }

    public async Task<AdminReportFileDto> ExportAsync(
        string reportType,
        string format,
        AdminReportQueryDto query,
        CancellationToken ct = default)
    {
        reportType = NormalizeReportType(reportType);
        format = NormalizeFormat(format);
        object report = reportType switch
        {
            "overview" => await _reports.GetOverviewAsync(query, ct),
            "orders" => await _reports.GetOrdersAsync(query, ct),
            _ => await _reports.GetSellersAsync(query, exportAll: true, ct)
        };
        var period = report switch
        {
            AdminOverviewReportDto x => x.Period,
            AdminOrderReportDto x => x.Period,
            AdminSellerReportDto x => x.Period,
            _ => throw new InvalidOperationException("Unknown report payload.")
        };
        var baseName = $"admin-{reportType}-report-{period.Start:yyyyMMdd}-{period.End:yyyyMMdd}";

        return format switch
        {
            "csv" => new AdminReportFileDto
            {
                Content = WithUtf8Bom(BuildCsv(report)),
                ContentType = "text/csv; charset=utf-8",
                FileName = baseName + ".csv"
            },
            "xlsx" => new AdminReportFileDto
            {
                Content = BuildWorkbook(report),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileName = baseName + ".xlsx"
            },
            _ => new AdminReportFileDto
            {
                Content = BuildPdf(reportType, report, period),
                ContentType = "application/pdf",
                FileName = baseName + ".pdf"
            }
        };
    }

    private static string BuildCsv(object report)
    {
        var sb = new StringBuilder();
        switch (report)
        {
            case AdminOverviewReportDto x:
                AppendPeriod(sb, x.Period);
                sb.AppendLine("Metric,Current,Previous,ChangePct");
                AppendMetric(sb, "Paid GMV", x.PaidGmv);
                AppendMetric(sb, "Platform Commission", x.PlatformCommission);
                AppendMetric(sb, "Seller Net Amount", x.SellerNetAmount);
                AppendMetric(sb, "Orders", x.Orders);
                AppendMetric(sb, "Units Sold", x.UnitsSold);
                AppendMetric(sb, "Average Order Value", x.AverageOrderValue);
                AppendMetric(sb, "Unique Buyers", x.UniqueBuyers);
                AppendMetric(sb, "Active Sellers", x.ActiveSellers);
                sb.AppendLine().AppendLine("Trend").AppendLine("Date,Label,PaidGmv,Commission,Orders");
                foreach (var p in x.Trend)
                    sb.AppendLine($"{p.Date:yyyy-MM-dd},{Csv(p.Label)},{Invariant(p.Gmv)},{Invariant(p.Commission)},{p.Orders}");
                sb.AppendLine().AppendLine("Category,Revenue,Percentage");
                foreach (var row in x.RevenueByCategory)
                    sb.AppendLine($"{Csv(row.CategoryName)},{Invariant(row.Revenue)},{Invariant(row.Percentage)}");
                break;

            case AdminOrderReportDto x:
                AppendPeriod(sb, x.Period);
                sb.AppendLine("Metric,Current,Previous,ChangePct");
                AppendMetric(sb, "Orders", x.Orders);
                AppendMetric(sb, "SubOrders", x.SubOrders);
                AppendMetric(sb, "Paid SubOrders", x.PaidSubOrders);
                sb.AppendLine($"Completion Rate,{Invariant(x.CompletionRate)},,");
                sb.AppendLine($"Cancellation Rate,{Invariant(x.CancellationRate)},,");
                sb.AppendLine($"Rejection Rate,{Invariant(x.RejectionRate)},,");
                sb.AppendLine($"Refund Rate,{Invariant(x.RefundRate)},,");
                sb.AppendLine($"Average Fulfillment Hours,{Invariant(x.AverageFulfillmentHours)},,");
                sb.AppendLine().AppendLine("Status,Count,Percentage");
                foreach (var row in x.StatusBreakdown)
                    sb.AppendLine($"{Csv(row.Status)},{row.Count},{Invariant(row.Percentage)}");
                sb.AppendLine().AppendLine("Payment Method,Orders,Amount");
                foreach (var row in x.PaymentMethods)
                    sb.AppendLine($"{Csv(row.Method)},{row.Count},{Invariant(row.Amount)}");
                sb.AppendLine().AppendLine("Top Orders").AppendLine("OrderCode,Customer,PaidGmv,Stores,CreatedAt,PaidAt");
                foreach (var row in x.HighValueOrders)
                    sb.AppendLine($"{row.OrderCode},{Csv(row.CustomerName)},{Invariant(row.PaidGmv)},{row.StoreCount},{row.CreatedAt:O},{row.PaidAt:O}");
                break;

            case AdminSellerReportDto x:
                AppendPeriod(sb, x.Period);
                sb.AppendLine("Metric,Current,Previous,ChangePct");
                AppendMetric(sb, "Active Sellers", x.ActiveSellers);
                AppendMetric(sb, "New Approved Sellers", x.NewApprovedSellers);
                AppendMetric(sb, "Paid GMV", x.PaidGmv);
                AppendMetric(sb, "Platform Commission", x.PlatformCommission);
                AppendMetric(sb, "Seller Net Amount", x.SellerNetAmount);
                sb.AppendLine().AppendLine("Store,Status,PaidGmv,PreviousGmv,GrowthPct,Commission,SellerNet,Orders,Units,AOV,CancellationRate,RefundRate,Rating");
                foreach (var row in x.Sellers.Items)
                    sb.AppendLine(string.Join(",",
                        Csv(row.StoreName), Csv(row.Status), Invariant(row.PaidGmv), Invariant(row.PreviousGmv),
                        Invariant(row.GrowthPct), Invariant(row.Commission), Invariant(row.SellerNetAmount), row.Orders,
                        row.Units, Invariant(row.AverageOrderValue), Invariant(row.CancellationRate),
                        Invariant(row.RefundRate), Invariant(row.AverageRating)));
                break;
        }
        return sb.ToString();
    }

    private static byte[] BuildWorkbook(object report)
    {
        using var workbook = new XLWorkbook();
        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = "GearZone Admin Business Intelligence";
        summary.Range(1, 1, 1, 4).Merge().Style.Font.SetBold().Font.SetFontSize(16);

        switch (report)
        {
            case AdminOverviewReportDto x:
                WritePeriod(summary, x.Period);
                WriteMetrics(summary, 5,
                    ("Paid GMV", x.PaidGmv), ("Platform Commission", x.PlatformCommission),
                    ("Seller Net Amount", x.SellerNetAmount), ("Orders", x.Orders),
                    ("Units Sold", x.UnitsSold), ("Average Order Value", x.AverageOrderValue),
                    ("Unique Buyers", x.UniqueBuyers), ("Active Sellers", x.ActiveSellers));
                var trend = workbook.Worksheets.Add("Trend");
                WriteTable(trend, ["Date", "Label", "Paid GMV", "Commission", "Orders"],
                    x.Trend.Select(p => new object?[] { p.Date, p.Label, p.Gmv, p.Commission, p.Orders }));
                var categories = workbook.Worksheets.Add("Details");
                WriteTable(categories, ["Category", "Revenue", "Percentage"],
                    x.RevenueByCategory.Select(p => new object?[] { p.CategoryName, p.Revenue, p.Percentage / 100m }));
                categories.Column(3).Style.NumberFormat.Format = "0.00%";
                break;

            case AdminOrderReportDto x:
                WritePeriod(summary, x.Period);
                WriteMetrics(summary, 5, ("Orders", x.Orders), ("SubOrders", x.SubOrders), ("Paid SubOrders", x.PaidSubOrders));
                var operations = workbook.Worksheets.Add("Trend");
                WriteTable(operations, ["Date", "Label", "Orders", "SubOrders", "Paid GMV"],
                    x.Trend.Select(p => new object?[] { p.Date, p.Label, p.Orders, p.SubOrders, p.Gmv }));
                var orderDetails = workbook.Worksheets.Add("Details");
                WriteTable(orderDetails, ["Order Code", "Customer", "Paid GMV", "Stores", "Created At", "Paid At"],
                    x.HighValueOrders.Select(p => new object?[] { p.OrderCode, p.CustomerName, p.PaidGmv, p.StoreCount, p.CreatedAt, p.PaidAt }));
                break;

            case AdminSellerReportDto x:
                WritePeriod(summary, x.Period);
                WriteMetrics(summary, 5,
                    ("Active Sellers", x.ActiveSellers), ("New Approved Sellers", x.NewApprovedSellers),
                    ("Paid GMV", x.PaidGmv), ("Platform Commission", x.PlatformCommission),
                    ("Seller Net Amount", x.SellerNetAmount));
                var sellers = workbook.Worksheets.Add("Details");
                WriteTable(sellers,
                    ["Store", "Status", "Paid GMV", "Previous GMV", "Growth %", "Commission", "Seller Net", "Orders", "Units", "AOV", "Cancellation %", "Refund %", "Rating"],
                    x.Sellers.Items.Select(p => new object?[]
                    {
                        p.StoreName, p.Status, p.PaidGmv, p.PreviousGmv, p.GrowthPct.HasValue ? p.GrowthPct / 100m : null,
                        p.Commission, p.SellerNetAmount, p.Orders, p.Units, p.AverageOrderValue,
                        p.CancellationRate / 100m, p.RefundRate / 100m, p.AverageRating
                    }));
                sellers.Columns(5, 5).Style.NumberFormat.Format = "0.00%";
                sellers.Columns(11, 12).Style.NumberFormat.Format = "0.00%";
                break;
        }

        foreach (var sheet in workbook.Worksheets)
        {
            sheet.Columns().AdjustToContents(8, 45);
            sheet.SheetView.FreezeRows(sheet.Name == "Summary" ? 1 : 1);
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] BuildPdf(string reportType, object report, AdminReportPeriodDto period)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().Column(column =>
                {
                    column.Item().Text($"GearZone Admin - {CultureInfo.InvariantCulture.TextInfo.ToTitleCase(reportType)} Report").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"{period.Label} | {period.TimeZone} | Grouped by {period.Granularity}").FontColor(Colors.Grey.Darken1);
                });
                page.Content().PaddingVertical(12).Column(column =>
                {
                    switch (report)
                    {
                        case AdminOverviewReportDto x:
                            PdfMetrics(column,
                                ("Paid GMV", Money(x.PaidGmv.Current)), ("Commission", Money(x.PlatformCommission.Current)),
                                ("Seller Net", Money(x.SellerNetAmount.Current)), ("Orders", Number(x.Orders.Current)),
                                ("Units", Number(x.UnitsSold.Current)), ("AOV", Money(x.AverageOrderValue.Current)),
                                ("Buyers", Number(x.UniqueBuyers.Current)), ("Active sellers", Number(x.ActiveSellers.Current)));
                            column.Item().PaddingTop(12).Text("Trend").Bold().FontSize(12);
                            PdfBarChart(column, x.Trend.Select(p => p.Gmv));
                            PdfTable(column, ["Period", "Paid GMV", "Commission", "Orders"],
                                x.Trend.Select(p => new[] { p.Label, Money(p.Gmv), Money(p.Commission), p.Orders.ToString("N0") }));
                            column.Item().PaddingTop(12).Text("Revenue by category").Bold().FontSize(12);
                            PdfTable(column, ["Category", "Revenue", "Share"],
                                x.RevenueByCategory.Select(p => new[] { p.CategoryName, Money(p.Revenue), $"{p.Percentage:N2}%" }));
                            break;
                        case AdminOrderReportDto x:
                            PdfMetrics(column,
                                ("Orders", Number(x.Orders.Current)), ("SubOrders", Number(x.SubOrders.Current)),
                                ("Paid SubOrders", Number(x.PaidSubOrders.Current)), ("Completed", $"{x.CompletionRate:N2}%"),
                                ("Cancelled", $"{x.CancellationRate:N2}%"), ("Rejected", $"{x.RejectionRate:N2}%"),
                                ("Refunded", $"{x.RefundRate:N2}%"), ("Avg fulfillment", x.AverageFulfillmentHours.HasValue ? $"{x.AverageFulfillmentHours:N1}h" : "-") );
                            column.Item().PaddingTop(12).Text("Order trend").Bold().FontSize(12);
                            PdfBarChart(column, x.Trend.Select(p => (decimal)p.Orders), Colors.Purple.Medium);
                            column.Item().PaddingTop(12).Text("Status breakdown").Bold().FontSize(12);
                            PdfTable(column, ["Status", "Count", "Share"],
                                x.StatusBreakdown.Select(p => new[] { p.Status, p.Count.ToString("N0"), $"{p.Percentage:N2}%" }));
                            column.Item().PaddingTop(12).Text("High value orders").Bold().FontSize(12);
                            PdfTable(column, ["Order", "Customer", "Paid GMV", "Stores", "Created"],
                                x.HighValueOrders.Select(p => new[] { p.OrderCode.ToString(), p.CustomerName, Money(p.PaidGmv), p.StoreCount.ToString(), p.CreatedAt.ToString("dd MMM yyyy") }));
                            break;
                        case AdminSellerReportDto x:
                            PdfMetrics(column,
                                ("Active sellers", Number(x.ActiveSellers.Current)), ("New sellers", Number(x.NewApprovedSellers.Current)),
                                ("Paid GMV", Money(x.PaidGmv.Current)), ("Commission", Money(x.PlatformCommission.Current)),
                                ("Seller Net", Money(x.SellerNetAmount.Current)));
                            column.Item().PaddingTop(12).Text("Seller performance").Bold().FontSize(12);
                            PdfTable(column,
                                ["Store", "Status", "Paid GMV", "Growth", "Orders", "Cancel", "Refund", "Rating"],
                                x.Sellers.Items.Select(p => new[]
                                {
                                    p.StoreName, p.Status, Money(p.PaidGmv), p.GrowthPct.HasValue ? $"{p.GrowthPct:N2}%" : "-",
                                    p.Orders.ToString("N0"), $"{p.CancellationRate:N2}%", $"{p.RefundRate:N2}%", p.AverageRating.ToString("N1")
                                }));
                            break;
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated by GearZone BI | Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void PdfMetrics(ColumnDescriptor column, params (string Label, string Value)[] metrics)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                for (var i = 0; i < metrics.Length; i++) columns.RelativeColumn();
            });
            foreach (var metric in metrics)
            {
                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(7).Column(cell =>
                {
                    cell.Item().Text(metric.Label).FontSize(7).FontColor(Colors.Grey.Darken1);
                    cell.Item().Text(metric.Value).Bold().FontSize(11);
                });
            }
        });
    }

    private static void PdfBarChart(
        ColumnDescriptor column,
        IEnumerable<decimal> source,
        string? color = null)
    {
        var values = source.TakeLast(60).ToList();
        if (values.Count == 0) return;
        var max = Math.Max(1m, values.Max());
        column.Item().Height(64).PaddingVertical(4).Row(row =>
        {
            foreach (var value in values)
            {
                var height = (float)Math.Max(1m, value / max * 56m);
                row.RelativeItem().PaddingHorizontal(1).AlignBottom().Height(height)
                    .Background(color ?? Colors.Blue.Medium);
            }
        });
    }

    private static void PdfTable(ColumnDescriptor column, string[] headers, IEnumerable<string[]> rows)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in headers) columns.RelativeColumn();
            });
            table.Header(header =>
            {
                foreach (var text in headers)
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text(text).FontColor(Colors.White).Bold();
            });
            foreach (var row in rows)
                foreach (var text in row)
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(text ?? string.Empty);
        });
    }

    private static void WritePeriod(IXLWorksheet sheet, AdminReportPeriodDto period)
    {
        sheet.Cell(3, 1).Value = "Period";
        sheet.Cell(3, 2).Value = period.Label;
        sheet.Cell(3, 3).Value = period.TimeZone;
        sheet.Cell(3, 4).Value = period.Granularity;
    }

    private static void WriteMetrics(IXLWorksheet sheet, int startRow, params (string Label, ComparisonMetricDto Metric)[] metrics)
    {
        WriteTable(sheet, ["Metric", "Current", "Previous", "Change %"], metrics.Select(x => new object?[]
        {
            x.Label, x.Metric.Current, x.Metric.Previous, x.Metric.ChangePct.HasValue ? x.Metric.ChangePct / 100m : null
        }), startRow);
        sheet.Column(4).Style.NumberFormat.Format = "0.00%";
    }

    private static void WriteTable(IXLWorksheet sheet, string[] headers, IEnumerable<object?[]> rows, int startRow = 1)
    {
        for (var column = 0; column < headers.Length; column++)
            sheet.Cell(startRow, column + 1).Value = headers[column];
        var header = sheet.Range(startRow, 1, startRow, headers.Length);
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("1A56DB");
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Font.Bold = true;

        var rowIndex = startRow + 1;
        foreach (var row in rows)
        {
            for (var column = 0; column < row.Length; column++)
            {
                var cell = sheet.Cell(rowIndex, column + 1);
                SetCellValue(cell, row[column]);
            }
            rowIndex++;
        }
        if (rowIndex > startRow + 1)
            sheet.Range(startRow, 1, rowIndex - 1, headers.Length).SetAutoFilter();
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null: cell.Value = Blank.Value; break;
            case DateTime date: cell.Value = date; cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm"; break;
            case decimal number: cell.Value = number; cell.Style.NumberFormat.Format = "#,##0.00"; break;
            case int number: cell.Value = number; break;
            case long number: cell.Value = number; break;
            default: cell.Value = value.ToString(); break;
        }
    }

    private static void AppendPeriod(StringBuilder sb, AdminReportPeriodDto period) =>
        sb.AppendLine($"Period,{Csv(period.Label)}").AppendLine($"Time Zone,{Csv(period.TimeZone)}").AppendLine($"Granularity,{period.Granularity}").AppendLine();

    private static void AppendMetric(StringBuilder sb, string name, ComparisonMetricDto metric) =>
        sb.AppendLine($"{Csv(name)},{Invariant(metric.Current)},{Invariant(metric.Previous)},{Invariant(metric.ChangePct)}");

    private static byte[] WithUtf8Bom(string content) =>
        Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();

    private static string Csv(string? value)
    {
        value ??= string.Empty;
        return value.IndexOfAny([',', '"', '\r', '\n']) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }

    private static string Invariant(decimal? value) => value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    private static string Money(decimal value) => value.ToString("N0", CultureInfo.InvariantCulture) + " VND";
    private static string Number(decimal value) => value.ToString("N0", CultureInfo.InvariantCulture);

    private static string NormalizeReportType(string value) => value.Trim().ToLowerInvariant() switch
    {
        "overview" => "overview",
        "orders" => "orders",
        "sellers" => "sellers",
        _ => throw new ArgumentException($"Unsupported report type '{value}'.")
    };

    private static string NormalizeFormat(string value) => value.Trim().ToLowerInvariant() switch
    {
        "csv" => "csv",
        "xlsx" => "xlsx",
        "pdf" => "pdf",
        _ => throw new ArgumentException($"Unsupported export format '{value}'.")
    };
}
