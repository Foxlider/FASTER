using NUnit.Framework;
using FASTER.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class EncryptionTests
    {
        [Test()]
        public void EncryptDataTest()
        {
            Assert.DoesNotThrow(() => Encryption.Instance.EncryptData("SomeText"));
            Assert.AreNotEqual("SomeText", Encryption.Instance.EncryptData("SomeText"));
        }

        [Test()]
        public void DecryptDataTest()
        {
            var encrypted = Encryption.Instance.EncryptData("SomeText");
            Assert.AreEqual("SomeText", Encryption.Instance.DecryptData(encrypted));
        }
    }
}