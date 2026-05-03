using ITW.Infrastructure.DependencyInjection;
using ITW.Web.DependencyInjection;
using ITW.Web.Middleware;
using ITW.Web.Setup.Identity;
using ITW.Web.Setup.Startup;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

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