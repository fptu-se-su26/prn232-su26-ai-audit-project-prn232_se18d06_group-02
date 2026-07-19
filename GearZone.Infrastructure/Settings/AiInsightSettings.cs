namespace GearZone.Infrastructure.Settings;

public sealed class AiInsightSettings
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "OpenAI";
    public int TimeoutSeconds { get; set; } = 30;
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiModel { get; set; } = "gpt-5.6-luna";
    public string GeminiApiKey { get; set; } = string.Empty;
    public string GeminiModel { get; set; } = "gemini-3.5-flash";
}

