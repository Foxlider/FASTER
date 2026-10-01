using NUnit.Framework;

using System.Runtime.Versioning;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class EncryptionTests
    {
        [SupportedOSPlatform("windows7.0")]
        [Test()]
        public void EncryptDataTest()
        {
            Assert.DoesNotThrow(() => Encryption.Instance.EncryptData("SomeText"));
            Assert.That(Encryption.Instance.EncryptData("SomeText"), Is.Not.EqualTo("SomeText"));
        }

        [SupportedOSPlatform("windows7.0")]
        [Test()]
        public void DecryptDataTest()
        {
            var encrypted = Encryption.Instance.EncryptData("SomeText");
            Assert.That(Encryption.Instance.DecryptData(encrypted), Is.EqualTo("SomeText"));
        }

        [SupportedOSPlatform("windows7.0")]
        [Test()]
        public void EncryptedDataUsesCurrentFormat()
        {
            Assert.That(Encryption.Instance.IsCurrentFormat(Encryption.Instance.EncryptData("SomeText")), Is.True);
        }

        [SupportedOSPlatform("windows7.0")]
        [Test()]
        public void MigrateLeavesCurrentFormatAlone()
        {
            var encrypted = Encryption.Instance.EncryptData("SomeText");
            Assert.That(Encryption.Instance.Migrate(encrypted), Is.EqualTo(encrypted));
        }

        [SupportedOSPlatform("windows7.0")]
        [Test()]
        public void MigrateLeavesUnreadableValueAlone()
        {
            Assert.That(Encryption.Instance.Migrate("not-a-real-secret"), Is.EqualTo("not-a-real-secret"));
        }
    }
}