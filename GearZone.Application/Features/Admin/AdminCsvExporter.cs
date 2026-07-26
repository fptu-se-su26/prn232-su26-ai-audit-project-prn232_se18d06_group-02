using System.Globalization;
using System.Text;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Features.Admin;

internal static class AdminCsvExporter
{
    private const string ContentType = "text/csv; charset=utf-8";

    public static AdminCsvFileDto ExportOrders(IEnumerable<AdminOrderDto> orders, DateTime generatedAtUtc)
    {
        var csv = new CsvBuilder();
        csv.AddRow(
            "Order ID",
            "Order Code",
            "Customer",
            "Receiver",
            "Receiver Phone",
            "Shipping Address",
            "Grand Total (VND)",
            "Payment Status",
            "Paid At",
            "Created At");

        foreach (var order in orders)
        {
            csv.AddRow(
                order.Id.ToString(),
                order.OrderCode.ToString(CultureInfo.InvariantCulture),
                order.CustomerName,
                order.ReceiverName,
                order.ReceiverPhone,
                order.ShippingAddress,
                order.GrandTotal.ToString("0.##", CultureInfo.InvariantCulture),
                order.PaidAt.HasValue ? "Paid" : "Unpaid",
                FormatDate(order.PaidAt),
                FormatDate(order.CreatedAt));
        }

        return csv.Build($"admin-orders-{generatedAtUtc:yyyyMMdd-HHmmss}.csv");
    }

    public static AdminCsvFileDto ExportProducts(IEnumerable<AdminProductDto> products, DateTime generatedAtUtc)
    {
        var csv = new CsvBuilder();
        csv.AddRow(
            "Product ID",
            "Name",
            "SKU",
            "Store",
            "Category",
            "Price (VND)",
            "Stock",
            "Status",
            "Created At",
            "Thumbnail URL");

        foreach (var product in products)
        {
            csv.AddRow(
                product.Id.ToString(),
                product.Name,
                product.Sku,
                product.StoreName,
                product.Category,
                product.Price.ToString("0.##", CultureInfo.InvariantCulture),
                product.Stock.ToString(CultureInfo.InvariantCulture),
                product.Status,
                FormatDate(product.CreatedAt),
                product.ThumbnailUrl);
        }

        return csv.Build($"admin-products-{generatedAtUtc:yyyyMMdd-HHmmss}.csv");
    }

    private static string FormatDate(DateTime? value) =>
        value?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;

    private sealed class CsvBuilder
    {
        private readonly StringBuilder _content = new();

        public void AddRow(params string?[] values)
        {
            _content.AppendLine(string.Join(",", values.Select(Escape)));
        }

        public AdminCsvFileDto Build(string fileName)
        {
            var body = Encoding.UTF8.GetBytes(_content.ToString());
            var preamble = Encoding.UTF8.GetPreamble();
            var content = new byte[preamble.Length + body.Length];
            Buffer.BlockCopy(preamble, 0, content, 0, preamble.Length);
            Buffer.BlockCopy(body, 0, content, preamble.Length, body.Length);
            return new AdminCsvFileDto(content, ContentType, fileName);
        }

        private static string Escape(string? value)
        {
            var safeValue = ProtectFromFormulaInjection(value ?? string.Empty);
            if (!safeValue.Contains(',') &&
                !safeValue.Contains('"') &&
                !safeValue.Contains('\r') &&
                !safeValue.Contains('\n'))
            {
                return safeValue;
            }

            return $"\"{safeValue.Replace("\"", "\"\"")}\"";
        }

        private static string ProtectFromFormulaInjection(string value)
        {
            if (value.Length == 0)
            {
                return value;
            }

            var firstNonWhitespace = value.AsSpan().TrimStart();
            if (firstNonWhitespace.Length == 0)
            {
                return value;
            }

            return firstNonWhitespace[0] is '=' or '+' or '-' or '@' or '\t'
                ? $"'{value}"
                : value;
        }
    }
}
