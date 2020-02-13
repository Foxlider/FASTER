using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class SteamWebApiTests
    {
        [Test()]
        public void GetSingleFileDetailsTest()
        {
            var res = SteamWebApi.GetSingleFileDetails(463939057);
            Assert.AreEqual("463939057", res["publishedfileid"].ToString());
            Assert.AreEqual("76561198194647182", res["creator"].ToString());
            Assert.AreEqual("ace", res["title"].ToString());
            Assert.AreEqual("107410", res["creator_appid"].ToString());
        }
    }
}