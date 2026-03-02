using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NexAlytics.Application.Services;
using NexAlytics.Domain.Interfaces;
using NexAlytics.Infrastructure.Data;
using NexAlytics.Infrastructure.Jobs;
using NexAlytics.Infrastructure.Middleware;
using NexAlytics.Infrastructure.Repositories;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/nexalytics-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;

    // EF Core
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connStr));

    // Repository pattern
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Application services
    builder.Services.AddScoped<AuthService>();
    builder.Services.AddScoped<ClienteService>();
    builder.Services.AddScoped<ProductoService>();
    builder.Services.AddScoped<VentaService>();
    builder.Services.AddScoped<PagoService>();
    builder.Services.AddScoped<ImportService>();
    builder.Services.AddScoped<DashboardService>();
    builder.Services.AddScoped<PowerBiService>();
    builder.Services.AddScoped<UsuarioService>();
    builder.Services.AddScoped<ExportService>();
    builder.Services.AddScoped<EtlJob>();

    // Cookie Authentication
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization();

    // Hangfire
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connStr, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));
    builder.Services.AddHangfireServer();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddControllersWithViews();

    var app = builder.Build();

    // Ensure DB is created and migrated
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    app.UseAuthentication();
    app.UseMiddleware<TenantMiddleware>();
    app.UseAuthorization();

    var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthFilter(httpContextAccessor) }
    });

    // Register recurring ETL job
    RecurringJob.AddOrUpdate<EtlJob>(
        "etl-job",
        job => job.ExecuteAsync(),
        "0 * * * *"); // Every hour

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Hangfire auth filter — allow only authenticated Admin users
public class HangfireAuthFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    private readonly IHttpContextAccessor _accessor;

    public HangfireAuthFilter(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        var user = _accessor.HttpContext?.User;
        return user?.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
    }
}
