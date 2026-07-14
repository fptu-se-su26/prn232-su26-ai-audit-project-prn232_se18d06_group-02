using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Auth;
using GearZone.Application.Features.Cart;
using GearZone.Application.Features.Catalog;
using GearZone.Application.Features.Chat;
using GearZone.Application.Features.Checkout;
using GearZone.Application.Features.Map;
using GearZone.Application.Features.Orders;
using GearZone.Application.Features.Payment;
using GearZone.Application.Features.Payout;
using GearZone.Application.Features.Reviews;
using GearZone.Application.Features.Seller;
using GearZone.Application.Features.Shipping;
using GearZone.Application.Features.User;
using Microsoft.Extensions.DependencyInjection;

namespace GearZone.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IOrderTrackingNotifier, NoOpOrderTrackingNotifier>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IAdminStoreService, AdminStoreService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();
            services.AddScoped<IAdminCategoryService, AdminCategoryService>();
            services.AddScoped<IAdminProductService, AdminProductService>();
            services.AddScoped<IAdminOrderService, AdminOrderService>();
            services.AddScoped<IAdminBrandService, AdminBrandService>();
            services.AddScoped<IAdminPayoutService, AdminPayoutService>();
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<ISellerStoreService, SellerStoreService>();
            services.AddScoped<IBankCatalogService, BankCatalogService>();
            services.AddScoped<ISellerProductService, SellerProductService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICheckoutService, CheckoutService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPayoutService, PayoutService>();
            services.AddScoped<IProductReviewService, ProductReviewService>();
            services.AddScoped<PaymentStrategyFactory>();
            services.AddScoped<IAdminWalletService, AdminWalletService>();
            services.AddScoped<IAdminPlatformService, AdminPlatformService>();
            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddScoped<IAdminVoucherService, AdminVoucherService>();
            services.AddScoped<IVoucherService, VoucherService>();
            services.AddScoped<IShippingService, ShippingService>();
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISellerVoucherService, SellerVoucherService>();
            services.AddScoped<ISellerReportService, SellerReportService>();
            services.AddScoped<IPaymentService, PaymentService>();

            return services;
        }
    }
}
