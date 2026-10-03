using NUnit.Framework;

using System.Globalization;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class CultureIndependenceTests
    {
        [Test()]
        public void DecimalValuesIgnoreCurrentCulture()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

                var profile = new Arma3Profile { SkillAi = 0.7, PrecisionAi = 0.3 };
                Assert.That(profile.ProcessFile(), Does.Contain("skillAI = 0.7;"));

                var cfg = new ServerCfg { VotingEnabled = true, VoteThreshold = 0.5 };
                Assert.That(cfg.ProcessFile(), Does.Contain("voteThreshold = 0.5;"));
            }
            finally
            { CultureInfo.CurrentCulture = previous; }
        }
    }
}