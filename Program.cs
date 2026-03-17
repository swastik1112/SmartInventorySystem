using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;         // ← assuming your DbContext folder/namespace
using SmartInventorySystem.Models;         // ApplicationUser
using SmartInventorySystem.Repositories;
using SmartInventorySystem.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Add services to the container ────────────────────────────────────────────

// Database (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
    ));

// ── ASP.NET Core Identity ────────────────────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings (reasonable defaults – adjust for your policy)
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Cookie authentication settings (must come AFTER AddIdentity)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);     // remember me duration

    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";

    options.SlidingExpiration = true;                      // renew cookie on activity
    options.ReturnUrlParameter = "returnUrl";
});

// MVC + Razor Views
builder.Services.AddControllersWithViews();

// HttpContext accessor (useful in services for audit logging, etc.)
builder.Services.AddHttpContextAccessor();

// ── Register Repositories & Services (Scoped lifetime is recommended) ────────
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add all your custom services here
builder.Services.AddScoped<IProductService, ProductService>();
// builder.Services.AddScoped<ISupplierService, SupplierService>();
// builder.Services.AddScoped<IWarehouseService, WarehouseService>();
// builder.Services.AddScoped<IPurchaseService, PurchaseService>();
// builder.Services.AddScoped<ISaleService, SaleService>();
// ... add others as you implement them

var app = builder.Build();

// ── Configure the HTTP request pipeline ─────────────────────────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();                      // HTTP Strict Transport Security
}

app.UseHttpsRedirection();              // Redirect HTTP → HTTPS
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Default route: Dashboard/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// ── Database seeding (runs once on startup) ──────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        // In production: consider sending alert (email/slack/etc.)
    }
}

app.Run();