using NUnit.Framework;

using System;
using System.IO;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class PresetLinkParsingTests
    {
        private static System.Collections.Generic.List<ArmaMod> Parse(string href)
        {
            var path = Path.Combine(Path.GetTempPath(), $"FASTER_Testing_{Guid.NewGuid():N}.html");
            File.WriteAllText(path,
                "<?xml version=\"1.0\"?><html><body><table><tr data-type=\"ModContainer\">"
                + "<td data-type=\"DisplayName\">ace</td>"
                + $"<td><a href=\"{href}\" data-type=\"Link\">link</a></td>"
                + "</tr></table></body></html>");
            try
            { return ModUtilities.ParseModsFromArmaProfileFile(path); }
            finally
            { File.Delete(path); }
        }

        [Test()]
        public void IdAfterAnotherQueryParameterStillParses()
        {
            var mods = Parse("https://steamcommunity.com/sharedfiles/filedetails/?l=english&amp;id=463939057");

            Assert.That(mods, Has.Count.EqualTo(1));
            Assert.That(mods[0].WorkshopId, Is.EqualTo(463939057u));
            Assert.That(mods[0].IsLocal, Is.False);
        }

        [Test()]
        public void LinkWithoutAnIdIsTreatedAsLocal()
        {
            var mods = Parse("https://example.com/mod");

            Assert.That(mods, Has.Count.EqualTo(1));
            Assert.That(mods[0].IsLocal, Is.True);
            Assert.That(mods[0].WorkshopId, Is.Not.EqualTo(0u));
        }
    }
}