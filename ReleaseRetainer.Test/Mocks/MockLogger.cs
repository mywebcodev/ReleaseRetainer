using Microsoft.Extensions.Logging;

namespace ReleaseRetainer.Test.Mocks;

// https://github.com/nsubstitute/NSubstitute/issues/597
public abstract class MockLogger<T> : ILogger<T>
{
    public List<string> Logs { get; } = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var unboxed = (state! as IReadOnlyList<KeyValuePair<string, object>>).ToDictionary(k => k.Key, v => v.Value);
        var message = formatter(state, exception);
        Logs.Add(message);
        Log();
        Log(logLevel, message, exception);
        Log(logLevel, unboxed, exception);
        LogInformation(message, unboxed.Where(kv => kv.Key != "OriginalFormat").Select(v => v.Value).ToArray());
    }

    public abstract void Log();

    public abstract void Log(LogLevel logLevel, string message, Exception? exception = null);

    public abstract void Log(LogLevel logLevel, IDictionary<string, object> state, Exception? exception = null);

    public abstract void LogInformation(string message, params object?[] args);

    public virtual bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public abstract IDisposable BeginScope<TState>(TState state);
}