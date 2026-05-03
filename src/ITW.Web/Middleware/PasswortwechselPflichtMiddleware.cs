// Datei: src/ITW.Web/Middleware/PasswortwechselPflichtMiddleware.cs
using ITW.Application.Abstractions.Identity;

namespace ITW.Web.Middleware;

public sealed class PasswortwechselPflichtMiddleware
{
    private readonly RequestDelegate _next;

    public PasswortwechselPflichtMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (MussUmgeleitetWerden(context))
        {
            var returnUrl = BuildReturnUrl(context.Request);
            var zielUrl = "/Account/PasswortAendern";

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                !string.Equals(returnUrl, "/Account/PasswortAendern", StringComparison.OrdinalIgnoreCase))
            {
                zielUrl += "?returnUrl=" + Uri.EscapeDataString(returnUrl);
            }

            context.Response.Redirect(zielUrl);
            return;
        }

        await _next(context);
    }

    private static bool MussUmgeleitetWerden(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (!context.User.HasClaim(BenutzerkontoClaimTypes.MussPasswortAendern, "true"))
        {
            return false;
        }

        var path = context.Request.Path.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        if (System.IO.Path.HasExtension(path))
        {
            return false;
        }

        if (path.StartsWith("/Account/PasswortAendern", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Account/Logout", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Account/KeinZugriff", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string? BuildReturnUrl(HttpRequest request)
    {
        var raw = $"{request.PathBase}{request.Path}{request.QueryString}";
        return string.IsNullOrWhiteSpace(raw) ? null : raw;
    }
}