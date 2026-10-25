using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class LocalModNameTests
    {
        [TestCase(@"C:\Mods\@cba",   "cba")]
        [TestCase(@"C:\Mods\cba",    "cba")]
        [TestCase(@"C:\Mods\@cba\",  "cba")]
        [TestCase(@"C:\Mods\@@odd",  "odd")]
        [TestCase("",                "")]
        [TestCase(null,              "")]
        public void NameIsTheFolderWithoutTheAtSign(string path, string expected)
        {
            Assert.That(ModUtilities.LocalModName(path), Is.EqualTo(expected));
        }
    }
}