using GearZone.Domain.Enums;
using System;
using System.Text.Json.Serialization;

namespace GearZone.Application.Features.User.Dtos;

public class UserAddressDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public AddressType AddressType { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateUserAddressDto
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? Ward { get; set; }
    public string? District { get; set; }
    public string? Province { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AddressType AddressType { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateUserAddressDto : CreateUserAddressDto
{
    public Guid Id { get; set; }
}
