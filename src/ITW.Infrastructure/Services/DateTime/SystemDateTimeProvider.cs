using ITW.Application.Abstractions.DateTime;

namespace ITW.Infrastructure.Services.DateTime;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}