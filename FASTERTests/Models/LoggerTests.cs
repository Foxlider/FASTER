using System.IO;
using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture]
    public class LoggerTests
    {
        private bool _originalDebugLogSetting;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _originalDebugLogSetting = Properties.Settings.Default.enableDebugLog;
            Properties.Settings.Default.enableDebugLog = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Properties.Settings.Default.enableDebugLog = _originalDebugLogSetting;
            Properties.Settings.Default.Save();
        }

        [SetUp]
        public void SetUp()
        { CleanupLogFiles(); }

        [TearDown]
        public void TearDown()
        { CleanupLogFiles(); }

        private static void CleanupLogFiles()
        {
            var logPath = Logger.LogFilePath;
            var backupPath = logPath + ".old";

            if (File.Exists(logPath)) File.Delete(logPath);
            if (File.Exists(backupPath)) File.Delete(backupPath);
        }

        [Test]
        public void Log_RotatesFile_WhenSizeExceedsThreshold()
        {
            var logPath = Logger.LogFilePath;
            var backupPath = logPath + ".old";

            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

            // Write a dummy log file larger than the 10MB rotation threshold
            var oversizedContent = new string('x', 11 * 1024 * 1024);
            File.WriteAllText(logPath, oversizedContent);

            Logger.Log("Trigger rotation");

            Assert.That(File.Exists(backupPath), Is.True,
                "Expected a backup (.old) log to be created after rotation.");
            Assert.That(new FileInfo(backupPath).Length, Is.GreaterThanOrEqualTo(11 * 1024 * 1024));

            var newContent = File.ReadAllText(logPath);
            Assert.That(newContent, Does.Contain("Trigger rotation"));
            Assert.That(newContent.Length, Is.LessThan(1024),
                "New log file should be freshly started, not carry over the oversized content.");
        }

        [Test]
        public void Log_DoesNotRotate_WhenBelowThreshold()
        {
            var logPath = Logger.LogFilePath;
            var backupPath = logPath + ".old";

            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.WriteAllText(logPath, "Existing small log content\n");

            Logger.Log("Small message");

            Assert.That(File.Exists(backupPath), Is.False,
                "No backup should be created when the log is below the rotation threshold.");

            var content = File.ReadAllText(logPath);
            Assert.That(content, Does.Contain("Existing small log content"));
            Assert.That(content, Does.Contain("Small message"));
        }
    }
}