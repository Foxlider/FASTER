using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ArmaModCollectionTests
    {
        ArmaModCollection _amc;
        ArmaMod _mod;

        [SetUp]
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
        [Category("Network")] // AddSteamMod starts a background Steam API lookup
        public void AddSteamModTest()
        {
            uint workshopId = (uint)System.Random.Shared.Next(2, int.MaxValue);
            _mod.WorkshopId = workshopId;
            Assert.DoesNotThrow(() => _amc.AddSteamMod(_mod));

            Assert.DoesNotThrow(() => _amc.DeleteSteamMod(workshopId));
        }

        [Test]
        public void TestSteamModGet()
        {
            Assert.That(_mod.Author, Is.EqualTo("Test"));
            Assert.That(_mod.Name, Is.EqualTo("Test"));
            Assert.That(_mod.Path, Is.EqualTo("Test"));
            Assert.That(_mod.Status, Is.EqualTo("Not Installed"));
            Assert.That(_mod.Size, Is.EqualTo(0));
            Assert.That(_mod.LocalLastUpdated, Is.EqualTo(0));
            Assert.That(_mod.WorkshopId, Is.EqualTo(1));
            Assert.That(_mod.SteamLastUpdated, Is.EqualTo(0));
            Assert.That(_mod.PrivateMod, Is.False);
            Assert.That(_mod.IsLocal, Is.False);
            Assert.That(_mod.IsLoading, Is.False);
        }

        [Test]
        public void TestSteamModSet()
        {
            _mod.Author = "Test2";
            _mod.Name = "Test2";
            _mod.Path = "Test2";
            _mod.Status = ArmaModStatus.UpToDate;
            _mod.Size = 5;
            _mod.LocalLastUpdated = 7;
            _mod.WorkshopId = 2;
            _mod.SteamLastUpdated = 1;
            _mod.PrivateMod = true;
            _mod.IsLocal = true;
            _mod.IsLoading = true;


            Assert.That(_mod.Author, Is.EqualTo("Test2"));
            Assert.That(_mod.Name, Is.EqualTo("Test2"));
            Assert.That(_mod.Path, Is.EqualTo("Test2"));
            Assert.That(_mod.Status, Is.EqualTo(ArmaModStatus.UpToDate));
            Assert.That(_mod.Size, Is.EqualTo(5));
            Assert.That(_mod.LocalLastUpdated, Is.EqualTo(7));
            Assert.That(_mod.WorkshopId, Is.EqualTo(2));
            Assert.That(_mod.SteamLastUpdated, Is.EqualTo(1));
            Assert.That(_mod.PrivateMod, Is.True);
            Assert.That(_mod.IsLocal, Is.True);
            Assert.That(_mod.IsLoading, Is.True);
        }
    }
}
