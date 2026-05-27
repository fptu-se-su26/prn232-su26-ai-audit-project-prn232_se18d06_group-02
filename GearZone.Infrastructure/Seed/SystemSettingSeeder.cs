using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Seed;

public static class SystemSettingSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>().HasData(
            // Payment Configuration
            new SystemSetting { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Key = "Payment_CommissionRate", Value = "0.05", DataType = SettingDataType.Number, GroupName = "Payment", Description = "Commission Rate (Decimal: 0.05 = 5%)" },
            new SystemSetting { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), Key = "Payment_MinimumPayout", Value = "50000", DataType = SettingDataType.Number, GroupName = "Payment", Description = "Minimum Payout (VND)" },
            new SystemSetting { Id = Guid.Parse("11111111-1111-1111-1111-111111111113"), Key = "Payment_OnlinePayments", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Payment", Description = "Allow credit cards & e-wallets" },
            new SystemSetting { Id = Guid.Parse("11111111-1111-1111-1111-111111111114"), Key = "Payment_CashOnDelivery", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Payment", Description = "Allow payment upon receipt" },
            new SystemSetting { Id = Guid.Parse("11111111-1111-1111-1111-111111111115"), Key = "Payment_WebhookSecret", Value = "", DataType = SettingDataType.String, GroupName = "Payment", Description = "Webhook signature secret" },

            // Store Configuration
            new SystemSetting { Id = Guid.Parse("22222222-2222-2222-2222-222222222221"), Key = "Store_NewStoreApproval", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Store", Description = "Manually approve new vendors" },
            new SystemSetting { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Key = "Store_IndividualSellers", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Store", Description = "Allow non-business entities to sell" },
            new SystemSetting { Id = Guid.Parse("22222222-2222-2222-2222-222222222223"), Key = "Store_DefaultStatus", Value = "Pending", DataType = SettingDataType.String, GroupName = "Store", Description = "Default Store Status (Active, Pending Review, Inactive)" },

            // Order Configuration
            new SystemSetting { Id = Guid.Parse("33333333-3333-3333-3333-333333333331"), Key = "Order_AutoCompleteDays", Value = "7", DataType = SettingDataType.Number, GroupName = "Order", Description = "Auto Complete (Days)" },
            new SystemSetting { Id = Guid.Parse("33333333-3333-3333-3333-333333333332"), Key = "Order_AutoCancelMinutes", Value = "30", DataType = SettingDataType.Number, GroupName = "Order", Description = "Auto Cancel (Minutes)" },
            new SystemSetting { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Key = "Order_BuyerCancellation", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Order", Description = "Allow buyers to cancel pending orders" },

            // Payout & Finance
            new SystemSetting { Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), Key = "Finance_ManualWithdraw", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Finance", Description = "Vendors must request payouts manually" },
            new SystemSetting { Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), Key = "Finance_HoldFunds", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Finance", Description = "Release funds only after order completion" },
            new SystemSetting { Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), Key = "Finance_PayoutDelayDays", Value = "7", DataType = SettingDataType.Number, GroupName = "Finance", Description = "Hold funds before payout (days)" },

            // Security & Compliance
            new SystemSetting { Id = Guid.Parse("55555555-5555-5555-5555-555555555551"), Key = "Security_KYCRequired", Value = "false", DataType = SettingDataType.Boolean, GroupName = "Security", Description = "Require sellers to upload ID documents" },
            new SystemSetting { Id = Guid.Parse("55555555-5555-5555-5555-555555555552"), Key = "Security_TaxCodeVerification", Value = "true", DataType = SettingDataType.Boolean, GroupName = "Security", Description = "Mandatory tax code input" }
        );
    }
}
