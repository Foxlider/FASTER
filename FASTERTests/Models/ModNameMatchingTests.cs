using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ModNameMatchingTests
    {
        [Test()]
        public void PunctuationAndAtSignAreIgnored()
        {
            Assert.That(ModUtilities.NamesMatch("@CBA_A3", "CBA A3"), Is.True);
        }

        [Test()]
        public void DifferentNonAsciiNamesDoNotMatch()
        {
            Assert.That(ModUtilities.NamesMatch("Мод", "Мир"), Is.False);
            Assert.That(ModUtilities.NamesMatch("Мод", "Мод"), Is.True);
        }

        [Test()]
        public void NamesWithNothingLeftNeverMatch()
        {
            Assert.That(ModUtilities.NamesMatch("!!!", "???"), Is.False);
            Assert.That(ModUtilities.NamesMatch("", ""), Is.False);
            Assert.That(ModUtilities.NamesMatch(null, null), Is.False);
        }
    }
}