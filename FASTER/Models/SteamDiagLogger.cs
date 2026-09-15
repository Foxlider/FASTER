using System;
using BytexDigital.Steam.ContentDelivery.Models.Downloading;
using Microsoft.Extensions.Logging;

namespace FASTER.Models
{
    /// <summary>
    /// Routes DefaultDownloadHandler's DiagnosticLog callback into FASTER's log file.
    /// Only active when debug logging is enabled in Settings.
    /// </summary>
    internal static class SteamDiagLogger
    {
        internal static void Attach(IDownloadHandler handler)
        {
            if (handler == null) return;  // ← null guard
            if (handler is DefaultDownloadHandler ddh)
                ddh.Logger = new FasterLoggerAdapter();
        }
    }

    internal class FasterLoggerAdapter : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var message = formatter(state, exception);
            Logger.Log($"[{logLevel}] {message}{(exception != null ? $" | {exception}" : "")}");
        }
    }
}