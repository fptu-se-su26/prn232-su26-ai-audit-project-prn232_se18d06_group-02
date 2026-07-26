using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GearZone.Application.Abstractions.Services;

namespace GearZone.Application.Features.AiChat;

internal sealed record AiNormalizedProductSearch(
    string? Search,
    string? CategorySlug,
    IReadOnlyList<string>? CategorySlugs);

internal static partial class AiProductSearchNormalizer
{
    private sealed record CategoryAlias(string Text, string Slug);

    private static readonly CategoryAlias[] KnownAliases =
    [
        new("ban phim co", "mechanical-keyboards"),
        new("mechanical keyboard", "mechanical-keyboards"),
        new("mechanical keyboards", "mechanical-keyboards"),
        new("ban phim membrane", "membrane-keyboards"),
        new("membrane keyboard", "membrane-keyboards"),
        new("membrane keyboards", "membrane-keyboards"),
        new("keyboard switch", "keyboard-switches"),
        new("keyboard switches", "keyboard-switches"),
        new("switch ban phim", "keyboard-switches"),
        new("keycap", "keycaps"),
        new("keycaps", "keycaps"),
        new("ban phim", "keyboards"),
        new("keyboard", "keyboards"),
        new("keyboards", "keyboards"),

        new("chuot gaming", "gaming-mice"),
        new("gaming mouse", "gaming-mice"),
        new("gaming mice", "gaming-mice"),
        new("chuot van phong", "office-mice"),
        new("office mouse", "office-mice"),
        new("office mice", "office-mice"),
        new("lot chuot", "mouse-pads"),
        new("mouse pad", "mouse-pads"),
        new("mouse pads", "mouse-pads"),
        new("chuot", "mice"),
        new("mouse", "mice"),
        new("mice", "mice"),

        new("tai nghe gaming", "gaming-headsets"),
        new("gaming headphone", "gaming-headsets"),
        new("gaming headphones", "gaming-headsets"),
        new("gaming headset", "gaming-headsets"),
        new("gaming headsets", "gaming-headsets"),
        new("tai nghe khong day", "wireless-headphones"),
        new("tai nghe bluetooth", "wireless-headphones"),
        new("wireless headphone", "wireless-headphones"),
        new("wireless headphones", "wireless-headphones"),
        new("wireless headset", "wireless-headphones"),
        new("wireless headsets", "wireless-headphones"),
        new("tai nghe", "headsets"),
        new("headphone", "headsets"),
        new("headphones", "headsets"),
        new("headset", "headsets"),
        new("headsets", "headsets"),
        new("microphone", "microphones"),
        new("microphones", "microphones"),
        new("micro", "microphones"),
        new("mic", "microphones"),

        new("man hinh gaming", "gaming-monitors"),
        new("gaming monitor", "gaming-monitors"),
        new("gaming monitors", "gaming-monitors"),
        new("man hinh van phong", "office-monitors"),
        new("office monitor", "office-monitors"),
        new("office monitors", "office-monitors"),
        new("man hinh cong", "curved-monitors"),
        new("curved monitor", "curved-monitors"),
        new("curved monitors", "curved-monitors"),
        new("man hinh", "monitors"),
        new("monitor", "monitors"),
        new("monitors", "monitors"),

        new("card man hinh", "gpus"),
        new("graphics card", "gpus"),
        new("graphics cards", "gpus"),
        new("gpu", "gpus"),
        new("gpus", "gpus"),
        new("bo vi xu ly", "cpus"),
        new("processor", "cpus"),
        new("processors", "cpus"),
        new("cpu", "cpus"),
        new("cpus", "cpus"),
        new("bo mach chu", "motherboards"),
        new("mainboard", "motherboards"),
        new("mainboards", "motherboards"),
        new("motherboard", "motherboards"),
        new("motherboards", "motherboards"),
        new("o cung", "storage"),
        new("ssd", "storage"),
        new("hdd", "storage"),
        new("storage", "storage"),
        new("bo nguon", "power-supplies"),
        new("power supply", "power-supplies"),
        new("power supplies", "power-supplies"),
        new("psu", "power-supplies"),
        new("vo may", "pc-cases"),
        new("pc case", "pc-cases"),
        new("pc cases", "pc-cases"),
        new("case may tinh", "pc-cases"),
        new("linh kien pc", "pc-components"),
        new("linh kien may tinh", "pc-components"),
        new("pc component", "pc-components"),
        new("pc components", "pc-components"),
        new("ram", "ram"),

        new("ghe gaming", "gaming-furniture"),
        new("ban gaming", "gaming-furniture"),
        new("gaming furniture", "gaming-furniture"),
        new("phu kien setup", "setup-accessories"),
        new("setup accessories", "setup-accessories"),
        new("tay cam", "console-controllers"),
        new("controller", "console-controllers"),
        new("controllers", "console-controllers"),
        new("console", "console-controllers")
    ];

