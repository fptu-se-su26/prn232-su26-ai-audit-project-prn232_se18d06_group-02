namespace GearZone.Application.Features.Map.Dtos;

public class AddressDetailDto
{
    public string FullAddress { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}
