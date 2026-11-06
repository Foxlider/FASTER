using NUnit.Framework;
using System.Runtime.Versioning;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    [SupportedOSPlatform("windows")]
    public class EncryptionTests
    {

        [Test()]
        public void EncryptDataTest()
        {
            Encryption.Instance.EncryptData("SomeText");
            Assert.That(Encryption.Instance.EncryptData("SomeText"), Is.Not.EqualTo("SomeText"));
        }

        [Test()]
        public void DecryptDataTest()
        {
            var encrypted = Encryption.Instance.EncryptData("SomeText");
            Assert.That(Encryption.Instance.DecryptData(encrypted), Is.EqualTo("SomeText"));
        }

        [Test()]
        public void EncryptedDataUsesCurrentFormat()
        {
            Assert.That(Encryption.Instance.IsCurrentFormat(Encryption.Instance.EncryptData("SomeText")), Is.True);
        }

        [Test()]
        public void MigrateLeavesCurrentFormatAlone()
        {
            var encrypted = Encryption.Instance.EncryptData("SomeText");
            Assert.That(Encryption.Instance.Migrate(encrypted), Is.EqualTo(encrypted));
        }

        [Test()]
        public void MigrateLeavesUnreadableValueAlone()
        {
            Assert.That(Encryption.Instance.Migrate("not-a-real-secret"), Is.EqualTo("not-a-real-secret"));
        }
    }
}