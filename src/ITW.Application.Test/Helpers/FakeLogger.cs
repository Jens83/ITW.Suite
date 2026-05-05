using Microsoft.Extensions.Logging;

namespace ITW.Application.Test.Helpers;

internal sealed class FakeLogger<T> : ILogger<T>
{
    public static readonly FakeLogger<T> Instance = new();

    private FakeLogger() { }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) { }
}
