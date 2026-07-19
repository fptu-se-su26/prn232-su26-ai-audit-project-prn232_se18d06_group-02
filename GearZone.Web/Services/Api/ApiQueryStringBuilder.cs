using System.Collections;
using System.Globalization;
using System.Reflection;

namespace GearZone.Web.Services.Api;

public static class ApiQueryStringBuilder
{
    public static string Build(object query, params string[] excludedProperties)
    {
        var excluded = excludedProperties.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var parts = new List<string>();

        foreach (var property in query.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || excluded.Contains(property.Name)) continue;

            var value = property.GetValue(query);
            if (value is null) continue;

            var key = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
            if (value is IEnumerable values and not string)
            {
                foreach (var item in values)
                {
                    if (item is not null) Add(parts, key, Format(item));
                }
            }
            else
            {
                Add(parts, key, Format(value));
            }
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private static void Add(List<string> parts, string key, string value) =>
        parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");

    private static string Format(object value) => value switch
    {
        DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        bool boolean => boolean ? "true" : "false",
        decimal number => number.ToString(CultureInfo.InvariantCulture),
        double number => number.ToString(CultureInfo.InvariantCulture),
        float number => number.ToString(CultureInfo.InvariantCulture),
        Enum enumValue => enumValue.ToString(),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty
    };
}
