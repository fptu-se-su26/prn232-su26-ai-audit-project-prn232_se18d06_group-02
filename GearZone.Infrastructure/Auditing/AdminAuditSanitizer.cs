using GearZone.Application.Features.Admin.Dtos;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GearZone.Infrastructure.Auditing;

public sealed partial class AdminAuditSanitizer
{
    private static readonly IReadOnlyDictionary<string, HashSet<string>> AllowedFields =
        new Dictionary<string, HashSet<string>>(StringComparer.Ordinal)
        {
            ["ApplicationUser"] = Set("FullName", "Email", "UserName", "IsActive", "IsDeleted", "DeletedAt", "DeletedBy", "EmailConfirmed", "LockoutEnd", "LockoutEnabled"),
            ["Store"] = Set("StoreName", "BusinessType", "Status", "RejectReason", "LockReason", "CommissionRate", "ApprovedAt", "UpdatedAt", "RegistrationStep"),
            ["Product"] = Set("Name", "CategoryId", "BrandId", "Status", "StatusReason", "BasePrice", "IsDeleted", "UpdatedAt"),
            ["Brand"] = Set("Name", "Slug", "IsApproved", "IsDeleted", "UpdatedAt"),
            ["Category"] = Set("ParentId", "Name", "Slug", "IsActive", "IsDeleted"),
            ["CategoryAttribute"] = Set("CategoryId", "Name", "FilterType", "DisplayOrder", "IsFilterable", "Scope", "IsComparable", "ValueType", "Unit"),
            ["CategoryAttributeOption"] = Set("CategoryAttributeId", "Value", "DisplayOrder"),
            ["Voucher"] = Set("Code", "Name", "Type", "DiscountType", "DiscountValue", "MaxDiscount", "MinOrderAmount", "UsageLimit", "MaxUsagePerUser", "Scope", "StoreId", "CategoryId", "StartAt", "EndAt", "IsActive", "Status"),
            ["SystemSetting"] = Set("Key", "Value", "DataType", "GroupName", "UpdatedAt"),
            ["WalletTransaction"] = Set("TransactionCode", "Type", "Amount", "Currency", "BalanceBefore", "BalanceAfter", "Direction", "ReferenceCode", "Status", "CreatedByAdminId", "Note", "CreatedAt"),
            ["PayoutBatch"] = Set("BatchCode", "PeriodStart", "PeriodEnd", "Status", "TotalGrossAmount", "TotalCommissionAmount", "TotalNetAmount", "TotalStores", "SuccessCount", "FailedCount", "HoldReason", "ApprovedByAdminId", "ApprovedAt", "CompletedAt"),
            ["PayoutTransaction"] = Set("PayoutBatchId", "StoreId", "TransactionCode", "OrderCount", "GrossAmount", "CommissionAmount", "NetAmount", "Status", "FailureReason", "ExcludeReason", "RetryCount", "ProcessedAt"),
            ["PayoutItem"] = Set("PayoutTransactionId", "SubOrderId", "GrandTotal", "CommissionAmount", "NetAmount", "IsExcluded", "ExcludeReason")
        };

    public List<AuditChangeDto> CaptureChanges(IEnumerable<EntityEntry> entries)
    {
        var changes = new List<AuditChangeDto>();

        foreach (var entry in entries)
        {
            var entityType = entry.Metadata.ClrType.Name;
            if (!AllowedFields.TryGetValue(entityType, out var allowed))
                continue;

            var entityId = GetEntityId(entry);
            foreach (var property in entry.Properties)
            {
                if (!allowed.Contains(property.Metadata.Name))
                    continue;

                var oldValue = entry.State == Microsoft.EntityFrameworkCore.EntityState.Added
                    ? null
                    : FormatValue(property.OriginalValue, entry, property.Metadata.Name);
                var newValue = entry.State == Microsoft.EntityFrameworkCore.EntityState.Deleted
                    ? null
                    : FormatValue(property.CurrentValue, entry, property.Metadata.Name);

                var isRedactedSettingValue = entityType == "SystemSetting" &&
                                             property.Metadata.Name == "Value" &&
                                             property.IsModified &&
                                             oldValue == "[REDACTED]";
                if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Modified &&
                    oldValue == newValue &&
                    !isRedactedSettingValue)
                    continue;

                changes.Add(new AuditChangeDto
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Field = property.Metadata.Name,
                    OldValue = oldValue,
                    NewValue = newValue
                });
            }
        }

        return changes;
    }

    public string? GetEntityId(EntityEntry entry)
    {
        var values = entry.Properties
            .Where(x => x.Metadata.IsPrimaryKey() && !x.IsTemporary)
            .Select(x => FormatValue(x.CurrentValue, entry, x.Metadata.Name))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        return values.Length == 0 ? null : string.Join("/", values);
    }

    public string? GetEntityDisplayName(EntityEntry entry)
    {
        foreach (var name in new[] { "StoreName", "Name", "Code", "BatchCode", "TransactionCode", "Email", "UserName", "Key" })
        {
            var property = entry.Properties.FirstOrDefault(x => x.Metadata.Name == name);
            if (property?.CurrentValue is not null)
                return SanitizeFreeText(Convert.ToString(property.CurrentValue, CultureInfo.InvariantCulture), 300);
        }
        return null;
    }

    public string? SanitizeFreeText(string? value, int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var sanitized = SecretAssignmentRegex().Replace(value.Trim(), "$1=[REDACTED]");
        sanitized = LongNumberRegex().Replace(sanitized, match =>
        {
            var raw = match.Value;
            return raw.Length < 6 ? "[REDACTED]" : $"{raw[..2]}***{raw[^2..]}";
        });
        sanitized = FormattedLongNumberRegex().Replace(sanitized, match =>
        {
            var digits = new string(match.Value.Where(char.IsDigit).ToArray());
            return digits.Length is >= 9 and <= 19
                ? $"{digits[..2]}***{digits[^2..]}"
                : match.Value;
        });
        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength];
    }

    private string? FormatValue(object? value, EntityEntry entry, string propertyName)
    {
        if (value is null) return null;
        if (entry.Metadata.ClrType.Name == "SystemSetting" && propertyName == "Value")
        {
            var key = Convert.ToString(entry.Property("Key").CurrentValue, CultureInfo.InvariantCulture) ?? string.Empty;
            if (SensitiveNameRegex().IsMatch(key)) return "[REDACTED]";
        }

        var text = value switch
        {
            DateTime date => date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            DateTimeOffset date => date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            decimal number => number.ToString(CultureInfo.InvariantCulture),
            double number => number.ToString(CultureInfo.InvariantCulture),
            float number => number.ToString(CultureInfo.InvariantCulture),
            Enum item => item.ToString(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
        return SanitizeFreeText(text, 500);
    }

    private static HashSet<string> Set(params string[] fields) => new(fields, StringComparer.Ordinal);

    [GeneratedRegex("(?i)(password|secret|token|api[_ -]?key|connection[_ -]?string)\\s*[:=]\\s*[^\\s,;]+")]
    private static partial Regex SecretAssignmentRegex();

    [GeneratedRegex("(?<![A-Za-z0-9])[0-9]{9,19}(?![A-Za-z0-9])")]
    private static partial Regex LongNumberRegex();

    [GeneratedRegex("(?<![A-Za-z0-9])[0-9][0-9 -]{7,25}[0-9](?![A-Za-z0-9])")]
    private static partial Regex FormattedLongNumberRegex();

    [GeneratedRegex("(?i)(password|secret|token|api.?key|connection)")]
    private static partial Regex SensitiveNameRegex();
}
