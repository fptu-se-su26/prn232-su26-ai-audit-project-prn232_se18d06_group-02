using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Tests;

public sealed class AdminReportPeriodResolverTests
{
    [Fact]
    public void Today_UsesVietnamCalendarDayAndUtcHalfOpenRange()
    {
        var period = AdminReportPeriodResolver.Resolve(
            new AdminReportQueryDto { Range = "today" },
            new DateTime(2026, 7, 18, 2, 30, 0, DateTimeKind.Utc));

        Assert.Equal(new DateTime(2026, 7, 18), period.StartLocal);
        Assert.Equal(new DateTime(2026, 7, 17, 17, 0, 0, DateTimeKind.Utc), period.StartUtc);
        Assert.Equal(new DateTime(2026, 7, 18, 17, 0, 0, DateTimeKind.Utc), period.EndExclusiveUtc);
        Assert.Equal(new DateTime(2026, 7, 17), period.PreviousStartLocal);
    }

    [Fact]
    public void LeapYearCustomRangeBuildsZeroFillBucketSkeleton()
    {
        var period = AdminReportPeriodResolver.Resolve(new AdminReportQueryDto
        {
            Range = "custom",
            From = new DateTime(2024, 2, 28),
            To = new DateTime(2024, 3, 1)
        });

        var buckets = AdminReportPeriodResolver.BuildBuckets(period);

        Assert.Equal(3, buckets.Count);
        Assert.Contains(buckets, x => x.StartLocal == new DateTime(2024, 2, 29));
        Assert.Equal(new DateTime(2024, 2, 25), period.PreviousStartLocal);
        Assert.Equal(new DateTime(2024, 2, 27), period.PreviousEndLocal);
    }

    [Fact]
    public void AutoGranularityUsesDayWeekAndMonthThresholds()
    {
        Assert.Equal("day", ResolveDays(31).Granularity);
        Assert.Equal("week", ResolveDays(32).Granularity);
        Assert.Equal("week", ResolveDays(180).Granularity);
        Assert.Equal("month", ResolveDays(181).Granularity);
    }

    [Fact]
    public void InvalidCustomRangesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => AdminReportPeriodResolver.Resolve(
            new AdminReportQueryDto { Range = "custom", From = new DateTime(2026, 1, 1) }));
        Assert.Throws<ArgumentException>(() => AdminReportPeriodResolver.Resolve(
            new AdminReportQueryDto { Range = "custom", From = new DateTime(2026, 2, 1), To = new DateTime(2026, 1, 1) }));
        Assert.Throws<ArgumentException>(() => AdminReportPeriodResolver.Resolve(
            new AdminReportQueryDto { Range = "custom", From = new DateTime(2025, 1, 1), To = new DateTime(2026, 1, 2) }));
    }

    private static ResolvedAdminReportPeriod ResolveDays(int days) =>
        AdminReportPeriodResolver.Resolve(new AdminReportQueryDto
        {
            Range = "custom",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 1).AddDays(days - 1)
        });
}
