namespace GearZone.Infrastructure.Settings;

public sealed class AiChatSettings
{
    public bool Enabled { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxOutputTokens { get; set; } = 700;
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-3.1-flash-lite";
}
