using NUnit.Framework;

using System.Linq;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ProfileModsFilterTests
    {
        private static ServerProfile ProfileWith(params string[] names)
        {
            var p = new ServerProfile("Prod", false);
            p.ProfileMods = names.Select((n, i) => new ProfileMod { Id = (uint) (i + 1), Name = n }).ToList();
            return p;
        }

        private static string[] Names(ServerProfile p) => p.FilteredProfileMods.Select(m => m.Name).ToArray();

        [Test()]
        public void EmptyFilterReturnsEverything()
        {
            Assert.That(Names(ProfileWith("ace", "Zeus")), Is.EqualTo(new[] { "ace", "Zeus" }));
        }

        [Test()]
        public void PlainTextIgnoresCaseByDefault()
        {
            var p = ProfileWith("ace", "ACE Compat", "Zeus");
            p.ProfileModsFilter = "ace";
            Assert.That(Names(p), Is.EqualTo(new[] { "ace", "ACE Compat" }));
        }

        [Test()]
        public void CaseSensitiveOnlyMatchesExactCase()
        {
            var p = ProfileWith("ace", "ACE Compat");
            p.ProfileModsFilter                 = "ace";
            p.ProfileModsFilterIsCaseSensitive  = true;
            Assert.That(Names(p), Is.EqualTo(new[] { "ace" }));
        }

        [Test()]
        public void WholeWordSkipsPartialMatches()
        {
            var p = ProfileWith("ace", "palace", "ace compat");
            p.ProfileModsFilter            = "ace";
            p.ProfileModsFilterIsWholeWord = true;
            Assert.That(Names(p), Is.EqualTo(new[] { "ace", "ace compat" }));
        }

        [Test()]
        public void SpecialCharactersAreLiteralUnlessRegexIsOn()
        {
            var p = ProfileWith("a.b", "axb");
            p.ProfileModsFilter = "a.b";
            Assert.That(Names(p), Is.EqualTo(new[] { "a.b" }));

            p.ProfileModsFilterIsRegex = true;
            Assert.That(Names(p), Is.EqualTo(new[] { "a.b", "axb" }));
        }

        [Test()]
        public void InvalidRegexReturnsNothingAndRecovers()
        {
            var p = ProfileWith("ace");
            p.ProfileModsFilterIsRegex = true;
            p.ProfileModsFilter        = "(";

            Assert.That(Names(p), Is.Empty);
            Assert.That(p.ProfileModsFilterIsInvalid, Is.True);

            p.ProfileModsFilter = "ace";
            Assert.That(Names(p), Is.EqualTo(new[] { "ace" }));
            Assert.That(p.ProfileModsFilterIsInvalid, Is.False);
        }
    }
}