using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using BytexDigital.Steam.Core;
using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture]
    [SupportedOSPlatform("windows")]
    public class AuthCodeProviderTests
    {
        private const string StorageName = "test-account";

        [Test]
        public async Task PersistAccessTokenStoresDpapiProtectedToken()
        {
            using var directory = new TemporaryDirectory();
            var authenticator = CreateAuthenticator(directory.DirectoryPath);
            const string token = "test-access-token";

            await authenticator.PersistAccessTokenAsync(token);

            var path = GetAccessTokenPath(directory.DirectoryPath);
            var storedValue = File.ReadAllText(path);

            Assert.That(File.Exists(path), Is.True);
            Assert.That(Encryption.Instance.IsCurrentFormat(storedValue), Is.True);
            Assert.That(Encryption.Instance.DecryptData(storedValue), Is.EqualTo(token));
            Assert.That(await authenticator.GetAccessTokenAsync(), Is.EqualTo(token));
        }

        [Test]
        public async Task PersistEmptyAccessTokenDeletesFileAndClearsMemory()
        {
            using var directory = new TemporaryDirectory();
            var authenticator = CreateAuthenticator(directory.DirectoryPath);
            const string token = "test-access-token";
            const string guardData = "test-guard-data";

            await authenticator.PersistAccessTokenAsync(token);
            await authenticator.PersistGuardDataAsync(guardData);

            await authenticator.PersistAccessTokenAsync(string.Empty);

            Assert.That(File.Exists(GetAccessTokenPath(directory.DirectoryPath)), Is.False);
            Assert.That(await authenticator.GetAccessTokenAsync(), Is.Empty);

            Assert.That(File.Exists(GetGuardDataPath(directory.DirectoryPath)), Is.True);
            Assert.That(await authenticator.GetGuardDataAsync(), Is.EqualTo(guardData));
        }

        [Test]
        public async Task PersistEmptyAccessTokenWithoutDirectoryClearsMemory()
        {
            var authenticator = CreateAuthenticator(string.Empty);

            await authenticator.PersistAccessTokenAsync("test-access-token");

            await authenticator.PersistAccessTokenAsync(string.Empty);

            Assert.That(await authenticator.GetAccessTokenAsync(), Is.Empty);
        }

        [Test]
        public async Task PersistEmptyAccessTokenSucceedsWhenFileIsMissing()
        {
            using var directory = new TemporaryDirectory();
            var authenticator = CreateAuthenticator(directory.DirectoryPath);
            const string token = "test-access-token";

            await authenticator.PersistAccessTokenAsync(token);

            File.Delete(GetAccessTokenPath(directory.DirectoryPath));

            await authenticator.PersistAccessTokenAsync(string.Empty);

            Assert.That(await authenticator.GetAccessTokenAsync(), Is.Empty);
            Assert.That(File.Exists(GetAccessTokenPath(directory.DirectoryPath)), Is.False);
        }

        [Test]
        public async Task CancelledTokenClearPreservesExistingToken()
        {
            using var directory = new TemporaryDirectory();
            var authenticator = CreateAuthenticator(directory.DirectoryPath);
            const string token = "test-access-token";

            await authenticator.PersistAccessTokenAsync(token);

            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await authenticator.PersistAccessTokenAsync(
                    string.Empty,
                    cancellationTokenSource.Token));

            Assert.That(await authenticator.GetAccessTokenAsync(), Is.EqualTo(token));
            Assert.That(File.Exists(GetAccessTokenPath(directory.DirectoryPath)), Is.True);
        }

        private static SteamAuthenticator CreateAuthenticator(string persistenceDirectory)
        {
            var authenticatorType = typeof(Encryption).Assembly.GetType(
                "FASTER.Models.AuthCodeProvider",
                throwOnError: true)!;

            return (SteamAuthenticator)(Activator.CreateInstance(
                authenticatorType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                args: new object[] { StorageName, persistenceDirectory },
                culture: null)
                ?? throw new InvalidOperationException("Could not create AuthCodeProvider."));
        }

        private static string GetAccessTokenPath(string directoryPath)
        {
            return Path.Combine(directoryPath, $"{StorageName}_accesstoken");
        }

        private static string GetGuardDataPath(string directoryPath)
        {
            return Path.Combine(directoryPath, $"{StorageName}_guarddata");
        }

        private sealed class TemporaryDirectory : IDisposable
        {
            public string DirectoryPath { get; } = Path.Combine(
                Path.GetTempPath(),
                $"FASTERTests_{Guid.NewGuid():N}");

            public TemporaryDirectory()
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            public void Dispose()
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, recursive: true);
                }
            }
        }
    }
}