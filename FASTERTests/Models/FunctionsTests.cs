using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ModFolderNameTests
    {
        [Test()]
        public void AsciiNamesKeepTheirOldName()
        {
            Assert.That(Functions.ModFolderName("Zeus Enhanced!", 1), Is.EqualTo(Functions.SafeName("Zeus Enhanced!")));
        }

        [Test()]
        public void NonAsciiLettersAddTheWorkshopId()
        {
            Assert.That(Functions.ModFolderName("Café Pack", 42), Is.EqualTo("Caf_Pack_42"));
        }

        [Test()]
        public void FullyNonAsciiNameStillGetsAUsableName()
        {
            Assert.That(Functions.ModFolderName("Мод", 7), Is.EqualTo("mod_7"));
        }

        [Test()]
        public void TwoNonAsciiModsDoNotCollide()
        {
            Assert.That(Functions.ModFolderName("Мод", 7), Is.Not.EqualTo(Functions.ModFolderName("Мир", 8)));
        }
    }
}