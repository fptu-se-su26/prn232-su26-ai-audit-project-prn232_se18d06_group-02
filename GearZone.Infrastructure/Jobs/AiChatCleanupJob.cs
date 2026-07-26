using System.ComponentModel;
using GearZone.Application.Abstractions.Persistence;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace GearZone.Infrastructure.Jobs;

public sealed class AiChatCleanupJob
{
    private readonly IAiConversationRepository _conversations;
    private readonly ILogger<AiChatCleanupJob> _logger;

    public AiChatCleanupJob(
        IAiConversationRepository conversations,
        ILogger<AiChatCleanupJob> logger)
    {
        _conversations = conversations;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = [60, 180])]
    [DisplayName("Delete expired AI chat conversations")]
    public async Task DeleteExpiredAsync()
    {
        var count = await _conversations.DeleteExpiredAsync(DateTime.UtcNow);
        _logger.LogInformation("Deleted {Count} expired AI chat conversations.", count);
    }
}
