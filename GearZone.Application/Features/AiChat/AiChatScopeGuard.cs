using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.AiChat;

internal static partial class AiChatScopeGuard
{
    private static readonly string[] PlatformTerms =
    [
        "gearzone",
        "san pham",
        "product",
        "products",
        "shop",
        "store",
        "seller",
        "nguoi ban",
        "cua hang",
        "gia",
        "price",
        "budget",
        "ngan sach",
        "khuyen mai",
        "promotion",
        "voucher",
        "coupon",
        "ma giam gia",
        "ton kho",
        "con hang",
        "het hang",
        "in stock",
        "out of stock",
        "bao hanh",
        "warranty",
        "doi tra",
        "return",
        "refund",
        "hoan tien",
        "giao hang",
        "van chuyen",
        "shipping",
        "ship",
        "delivery",
        "don hang",
        "dat hang",
        "huy don",
        "order",
        "tracking",
        "thanh toan",
        "payment",
        "payos",
        "cod",
        "gio hang",
        "cart",
        "checkout",
        "tai khoan",
        "account",
        "dang nhap",
        "login",
        "mat khau",
        "password",
        "danh gia",
        "review",
        "chinh sach",
        "policy",
        "khieu nai",
        "complaint",
        "lien he ho tro",
        "contact support",
        "customer service",
        "so sanh",
        "compare",
        "recommend",
        "suggest",
        "goi y",
        "co ban",
        "available",
        "mua",
        "buy",

        "ban phim",
        "keyboard",
        "keycap",
        "switch",
        "chuot",
        "mouse",
        "lot chuot",
        "mouse pad",
        "tai nghe",
        "headphone",
        "headset",
        "microphone",
        "micro",
        "man hinh",
        "monitor",
        "linh kien",
        "pc component",
        "cpu",
        "processor",
        "gpu",
        "graphics card",
        "card man hinh",
        "ram",
        "mainboard",
        "motherboard",
        "ssd",
        "hdd",
        "o cung",
        "storage",
        "psu",
        "power supply",
        "bo nguon",
        "pc case",
        "vo may",
        "gaming furniture",
        "ghe gaming",
        "setup",
        "console",
        "controller",
        "tay cam",
        "laptop",
        "may tinh",
        "computer",
        "gaming pc"
    ];

    private static readonly string[] ExplicitOutOfScopeTasks =
    [
        "viet code",
        "code cho toi",
        "lap trinh cho toi",
        "debug code",
        "sua code",
        "write code",
        "implement code",
        "program this",
        "giai bai toan",
        "giai phuong trinh",
        "giai bai tap",
        "lam bai tap",
        "solve this equation",
        "solve this math",
        "viet bai tho",
        "sang tac tho",
        "ke chuyen",
        "viet truyen",
        "viet essay",
        "write a poem",
        "write poem",
        "write a story",
        "write an essay",
        "du bao thoi tiet",
        "thoi tiet hom nay",
        "weather today",
        "weather forecast",
        "tin tuc hom nay",
        "tin bong da",
        "ket qua bong da",
        "news today",
        "football score",
        "nau mon",
        "cong thuc nau",
        "huong dan nau",
        "cooking recipe",
        "medical advice",
        "tu van benh",
        "chan doan benh",
        "tu van phap luat",
        "legal advice",
        "tu van dau tu",
        "investment advice",
        "gia bitcoin",
        "bitcoin price",
        "bo qua huong dan",
        "bo qua chi dan",
        "ignore previous instructions",
        "ignore system prompt"
    ];

    private static readonly HashSet<string> CourtesyMessages = new(StringComparer.Ordinal)
    {
        "hi",
        "hello",
        "hey",
        "xin chao",
        "chao",
        "chao ban",
        "good morning",
        "good afternoon",
        "good evening",
        "cam on",
        "cam on ban",
        "thank you",
        "thanks",
        "ok",
        "okay",
        "tam biet",
        "bye",
        "goodbye",
        "ban la ai",
        "who are you",
        "ban co the lam gi",
        "what can you do"
    };

    private static readonly string[] FollowUpTerms =
    [
        "cai nay",
        "mau nay",
        "loai nay",
        "san pham nay",
        "cai dau tien",
        "cai thu hai",
        "cai thu",
        "con cai",
        "con mau",
        "mau nao",
        "loai nao",
        "tot khong",
        "phu hop khong",
        "them thong tin",
        "chi tiet hon",
        "the first one",
        "the second one",
        "this one",
        "that one",
        "what about",
        "tell me more",
        "more details",
        "is it good",
        "which one",
        "them nua",
        "con nua"
    ];

    public static bool IsInScope(
        string message,
        IReadOnlyList<AiMessage> history,
        AiChatPageContextDto? pageContext)
    {
        var normalized = Normalize(message);
        if (string.IsNullOrWhiteSpace(normalized)) return false;

        if (ContainsAny(normalized, ExplicitOutOfScopeTasks)) return false;
        if (CourtesyMessages.Contains(normalized)) return true;
        if (ContainsAny(normalized, PlatformTerms)) return true;

        var hasPageContext =
            !string.IsNullOrWhiteSpace(pageContext?.ProductSlug) ||
            !string.IsNullOrWhiteSpace(pageContext?.StoreSlug);
        var isContextualFollowUp = ContainsAny(normalized, FollowUpTerms);

        return isContextualFollowUp &&
               (hasPageContext || HasRecentPlatformContext(history));
    }

    public static string OutOfScopeResponse() =>
        "Sorry, I can only help with GearZone topics such as products, pricing and stock, stores, policies, payments, delivery, and orders.";

    private static bool HasRecentPlatformContext(IReadOnlyList<AiMessage> history)
    {
        foreach (var item in history.TakeLast(6).Reverse())
        {
            if (item.Role == AiMessageRole.Assistant &&
                (item.MetadataJson.Contains("\"products\":[{", StringComparison.OrdinalIgnoreCase) ||
                 item.MetadataJson.Contains("\"orders\":[{", StringComparison.OrdinalIgnoreCase) ||
                 item.MetadataJson.Contains("\"sources\":[{", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (item.Role == AiMessageRole.User &&
                ContainsAny(Normalize(item.Content), PlatformTerms))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAny(string normalized, IEnumerable<string> terms) =>
        terms.Any(term => ContainsPhrase(normalized, term));

    private static bool ContainsPhrase(string normalized, string phrase) =>
        Regex.IsMatch(
            normalized,
            $@"(?<![a-z0-9]){Regex.Escape(phrase)}(?![a-z0-9])",
            RegexOptions.CultureInvariant);

    private static string Normalize(string? value)
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
