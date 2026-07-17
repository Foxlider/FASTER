using System;
using System.IO;

namespace FASTER.Models
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FASTER", "faster.log");

        public static bool IsEnabled => Properties.Settings.Default.enableDebugLog;

        public static string LogFilePath => LogPath;

        public static void Log(string message)
        {
            if (!IsEnabled) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Logging failures are intentionally ignored — a broken log file
                // (e.g. disk full, permissions, file locked by another process)
                // must never crash the app or interrupt the calling code.
            }
        }
    }
}
