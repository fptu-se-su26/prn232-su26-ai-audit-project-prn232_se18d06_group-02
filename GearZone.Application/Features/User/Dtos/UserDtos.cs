using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Features.User.Dtos;

public class UpdateProfileDto
{
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
}
