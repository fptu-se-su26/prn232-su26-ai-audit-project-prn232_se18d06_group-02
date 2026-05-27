using System;

namespace GearZone.Application.Features.Admin.Dtos;

public class StoreApplicationStatsDto
{
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
}
