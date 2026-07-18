using GearZone.Domain.Entities;
using GearZone.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }

        public DbSet<Store> Stores { get; set; }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryAttribute> CategoryAttributes { get; set; }
        public DbSet<CategoryAttributeOption> CategoryAttributeOptions { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }
        public DbSet<ProductAttributeValue> ProductAttributeValues { get; set; }

        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<SubOrder> SubOrders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Shipment> Shipments { get; set; }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<StoreFollow> StoreFollows { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        
        public DbSet<PayoutBatch> PayoutBatches { get; set; }
        public DbSet<PayoutTransaction> PayoutTransactions { get; set; }
        public DbSet<PayoutItem> PayoutItems { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherUsage> VoucherUsages { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            SystemSettingSeeder.Seed(builder);
            CategorySeeder.Seed(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
