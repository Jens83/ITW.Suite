namespace ITW.Application.Abstractions.DateTime;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}