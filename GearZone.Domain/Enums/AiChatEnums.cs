namespace GearZone.Domain.Enums;

public enum AiConversationStatus
{
    Active,
    Archived
}

public enum AiMessageRole
{
    User,
    Assistant
}

public enum AiMessageStatus
{
    Pending,
    Streaming,
    Completed,
    Failed,
    Blocked
}

public enum AiKnowledgeStatus
{
    Draft,
    Published,
    Archived
}

public enum AiKnowledgeCategory
{
    General,
    Products,
    Orders,
    Shipping,
    Returns,
    Warranty,
    Payments,
    Account
}
