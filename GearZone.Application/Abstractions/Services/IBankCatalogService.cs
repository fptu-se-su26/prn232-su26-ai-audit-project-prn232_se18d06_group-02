using GearZone.Application.Features.Seller.Dtos;

namespace GearZone.Application.Abstractions.Services;

public interface IBankCatalogService
{
    IReadOnlyList<BankOptionDto> GetSupportedBanks();
    BankOptionDto? FindByName(string bankName);
}
