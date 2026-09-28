using System;
using System.IO;

namespace FASTER.Models
{
    public static class Logger
    {
        private const long MaxLogSizeBytes = 10 * 1024 * 1024; // 10 MB

        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FASTER", "faster.log");

        private static readonly string BackupLogPath = LogPath + ".old";

        public static bool IsEnabled => Properties.Settings.Default.enableDebugLog;

        public static string LogFilePath => LogPath;

        public static void Log(string message)
        {
            if (!IsEnabled) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                RotateIfNeeded();
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
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
            var fileInfo = new FileInfo(LogPath);
            if (!fileInfo.Exists || fileInfo.Length < MaxLogSizeBytes) return;

            if (File.Exists(BackupLogPath))
                File.Delete(BackupLogPath);

            File.Move(LogPath, BackupLogPath);
        }
    }
}
