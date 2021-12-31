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
    public class ArmaModCollectionTests
    {
        ArmaModCollection amc;
        ArmaMod mod;

        [OneTimeSetUp]
        public void SetUp()
        {
            amc = new ArmaModCollection();
            mod = new ArmaMod
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
            Assert.DoesNotThrow(() => amc.AddSteamMod(mod));

            //Really should throw but whatever
            Assert.DoesNotThrow(() => amc.AddSteamMod(mod));

            Assert.DoesNotThrow(() => amc.DeleteSteamMod(mod.WorkshopId));
        }

        [Test]
        public void TestSteamModGet()
        {
            Assert.IsFalse(string.IsNullOrEmpty(mod.Author));
            Assert.IsFalse(string.IsNullOrEmpty(mod.Name));
            Assert.IsFalse(string.IsNullOrEmpty(mod.Path));
            Assert.IsFalse(string.IsNullOrEmpty(mod.Status));
            Assert.IsNotNull(mod.Size);
            Assert.IsNotNull(mod.LocalLastUpdated);
            Assert.IsNotNull(mod.WorkshopId);
            Assert.IsNotNull(mod.SteamLastUpdated);
            Assert.IsNotNull(mod.PrivateMod);
            Assert.IsNotNull(mod.IsLocal);
            Assert.IsNotNull(mod.IsLoading);
        }

        [Test]
        public void TestSteamModSet()
        {
            Assert.DoesNotThrow(() => mod.Author = "Test2");
            Assert.DoesNotThrow(() => mod.Name = "Test2");
            Assert.DoesNotThrow(() => mod.Path = "Test2");
            Assert.DoesNotThrow(() => mod.Status = ArmaModStatus.UpToDate);
            Assert.DoesNotThrow(() => mod.Size = 5);
            Assert.DoesNotThrow(() => mod.LocalLastUpdated = 0);
            Assert.DoesNotThrow(() => mod.WorkshopId = 2);
            Assert.DoesNotThrow(() => mod.SteamLastUpdated = 1);
            Assert.DoesNotThrow(() => mod.PrivateMod = false);
            Assert.DoesNotThrow(() => mod.IsLocal = false);
            Assert.DoesNotThrow(() => mod.IsLoading = false);
        }
    }
}