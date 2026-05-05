using ITW.Infrastructure.DependencyInjection;
using ITW.Web.DependencyInjection;
using ITW.Web.Middleware;
using ITW.Web.Setup.Identity;
using ITW.Web.Setup.Startup;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    var logDir = ctx.Configuration["Logging:LogVerzeichnis"]
        ?? Path.Combine(AppContext.BaseDirectory, "logs");

    cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ITW.Suite")
        .WriteTo.Console(outputTemplate:
            "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            new CompactJsonFormatter(),
            Path.Combine(logDir, "itw-suite-.clef"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            shared: true);
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// PWA: .webmanifest ist im Standard-MIME-Mapping nicht enthalten.
// Damit der Browser die Datei als Web App Manifest erkennt, mappen
// wir die Endung explizit auf "application/manifest+json".
builder.Services.Configure<StaticFileOptions>(options =>
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".webmanifest"] = "application/manifest+json";
    options.ContentTypeProvider = provider;
});

builder.Services.AddInfrastructure(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PlatformConnection")
                           ?? throw new InvalidOperationException(
                               "Es wurde keine ConnectionString-Konfiguration 'PlatformConnection' gefunden.");

    options.UseSqlServer(connectionString);
});

builder.Services.AddWebIdentitySetup(builder.Configuration);
builder.Services.AddWebApplicationServices();

var app = builder.Build();

await app.InitializeApplicationAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseMiddleware<PasswortwechselPflichtMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();