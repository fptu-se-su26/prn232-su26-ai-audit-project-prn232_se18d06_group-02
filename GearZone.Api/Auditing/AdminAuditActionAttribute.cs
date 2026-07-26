using GearZone.Domain.Enums;

namespace GearZone.Api.Auditing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AdminAuditActionAttribute : Attribute
{
    public AdminAuditActionAttribute(string action, string module, AdminAuditRiskLevel riskLevel)
    {
        Action = action;
        Module = module;
        RiskLevel = riskLevel;
    }

    public string Action { get; }
    public string Module { get; }
    public AdminAuditRiskLevel RiskLevel { get; }
    public AdminAuditOutcome SuccessOutcome { get; set; } = AdminAuditOutcome.Succeeded;
    public string? EntityType { get; set; }
    public string RouteIdName { get; set; } = "id";
    public string? Description { get; set; }
    public string? ReasonArgumentName { get; set; }
    public string ReasonPropertyName { get; set; } = "Reason";
}