    private static readonly string[] SearchFillerPhrases =
    [
        "recommendations",
        "recommendation",
        "list products",
        "list product",
        "search products",
        "search product",
        "find products",
        "find product",
        "from the shop",
        "from shop",
        "in the shop",
        "at gearzone",
        "goi y san pham",
        "tim kiem san pham",
        "tim san pham",
        "san pham",
        "cua hang",
        "cho toi",
        "giup toi",
        "muon mua",
        "can mua",
        "recommend",
        "suggest",
        "search",
        "find",
        "show me",
        "list",
        "goi y",
        "tim kiem",
        "tim",
        "gearzone"
    ];

    public static AiNormalizedProductSearch Normalize(
        string? query,
        string? requestedCategorySlug,
        IReadOnlyCollection<CatalogCategoryDto> categoryTree)
    {
        var categories = Flatten(categoryTree).ToList();
        var validSlugs = categories
            .Select(x => x.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var aliases = BuildAvailableAliases(categories, validSlugs);

        var normalizedRequestedCategory = NormalizeText(requestedCategorySlug);
        string? categorySlug = null;
        if (!string.IsNullOrWhiteSpace(requestedCategorySlug))
        {
            categorySlug = validSlugs.FirstOrDefault(
                slug => string.Equals(slug, requestedCategorySlug.Trim(), StringComparison.OrdinalIgnoreCase));
            categorySlug ??= FindAlias(normalizedRequestedCategory, aliases)?.Slug;
        }

        var normalizedQuery = NormalizeText(query);
        var queryAlias = FindAlias(normalizedQuery, aliases);
        if (queryAlias is not null)
        {
            // The query usually carries the customer's most specific intent, while
            // category_slug is model-generated and may be broad or invalid.
            categorySlug = queryAlias.Slug;
            normalizedQuery = RemovePhrase(normalizedQuery, queryAlias.Text);
        }

        normalizedQuery = RemoveFillerPhrases(normalizedQuery);
        var search = string.IsNullOrWhiteSpace(normalizedQuery)
            ? null
            : normalizedQuery;

        IReadOnlyList<string>? categorySlugs = null;
        if (string.Equals(categorySlug, "headsets", StringComparison.OrdinalIgnoreCase))
        {
            // Microphones are currently modeled as a child of Headsets. A customer
            // asking for headphones/headsets should not receive microphone cards.
            categorySlugs =
            [
                .. new[] { "gaming-headsets", "wireless-headphones" }
                    .Where(validSlugs.Contains)
            ];
        }

        return new AiNormalizedProductSearch(search, categorySlug, categorySlugs);
    }

    private static IEnumerable<CatalogCategoryDto> Flatten(
        IEnumerable<CatalogCategoryDto> categories)
    {
        foreach (var category in categories)
        {
            yield return category;
            foreach (var child in Flatten(category.SubCategories))
            {
                yield return child;
            }
        }
    }

    private static List<CategoryAlias> BuildAvailableAliases(
        IEnumerable<CatalogCategoryDto> categories,
        HashSet<string> validSlugs)
    {
        var aliases = KnownAliases
            .Where(x => validSlugs.Contains(x.Slug))
            .ToList();

        foreach (var category in categories)
        {
            aliases.Add(new CategoryAlias(NormalizeText(category.Name), category.Slug));
            aliases.Add(new CategoryAlias(NormalizeText(category.Slug), category.Slug));
        }

        return aliases
            .Where(x => !string.IsNullOrWhiteSpace(x.Text))
            .DistinctBy(x => new { x.Text, Slug = x.Slug.ToLowerInvariant() })
            .OrderByDescending(x => x.Text.Length)
            .ToList();
    }

    private static CategoryAlias? FindAlias(
        string normalizedText,
        IEnumerable<CategoryAlias> aliases)
    {
        if (string.IsNullOrWhiteSpace(normalizedText)) return null;

        return aliases.FirstOrDefault(alias =>
            Regex.IsMatch(
                normalizedText,
                $@"(?<![a-z0-9]){Regex.Escape(alias.Text)}(?![a-z0-9])",
                RegexOptions.CultureInvariant));
    }

    private static string RemoveFillerPhrases(string value)
    {
        foreach (var phrase in SearchFillerPhrases.OrderByDescending(x => x.Length))
        {
            value = RemovePhrase(value, phrase);
        }

        return CollapseWhitespaceRegex().Replace(value, " ").Trim();
    }

    private static string RemovePhrase(string value, string phrase)
    {
        var result = Regex.Replace(
            value,
            $@"(?<![a-z0-9]){Regex.Escape(phrase)}(?![a-z0-9])",
            " ",
            RegexOptions.CultureInvariant);
        return CollapseWhitespaceRegex().Replace(result, " ").Trim();
    }

    private static string NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var normalizedCharacter = character == 'đ' ? 'd' : character;
            builder.Append(char.IsLetterOrDigit(normalizedCharacter) ? normalizedCharacter : ' ');
        }

        return CollapseWhitespaceRegex()
            .Replace(builder.ToString().Normalize(NormalizationForm.FormC), " ")
            .Trim();
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex CollapseWhitespaceRegex();
}
