namespace GearZone.Application.Features.Admin.Dtos;

public sealed record AdminCsvFileDto(byte[] Content, string ContentType, string FileName);
