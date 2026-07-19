namespace GearZone.Application.Features.Admin;

public static class AdminAuditModules
{
    public const string Users = "Users";
    public const string Stores = "Stores";
    public const string Products = "Products";
    public const string Brands = "Brands";
    public const string Categories = "Categories";
    public const string Vouchers = "Vouchers";
    public const string Finance = "Finance";
    public const string Settings = "Settings";
    public const string Reports = "Reports";
    public const string Security = "Security";
    public const string Audit = "Audit";
}

public static class AdminAuditActions
{
    public const string UserCreated = "ADMIN_USER_CREATED";
    public const string UserUpdated = "ADMIN_USER_UPDATED";
    public const string UserDeleted = "ADMIN_USER_DELETED";
    public const string UserRestored = "ADMIN_USER_RESTORED";
    public const string StoreApproved = "STORE_APPLICATION_APPROVED";
    public const string StoreRejected = "STORE_APPLICATION_REJECTED";
    public const string StoreInfoRequested = "STORE_INFO_REQUESTED";
    public const string StoreStatusChanged = "STORE_STATUS_CHANGED";
    public const string ProductApproved = "PRODUCT_APPROVED";
    public const string ProductRejected = "PRODUCT_REJECTED";
    public const string ProductSuspended = "PRODUCT_SUSPENDED";
    public const string ProductDeleted = "PRODUCT_DELETED";
    public const string ProductBulkStatusChanged = "PRODUCT_BULK_STATUS_CHANGED";
    public const string BrandCreated = "BRAND_CREATED";
    public const string BrandUpdated = "BRAND_UPDATED";
    public const string BrandApproved = "BRAND_APPROVED";
    public const string BrandRejected = "BRAND_REJECTED";
    public const string BrandDeleted = "BRAND_DELETED";
    public const string CategoryCreated = "CATEGORY_CREATED";
    public const string CategoryUpdated = "CATEGORY_UPDATED";
    public const string CategoryAttributesUpdated = "CATEGORY_ATTRIBUTES_UPDATED";
    public const string CategoryDeleted = "CATEGORY_DELETED";
    public const string VoucherCreated = "VOUCHER_CREATED";
    public const string VoucherUpdated = "VOUCHER_UPDATED";
    public const string VoucherStatusChanged = "VOUCHER_STATUS_CHANGED";
    public const string SettingsUpdated = "PLATFORM_SETTINGS_UPDATED";
    public const string WalletToppedUp = "WALLET_TOPPED_UP";
    public const string PayoutBatchGenerated = "PAYOUT_BATCH_GENERATED";
    public const string PayoutBatchApproved = "PAYOUT_BATCH_APPROVED";
    public const string PayoutBatchHeld = "PAYOUT_BATCH_HELD";
    public const string PayoutProcessQueued = "PAYOUT_PROCESS_QUEUED";
    public const string PayoutProcessSucceeded = "PAYOUT_PROCESS_SUCCEEDED";
    public const string PayoutProcessFailed = "PAYOUT_PROCESS_FAILED";
    public const string PayoutTransactionRetried = "PAYOUT_TRANSACTION_RETRIED";
    public const string PayoutTransactionExcluded = "PAYOUT_TRANSACTION_EXCLUDED";
    public const string ReportExported = "REPORT_EXPORTED";
    public const string AiInsightGenerated = "AI_INSIGHT_GENERATED";
    public const string AiInsightRegenerated = "AI_INSIGHT_REGENERATED";
    public const string AdminLoginSucceeded = "ADMIN_LOGIN_SUCCEEDED";
    public const string AdminLoginFailed = "ADMIN_LOGIN_FAILED";
    public const string AdminLogout = "ADMIN_LOGOUT";
    public const string AuditLogExported = "AUDIT_LOG_EXPORTED";
}
