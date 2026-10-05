using NUnit.Framework;

using System;
using System.IO;
using System.Threading.Tasks;

namespace FASTER.Models.Tests
{
    [TestFixture]
    public class LoggerTests
    {
        private const int OverThreshold = 11 * 1024 * 1024; // rotation threshold is 10 MB

        private string _tempDir;
        private string _logPath;
        private string _backupPath;

        [SetUp]
        public void SetUp()
        {
            _tempDir    = Path.Combine(Path.GetTempPath(), $"FASTER_LoggerTests_{Guid.NewGuid():N}");
            _logPath    = Path.Combine(_tempDir, "faster.log");
            _backupPath = _logPath + ".old";
            Directory.CreateDirectory(_tempDir);

            Logger.ConfigureForTesting(_logPath, enabled: true);
        }

        [TearDown]
        public void TearDown()
        {
            Logger.ConfigureForTesting(); // back to the real log file and setting
            Directory.Delete(_tempDir, true);
        }

        private static void CreateFileOfSize(string path, long bytes)
        {
            using var stream = new FileStream(path, FileMode.Create);
            stream.SetLength(bytes);
        }

        [Test]
        public void Log_RotatesFile_WhenSizeExceedsThreshold()
        {
            CreateFileOfSize(_logPath, OverThreshold);

            Logger.Log("Trigger rotation");

            Assert.That(File.Exists(_backupPath), Is.True, "Expected a .old backup after rotation.");
            Assert.That(new FileInfo(_backupPath).Length, Is.GreaterThanOrEqualTo(OverThreshold));

            var newContent = File.ReadAllText(_logPath);
            Assert.That(newContent, Does.Contain("Trigger rotation"));
            Assert.That(newContent.Length, Is.LessThan(1024), "New log should start fresh.");
        }

        [Test]
        public void Log_DoesNotRotate_WhenBelowThreshold()
        {
            File.WriteAllText(_logPath, "Existing small log content\n");

            Logger.Log("Small message");

            Assert.That(File.Exists(_backupPath), Is.False);

            var content = File.ReadAllText(_logPath);
            Assert.That(content, Does.Contain("Existing small log content"));
            Assert.That(content, Does.Contain("Small message"));
        }

        [Test]
        public void Rotation_ReplacesAnExistingBackup()
        {
            File.WriteAllText(_backupPath, "stale backup");
            CreateFileOfSize(_logPath, OverThreshold);

            Logger.Log("Trigger rotation");

            Assert.That(new FileInfo(_backupPath).Length, Is.GreaterThanOrEqualTo(OverThreshold));
        }

        [Test]
        public void Log_WritesNothing_WhenDisabled()
        {
            Logger.ConfigureForTesting(_logPath, enabled: false);

            Logger.Log("Should not appear");

            Assert.That(File.Exists(_logPath), Is.False);
        }

        [Test]
        public void LogCritical_WritesEvenWhenDisabled()
        {
            Logger.ConfigureForTesting(_logPath, enabled: false);

            Logger.LogCritical("Fatal thing");

            Assert.That(File.ReadAllText(_logPath), Does.Contain("Fatal thing"));
        }

        [Test]
        public void Log_KeepsEveryLine_WhenCalledFromManyThreads()
        {
            Parallel.For(0, 200, i => Logger.Log($"line {i}"));

            Assert.That(File.ReadAllLines(_logPath).Length, Is.EqualTo(200));
        }
    }
}