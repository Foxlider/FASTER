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
    }
}