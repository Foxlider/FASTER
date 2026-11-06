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
            if (handler == null) return;
            if (handler is DefaultDownloadHandler ddh)
                ddh.Logger = new FasterLoggerAdapter();
        }
    }

    internal class FasterLoggerAdapter : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;

        // Debug and above, and only while Debug Logging is on. Trace is one line per chunk, far too noisy for the file.
        public bool IsEnabled(LogLevel logLevel) => Logger.IsEnabled && logLevel >= LogLevel.Debug;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            Logger.Log($"[{logLevel}] {message}{(exception != null ? $" | {exception}" : "")}");
        }
    }
}