using System.Globalization;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Features.Admin;

public sealed record ResolvedAdminReportPeriod(
    DateTime StartUtc,
    DateTime EndExclusiveUtc,
    DateTime PreviousStartUtc,
    DateTime PreviousEndExclusiveUtc,
    DateTime StartLocal,
    DateTime EndLocal,
    DateTime PreviousStartLocal,
    DateTime PreviousEndLocal,
    string Granularity,
    TimeZoneInfo TimeZone)
{
    public AdminReportPeriodDto ToDto() => new()
    {
        Start = StartLocal,
        End = EndLocal,
        PreviousStart = PreviousStartLocal,
        PreviousEnd = PreviousEndLocal,
        Label = $"{StartLocal:dd MMM yyyy} - {EndLocal:dd MMM yyyy}",
        Granularity = Granularity,
        TimeZone = TimeZone.Id
    };
}

public static class AdminReportPeriodResolver
{
    public const int MaximumRangeDays = 366;
    public const string DefaultTimeZone = "Asia/Ho_Chi_Minh";
    private static readonly string[] AllowedRanges = ["today", "7d", "30d", "thismonth", "lastmonth", "custom"];
    private static readonly string[] AllowedGranularities = ["day", "week", "month"];

    public static ResolvedAdminReportPeriod Resolve(
        AdminReportQueryDto query,
        DateTime? utcNow = null,
        string timeZoneId = DefaultTimeZone)
    {
        ArgumentNullException.ThrowIfNull(query);

        var zone = FindTimeZone(timeZoneId);
        var nowUtc = DateTime.SpecifyKind(utcNow ?? DateTime.UtcNow, DateTimeKind.Utc);
        var localToday = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, zone).Date;
        var range = string.IsNullOrWhiteSpace(query.Range) ? "30d" : query.Range.Trim().ToLowerInvariant();
        if (!AllowedRanges.Contains(range))
            throw new ArgumentException($"Unsupported range '{query.Range}'.");

        DateTime start;
        DateTime end;
        switch (range)
        {
            case "today":
                start = end = localToday;
                break;
            case "7d":
                end = localToday;
                start = end.AddDays(-6);
                break;
            case "thismonth":
                start = new DateTime(localToday.Year, localToday.Month, 1);
                end = localToday;
                break;
            case "lastmonth":
                var firstThisMonth = new DateTime(localToday.Year, localToday.Month, 1);
                end = firstThisMonth.AddDays(-1);
                start = new DateTime(end.Year, end.Month, 1);
                break;
            case "custom":
                if (!query.From.HasValue || !query.To.HasValue)
                    throw new ArgumentException("Custom range requires both from and to dates.");
                start = query.From.Value.Date;
                end = query.To.Value.Date;
                break;
            default:
                end = localToday;
                start = end.AddDays(-29);
                break;
        }

        if (start > end)
            throw new ArgumentException("From date cannot be after to date.");

        var dayCount = (end - start).Days + 1;
        if (dayCount > MaximumRangeDays)
            throw new ArgumentException($"Report range cannot exceed {MaximumRangeDays} days.");

        var granularity = string.IsNullOrWhiteSpace(query.Granularity)
            ? dayCount <= 31 ? "day" : dayCount <= 180 ? "week" : "month"
            : query.Granularity.Trim().ToLowerInvariant();
        if (!AllowedGranularities.Contains(granularity))
            throw new ArgumentException($"Unsupported granularity '{query.Granularity}'.");

        var previousEnd = start.AddDays(-1);
        var previousStart = previousEnd.AddDays(-(dayCount - 1));
        return new ResolvedAdminReportPeriod(
            ToUtc(start, zone),
            ToUtc(end.AddDays(1), zone),
            ToUtc(previousStart, zone),
            ToUtc(previousEnd.AddDays(1), zone),
            start,
            end,
            previousStart,
            previousEnd,
            granularity,
            zone);
    }

    public static List<(DateTime StartLocal, DateTime EndExclusiveLocal, string Label)> BuildBuckets(
        ResolvedAdminReportPeriod period)
    {
        var buckets = new List<(DateTime, DateTime, string)>();
        var cursor = period.StartLocal;
        while (cursor <= period.EndLocal)
        {
            DateTime next;
            if (period.Granularity == "month")
                next = new DateTime(cursor.Year, cursor.Month, 1).AddMonths(1);
            else if (period.Granularity == "week")
                next = cursor.AddDays(7);
            else
                next = cursor.AddDays(1);

            var cappedNext = next > period.EndLocal.AddDays(1) ? period.EndLocal.AddDays(1) : next;
            var label = period.Granularity switch
            {
                "month" => cursor.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                "week" => $"{cursor:dd MMM}-{cappedNext.AddDays(-1):dd MMM}",
                _ => cursor.ToString("dd MMM", CultureInfo.InvariantCulture)
            };
            buckets.Add((cursor, cappedNext, label));
            cursor = cappedNext;
        }
        return buckets;
    }

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

    private static DateTime ToUtc(DateTime local, TimeZoneInfo zone) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), zone);

    private static TimeZoneInfo FindTimeZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException) when (id == DefaultTimeZone)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        }
    }
}
