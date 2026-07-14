using GearZone.Application;
using GearZone.Application.Abstractions.External;
using GearZone.Domain.Entities;
using GearZone.Infrastructure;
using GearZone.Infrastructure.Jobs;
using GearZone.Infrastructure.Seed;
using GearZone.Web.Hubs;
using GearZone.Web.Pages.Public.User.Messages;
using Hangfire;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;

var builder = WebApplication.CreateBuilder(args);

var envCandidates = new[]
{
    System.IO.Path.Combine(builder.Environment.ContentRootPath, ".env"),
    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), ".env"),
    System.IO.Path.GetFullPath(System.IO.Path.Combine(builder.Environment.ContentRootPath, "..", ".env"))
}
.Distinct(StringComparer.OrdinalIgnoreCase);

foreach (var envPath in envCandidates)
{
    if (!System.IO.File.Exists(envPath))
    {
        continue;
    }

    DotNetEnv.Env.Load(envPath);
    Console.WriteLine($"Environment: loaded {envPath}");
}

static void EnsureEnvAlias(string targetKey, string sourceKey)
{
    var targetValue = Environment.GetEnvironmentVariable(targetKey);
    if (!string.IsNullOrWhiteSpace(targetValue))
    {
        return;
    }

    var sourceValue = Environment.GetEnvironmentVariable(sourceKey);
    if (!string.IsNullOrWhiteSpace(sourceValue))
    {
        Environment.SetEnvironmentVariable(targetKey, sourceValue.Trim());
    }
}

// Backward-compatible PayOS env aliases so both legacy PAYOS_* and new PAYOS_PAYIN_* keys work.
EnsureEnvAlias("PAYOS_CLIENT_ID", "PAYOS_PAYIN_CLIENT_ID");
EnsureEnvAlias("PAYOS_API_KEY", "PAYOS_PAYIN_API_KEY");
EnsureEnvAlias("PAYOS_CHECKSUM_KEY", "PAYOS_PAYIN_CHECKSUM_KEY");
EnsureEnvAlias("PAYOS_RETURN_URL", "PAYOS_PAYIN_RETURN_URL");
EnsureEnvAlias("PAYOS_CANCEL_URL", "PAYOS_PAYIN_CANCEL_URL");
EnsureEnvAlias("PAYOS_PAYIN_CLIENT_ID", "PAYOS_CLIENT_ID");
EnsureEnvAlias("PAYOS_PAYIN_API_KEY", "PAYOS_API_KEY");
EnsureEnvAlias("PAYOS_PAYIN_CHECKSUM_KEY", "PAYOS_CHECKSUM_KEY");
EnsureEnvAlias("PAYOS_PAYIN_RETURN_URL", "PAYOS_RETURN_URL");
EnsureEnvAlias("PAYOS_PAYIN_CANCEL_URL", "PAYOS_CANCEL_URL");

builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration["DB_CONNECTION_STRING"] ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderTrackingNotifier, SignalROrderTrackingNotifier>();
builder.Services.AddScoped<BuyerInboxComposer>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Shared Data Protection key ring. Persisting the keys with a fixed application
// name lets a future separate API host (GearZone.Api) decrypt & validate the same
// Identity auth cookie issued here. Both apps MUST use the same path + name.
// Note: changing the application name invalidates cookies issued before this change,
// so existing sessions are logged out once (login flow itself is unchanged).
var dataProtectionKeysPath = builder.Configuration["DATA_PROTECTION_KEYS_PATH"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "..", "shared-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("GearZone");

builder.Services.AddAuthentication(options =>
{
    // Use Identity's scheme as the default for everything
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["GOOGLE_CLIENT_ID"] ?? "";
    options.ClientSecret = builder.Configuration["GOOGLE_CLIENT_SECRET"] ?? "";
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;

    // Reduce local/dev correlation-cookie drop issues during Google callback.
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.CorrelationCookie.HttpOnly = true;
    options.CorrelationCookie.IsEssential = true;

    options.Events = new OAuthEvents
    {
        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            var message = Uri.EscapeDataString(context.Failure?.Message ?? "External login failed.");
            context.Response.Redirect($"/Auth/Login?remoteError={message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(GearZone.Application.Abstractions.Services.IAuthService).Assembly);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.AccessDeniedPath = "/Auth/Login";
    opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    opt.SlidingExpiration = true;
    opt.Cookie.SameSite = SameSiteMode.Lax;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

builder.Services
    .AddDatabase(connectionString)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    ;

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var configuration = services.GetRequiredService<IConfiguration>();

    try
    {
        await IdentitySeeder.SeedAsync(userManager, roleManager, configuration);
        Console.WriteLine("Seed[Identity]: completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed[Identity]: {ex}");
    }

    try
    {
        await CatalogSeeder.SeedAsync(dbContext);
        Console.WriteLine("Seed[Catalog]: completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed[Catalog]: {ex}");
    }
}

app.UseHttpsRedirection();
app.UseCors();

// URL Rewrite for backward compatibility
var rewriteOptions = new RewriteOptions()
    .AddRedirect("(?i)Public/Catalog/Browse/?$", "products")
    .AddRedirect("(?i)Public/Catalog/Browse(.*)", "products$1");
app.UseRewriter(rewriteOptions);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

using (var scope = app.Services.CreateScope())
{
    // Recurring jobs
    RecurringJob.AddOrUpdate<PayoutBatchJob>(
        "generate-weekly-payout",
        job => job.GenerateWeeklyBatchAsync(),
        "1 17 * * 0", // Sunday 17:01 UTC = Monday 00:01 Vietnam time
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<PayoutBatchJob>(
        "retry-failed-payouts",
        job => job.RetryFailedTransactionsAsync(),
        "0 */6 * * *",
        TimeZoneInfo.Utc);

    RecurringJob.AddOrUpdate<OrderAutoCompleteJob>(
        "order-auto-complete",
        job => job.AutoCompleteOrdersAsync(),
        Cron.Daily(),
        TimeZoneInfo.Utc);
}

app.UseStaticFiles();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<OrderTrackingHub>("/hubs/order-tracking");
app.MapRazorPages();

app.Run();
