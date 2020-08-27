using System;
using NUnit.Framework;
// ReSharper disable CheckNamespace

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class SteamModTests
    {
        [Test()]
        public void SteamIdFromUrlTest()
        {
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?id=463939057"));
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?id=463939057&l=french"));
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?l=french&id=463939057"));
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?l=english&id=463939057&l=french"));
        }

        [Test()]
        public void GetModInfoTest()
        {
            Tuple<string, string, int> expected = new Tuple<string, string, int>("ace", "acemod", 1577907553);
            Tuple<string, string, int> res = null;
            Assert.DoesNotThrow(() => { res = SteamMod.GetModInfo(463939057); });
            Assert.AreEqual(expected.Item1, res.Item1, "The expected mod name was wrong");
            Assert.AreEqual(expected.Item2, res.Item2, "The expected creator name was wrong");
            Assert.GreaterOrEqual(res.Item3, expected.Item3, "The expected update time was wrong");
        }
    }
}