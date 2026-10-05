using System;
using System.IO;

namespace FASTER.Models
{
    public static class Logger
    {
        private const long MaxLogSizeBytes = 10 * 1024 * 1024; // 10 MB

        private static readonly string DefaultLogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FASTER", "faster.log");

        private static string _logPath = DefaultLogPath;
        private static bool? _enabledOverride;

        private static string BackupLogPath => _logPath + ".old";

        public static bool IsEnabled => _enabledOverride ?? Properties.Settings.Default.enableDebugLog;

        public static string LogFilePath => _logPath;

        /// <summary>
        /// Test hook: write to another file and ignore the user's debug-log setting.
        /// Call with no arguments to go back to the real log file and setting.
        /// </summary>
        public static void ConfigureForTesting(string? logPath = null, bool? enabled = null)
        {
            _logPath         = logPath ?? DefaultLogPath;
            _enabledOverride = enabled;
        }

        public static void Log(string message)
        {
            if (!IsEnabled) return;
            WriteLine(message);
        }

        // Always writes, regardless of the debug-logging setting. Reserved for
        // fatal/unhandled-exception logging so crashes are never silently lost.
        public static void LogCritical(string message)
        { WriteLine(message); }

       private static readonly object Gate = new();

        private static void WriteLine(string message)
        {
            try
            {
                lock (Gate)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
                    RotateIfNeeded();
                    File.AppendAllText(_logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
                // Logging failures are intentionally ignored — a broken log file
                // (e.g. disk full, permissions, file locked by another process)
                // must never crash the app or interrupt the calling code.
            }
        }
        /// <summary>
        /// Rotates the log file if it has exceeded the size threshold, keeping a single
        /// backup (faster.log.old) so total on-disk log size stays bounded.
        /// </summary>
        private static void RotateIfNeeded()
        {
            var fileInfo = new FileInfo(_logPath);
            if (!fileInfo.Exists || fileInfo.Length < MaxLogSizeBytes) return;

            if (File.Exists(BackupLogPath))
                File.Delete(BackupLogPath);

            File.Move(_logPath, BackupLogPath);
        }
    }
}
