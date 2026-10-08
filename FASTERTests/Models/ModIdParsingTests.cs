using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ModIdParsingTests
    {
        [TestCase("463939057", 463939057u)]
        [TestCase("  463939057  ", 463939057u)]
        [TestCase("https://steamcommunity.com/sharedfiles/filedetails/?id=463939057", 463939057u)]
        [TestCase("https://steamcommunity.com/workshop/filedetails/?l=french&id=463939057", 463939057u)]
        public void ValidInputsAreParsed(string input, uint expected)
        {
            Assert.That(ModUtilities.TryParseModId(input, out var id), Is.True);
            Assert.That(id, Is.EqualTo(expected));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("abc")]
        [TestCase("0")]
        [TestCase("-5")]
        [TestCase("https://example.com/?id=463939057")]
        [TestCase("https://evilsteamcommunity.com/?id=463939057")]
        [TestCase("https://steamcommunity.com/workshop/filedetails/")]
        [TestCase("https://steamcommunity.com/workshop/filedetails/?id=abc")]
        public void InvalidInputsAreRejected(string input)
        {
            Assert.That(ModUtilities.TryParseModId(input, out _), Is.False);
        }
    }
}