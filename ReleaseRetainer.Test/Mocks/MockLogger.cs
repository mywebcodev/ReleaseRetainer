using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ReleaseRetainer.Test.Mocks;

// https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider
public class MockLogger<T> : ILogger<T>
{
    public List<string> Logs { get; } = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        Logs.Add(message);
        
        if (!IsEnabled(logLevel))
        {
            return;
        }
        
        Debug.WriteLine($"[{eventId.Id,2}: {logLevel,-12}]");
        Debug.WriteLine($"{formatter(state, exception)}");
        Debug.WriteLine(string.Empty);
    }
    
    public virtual bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default!;
    }
}