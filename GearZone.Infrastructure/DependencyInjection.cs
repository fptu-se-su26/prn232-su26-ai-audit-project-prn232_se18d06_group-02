using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Infrastructure.External;
using GearZone.Infrastructure.Jobs;
using GearZone.Infrastructure.Repositories;
using GearZone.Infrastructure.Settings;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GearZone.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            static string ReadFirstNonEmpty(IConfiguration config, params string[] keys)
            {
                foreach (var key in keys)
                {
                    var value = config[key];
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value.Trim();
                    }
                }

                return string.Empty;
            }

            var payInClientId = ReadFirstNonEmpty(configuration, "PAYOS_PAYIN_CLIENT_ID", "PAYOS_CLIENT_ID");
            var payInApiKey = ReadFirstNonEmpty(configuration, "PAYOS_PAYIN_API_KEY", "PAYOS_API_KEY");
            var payInChecksumKey = ReadFirstNonEmpty(configuration, "PAYOS_PAYIN_CHECKSUM_KEY", "PAYOS_CHECKSUM_KEY");
            var payInReturnUrl = ReadFirstNonEmpty(
                configuration,
                "PAYOS_PAYIN_RETURN_URL",
                "PAYOS_RETURN_URL",
                "PayOS:ReturnUrl");
            var payInCancelUrl = ReadFirstNonEmpty(
                configuration,
                "PAYOS_PAYIN_CANCEL_URL",
                "PAYOS_CANCEL_URL",
                "PayOS:CancelUrl");

            var payOutClientId = ReadFirstNonEmpty(configuration, "PAYOS_PAYOUT_CLIENT_ID", "PAYOS_CLIENT_ID");
            var payOutApiKey = ReadFirstNonEmpty(configuration, "PAYOS_PAYOUT_API_KEY", "PAYOS_API_KEY");
            var payOutChecksumKey = ReadFirstNonEmpty(configuration, "PAYOS_PAYOUT_CHECKSUM_KEY", "PAYOS_CHECKSUM_KEY");

            var payInConfigured =
                !string.IsNullOrWhiteSpace(payInClientId) &&
                !string.IsNullOrWhiteSpace(payInApiKey) &&
                !string.IsNullOrWhiteSpace(payInChecksumKey);

            var payOutConfigured =
                !string.IsNullOrWhiteSpace(payOutClientId) &&
                !string.IsNullOrWhiteSpace(payOutApiKey) &&
                !string.IsNullOrWhiteSpace(payOutChecksumKey);

            // PayOS PayIn settings (standardized variable names)
            services.Configure<PayOSSettings>(options =>
            {
                options.ClientId = payInClientId;
                options.ApiKey = payInApiKey;
                options.ChecksumKey = payInChecksumKey;
                options.ReturnUrl = payInReturnUrl;
                options.CancelUrl = payInCancelUrl;
            });

            // PayOS Payout settings
            services.Configure<PayOSPayoutSettings>(options =>
            {
                options.ClientId = payOutClientId;
                options.ApiKey = payOutApiKey;
                options.ChecksumKey = payOutChecksumKey;
            });

            services.AddMemoryCache();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFileStorageService, CloudinaryStorageService>();
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddHttpClient<IGoongService, GoongService>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<ICategoryAttributeRepository, CategoryAttributeRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ISubOrderRepository, SubOrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
            services.AddScoped<IProductAttributeValueRepository, ProductAttributeValueRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IVariantAttributeValueRepository, VariantAttributeValueRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
            services.AddScoped<IStoreFollowRepository, StoreFollowRepository>();
            services.AddScoped<IConversationRepository, ConversationRepository>();
            services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            services.AddScoped<IPayoutBatchRepository, PayoutBatchRepository>();
            services.AddScoped<IPayoutTransactionRepository, PayoutTransactionRepository>();
            services.AddScoped<IPayoutItemRepository, PayoutItemRepository>();
            services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
            services.AddScoped<IPlatformTransactionRepository, PlatformTransactionRepository>();
            services.AddScoped<IVoucherRepository, VoucherRepository>();
            services.AddScoped<IVoucherUsageRepository, VoucherUsageRepository>();
            services.AddScoped<IUserAddressRepository, UserAddressRepository>();

            // Jobs
            services.AddScoped<PayoutBatchJob>();
            services.AddScoped<OrderAutoCompleteJob>();
            services.AddScoped<PaymentTimeoutJob>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            // Payment strategies
            services.AddScoped<IPaymentStrategy, CodPaymentStrategy>();

            if (payInConfigured)
            {
                services.AddScoped<IPaymentStrategy, PayOSPaymentStrategy>();
                services.AddScoped<IPaymentGateway, PayOSPaymentGateway>();
            }
            else
            {
                services.AddScoped<IPaymentStrategy, UnavailablePayOSPaymentStrategy>();
                services.AddScoped<IPaymentGateway, DisabledPaymentGateway>();
            }

            if (payOutConfigured)
            {
                services.AddScoped<IPayoutClient, PayOSPayoutClient>();
            }
            else
            {
                services.AddScoped<IPayoutClient, DisabledPayoutClient>();
            }

            services.AddHangfireServer(opt => opt.WorkerCount = 2);

            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString));

            return services;
        }
    }
}
