using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;

namespace GearZone.Application.Features.Seller;

public class BankCatalogService : IBankCatalogService
{
    private static readonly IReadOnlyList<BankOptionDto> SupportedBanks = new List<BankOptionDto>
    {
        new() { Name = "Vietcombank", ShortName = "VCB", Bin = "970436" },
        new() { Name = "BIDV", ShortName = "BIDV", Bin = "970418" },
        new() { Name = "VietinBank", ShortName = "CTG", Bin = "970415" },
        new() { Name = "Agribank", ShortName = "Agribank", Bin = "970405" },
        new() { Name = "Techcombank", ShortName = "TCB", Bin = "970407" },
        new() { Name = "VPBank", ShortName = "VPBank", Bin = "970432" },
        new() { Name = "MBBank", ShortName = "MB", Bin = "970422" },
        new() { Name = "ACB", ShortName = "ACB", Bin = "970416" },
        new() { Name = "TPBank", ShortName = "TPB", Bin = "970423" },
        new() { Name = "Sacombank", ShortName = "STB", Bin = "970403" },
        new() { Name = "HDBank", ShortName = "HDB", Bin = "970437" },
        new() { Name = "SHB", ShortName = "SHB", Bin = "970443" },
        new() { Name = "MSB", ShortName = "MSB", Bin = "970426" },
        new() { Name = "OCB", ShortName = "OCB", Bin = "970448" },
        new() { Name = "LienVietPostBank", ShortName = "LPB", Bin = "970449" }
    };

    public IReadOnlyList<BankOptionDto> GetSupportedBanks()
    {
        return SupportedBanks;
    }

    public BankOptionDto? FindByName(string bankName)
    {
        if (string.IsNullOrWhiteSpace(bankName))
        {
            return null;
        }

        return SupportedBanks.FirstOrDefault(b =>
            string.Equals(b.Name, bankName.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
