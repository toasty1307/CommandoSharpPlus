using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CommandoSharpPlus.Utils
{
    public class Logger : ILogger
    {
        private readonly ConsoleTheme _theme;
        private readonly string _name;

        public Logger(string name, ConsoleTheme theme)
        {
            _name = name;
            _theme = theme;
        }

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) =>
            _theme.LogLevels.ContainsKey(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (_theme.EventId != 0 && _theme.EventId != eventId.Id) return;
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = _theme.LogLevels[logLevel];
            Console.WriteLine($"[{eventId.Id} : {logLevel} : {DateTime.Now:hh:mm:ss} : {_name}] {formatter(state, exception)}");
            Console.ForegroundColor = originalColor;
        }
    }

    public class LoggerFactory : ILoggerFactory
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => new Logger(categoryName, new ConsoleTheme());

        public void AddProvider(ILoggerProvider provider) { }
    }
    
    public class ConsoleTheme
    {
        public int EventId { get; set; }
        
        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Critical] = ConsoleColor.Red,
            [LogLevel.Debug] = ConsoleColor.Blue,
            [LogLevel.Error] = ConsoleColor.Green,
            [LogLevel.Warning] = ConsoleColor.Yellow,
        };
    }
}