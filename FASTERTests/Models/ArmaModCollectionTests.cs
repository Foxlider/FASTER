using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ArmaModCollectionTests
    {
        ArmaModCollection _amc;
        ArmaMod _mod;

        [OneTimeSetUp]
        public void SetUp()
        {
            _amc = new ArmaModCollection();
            _mod = new ArmaMod
            {
                Author = "Test",
                IsLocal = false,
                Name = "Test",
                Path = "Test",
                WorkshopId = 1
            };
        }

        [Test()]
        public void AddSteamModTest()
        {
            Assert.DoesNotThrow(() => _amc.AddSteamMod(_mod));

            //Really should throw but whatever
            Assert.DoesNotThrow(() => _amc.AddSteamMod(_mod));

            Assert.DoesNotThrow(() => _amc.DeleteSteamMod(_mod.WorkshopId));
        }

        [Test]
        public void TestSteamModGet()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_mod.Author));
            Assert.IsFalse(string.IsNullOrEmpty(_mod.Name));
            Assert.IsFalse(string.IsNullOrEmpty(_mod.Path));
            Assert.IsFalse(string.IsNullOrEmpty(_mod.Status));
            Assert.IsNotNull(_mod.Size);
            Assert.IsNotNull(_mod.LocalLastUpdated);
            Assert.IsNotNull(_mod.WorkshopId);
            Assert.IsNotNull(_mod.SteamLastUpdated);
            Assert.IsNotNull(_mod.PrivateMod);
            Assert.IsNotNull(_mod.IsLocal);
            Assert.IsNotNull(_mod.IsLoading);
        }

        [Test]
        public void TestSteamModSet()
        {
            Assert.DoesNotThrow(() => _mod.Author = "Test2");
            Assert.DoesNotThrow(() => _mod.Name = "Test2");
            Assert.DoesNotThrow(() => _mod.Path = "Test2");
            Assert.DoesNotThrow(() => _mod.Status = ArmaModStatus.UpToDate);
            Assert.DoesNotThrow(() => _mod.Size = 5);
            Assert.DoesNotThrow(() => _mod.LocalLastUpdated = 0);
            Assert.DoesNotThrow(() => _mod.WorkshopId = 2);
            Assert.DoesNotThrow(() => _mod.SteamLastUpdated = 1);
            Assert.DoesNotThrow(() => _mod.PrivateMod = false);
            Assert.DoesNotThrow(() => _mod.IsLocal = false);
            Assert.DoesNotThrow(() => _mod.IsLoading = false);
        }
    }
}