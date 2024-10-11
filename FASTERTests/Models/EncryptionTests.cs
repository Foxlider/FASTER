using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class EncryptionTests
    {
        [Test()]
        public void EncryptDataTest()
        {
            Assert.DoesNotThrow(() => Encryption.Instance.EncryptData("SomeText"));
            Assert.That(Encryption.Instance.EncryptData("SomeText"), Is.EqualTo("SomeText"));
        }

        [Test()]
        public void DecryptDataTest()
        {
            var encrypted = Encryption.Instance.EncryptData("SomeText");
            Assert.That(Encryption.Instance.DecryptData(encrypted), Is.EqualTo("SomeText"));
        }
    }
}