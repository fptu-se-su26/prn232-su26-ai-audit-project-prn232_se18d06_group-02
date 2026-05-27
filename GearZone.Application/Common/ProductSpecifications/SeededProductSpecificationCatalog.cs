using System;
using System.Collections.Generic;
using System.Linq;

namespace GearZone.Application.Common.ProductSpecifications
{
    public sealed class SeededProductSpecificationDefinition
    {
        public SeededProductSpecificationDefinition(string displayName, string? unit = null, string? valueType = null, params string[] legacyKeys)
        {
            DisplayName = displayName;
            Unit = unit;
            ValueType = valueType;
            LegacyKeys = legacyKeys.Length == 0
                ? new[] { displayName }
                : legacyKeys;
        }

        public string DisplayName { get; }
        public string? Unit { get; }
        public string? ValueType { get; }
        public IReadOnlyList<string> LegacyKeys { get; }
    }

    public static class SeededProductSpecificationCatalog
    {
        private static readonly IReadOnlyDictionary<string, IReadOnlyList<SeededProductSpecificationDefinition>> Templates
            = new Dictionary<string, IReadOnlyList<SeededProductSpecificationDefinition>>(StringComparer.OrdinalIgnoreCase)
            {
                ["gpus"] = new List<SeededProductSpecificationDefinition>
                {
                    new("CUDA Cores", valueType: "Number"),
                    new("Boost Clock", unit: "MHz", valueType: "Number"),
                    new("TDP", unit: "W", valueType: "Number"),
                    new("Process Node"),
                    new("Max Resolution")
                },
                ["cpus"] = new List<SeededProductSpecificationDefinition>
                {
                    new("Threads", valueType: "Number"),
                    new("Base Clock", unit: "GHz", valueType: "Number"),
                    new("Boost Clock", unit: "GHz", valueType: "Number", "Boost Clock", "Max Boost Clock"),
                    new("L3 Cache", unit: "MB", valueType: "Number"),
                    new("TDP", unit: "W", valueType: "Number")
                },
                ["ram"] = new List<SeededProductSpecificationDefinition>
                {
                    new("Latency"),
                    new("Voltage", unit: "V"),
                    new("Heat Spreader"),
                    new("RGB")
                },
                ["mechanical-keyboards"] = new List<SeededProductSpecificationDefinition>
                {
                    new("Body Material"),
                    new("Backlight"),
                    new("Battery", unit: "mAh"),
                    new("Hot-swappable"),
                    new("Polling Rate")
                }
            };

        public static IReadOnlyList<SeededProductSpecificationDefinition> GetTemplate(string? categorySlug)
        {
            if (string.IsNullOrWhiteSpace(categorySlug))
            {
                return Array.Empty<SeededProductSpecificationDefinition>();
            }

            return Templates.TryGetValue(categorySlug, out var specs)
                ? specs
                : Array.Empty<SeededProductSpecificationDefinition>();
        }

        public static SeededProductSpecificationDefinition? GetDefinition(string? categorySlug, string specificationName)
        {
            return GetTemplate(categorySlug)
                .FirstOrDefault(def => def.DisplayName.Equals(specificationName, StringComparison.OrdinalIgnoreCase)
                    || def.LegacyKeys.Any(key => key.Equals(specificationName, StringComparison.OrdinalIgnoreCase)));
        }

        public static string? FindLegacyValue(string? categorySlug, IReadOnlyDictionary<string, string> specs, string specificationName)
        {
            var definition = GetDefinition(categorySlug, specificationName);
            if (definition != null)
            {
                foreach (var key in definition.LegacyKeys)
                {
                    var match = specs.FirstOrDefault(kv => kv.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
                    if (!string.IsNullOrWhiteSpace(match.Key))
                    {
                        return match.Value;
                    }
                }
            }

            var directMatch = specs.FirstOrDefault(kv => kv.Key.Equals(specificationName, StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(directMatch.Key) ? null : directMatch.Value;
        }
    }
}
