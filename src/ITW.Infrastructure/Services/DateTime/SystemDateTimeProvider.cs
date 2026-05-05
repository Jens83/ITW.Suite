using ITW.Application.Abstractions.DateTime;

namespace ITW.Infrastructure.Services.DateTime;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    // Intentional system-time access — this is the single authorised point.
#pragma warning disable RS0030
    public DateOnly Today => DateOnly.FromDateTime(System.DateTime.Today);
#pragma warning restore RS0030
}
