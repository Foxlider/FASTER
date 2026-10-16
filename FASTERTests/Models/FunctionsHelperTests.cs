using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class FunctionsHelperTests
    {
        [TestCase("b",   0, 1)]
        [TestCase("zzz", 2, 2)]
        [TestCase(null,  1, 1)]
        public void IndexOrKeepFindsTheValueOrKeepsCurrent(string value, int current, int expected)
        {
            Assert.That(Functions.IndexOrKeep(new[] { "a", "b", "c" }, value, current), Is.EqualTo(expected));
        }

        [Test()]
        public void OpenFolderRefusesFoldersThatDoNotExist()
        {
            Assert.That(Functions.OpenFolder(@"C:\definitely\not\a\real\folder"), Is.False);
            Assert.That(Functions.OpenFolder(""),                                 Is.False);
            Assert.That(Functions.OpenFolder(null),                               Is.False);
        }
    }
}