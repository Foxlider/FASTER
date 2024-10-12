using NUnit.Framework;
// ReSharper disable CheckNamespace
// ReSharper disable PossibleNullReferenceException

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class SteamWebApiTests
    {
        [Test()]
        public void GetSingleFileDetailsTest()
        {
            var res = SteamWebApi.GetSingleFileDetails(463939057);
            Assert.That(res, Is.Not.Null);
            Assert.That(res["publishedfileid"].ToString(), Is.EqualTo("463939057"));
            Assert.That(res["creator"].ToString(), Is.EqualTo("76561198194647182"));
            Assert.That(res["title"].ToString(), Is.EqualTo("ace"));
            Assert.That(res["creator_appid"].ToString(), Is.EqualTo("107410"));
        }
    }
}