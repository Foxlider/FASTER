using NUnit.Framework;
using System.Linq;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class Arma3ProfileTests
    {
        Arma3Profile _p;
        [OneTimeSetUp]
        public void Arma3ProfileSetUp()
        { Assert.DoesNotThrow(() => _p = new Arma3Profile()); }

        [Test()]
        public void Arma3ProfileCorrectData()
        {
            Assert.That(ProfileCfgArrays.AiPresetStrings.Contains(_p.AiLevelPreset));
            Assert.That(ProfileCfgArrays.ThirdPersonStrings.Contains(_p.ThirdPersonView));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.ReducedDamage));
            Assert.That(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.GroupIndicators));
            Assert.That(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.FriendlyTags));
            Assert.That(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.EnemyTags));
            Assert.That(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.DetectedMines));
            Assert.That(ProfileCfgArrays.FadeOutStrings.Contains(_p.Commands));
            Assert.That(ProfileCfgArrays.FadeOutStrings.Contains(_p.Waypoints));
            Assert.That(ProfileCfgArrays.FadeOutStrings.Contains(_p.WeaponInfo));
            Assert.That(ProfileCfgArrays.FadeOutStrings.Contains(_p.StanceIndicator));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.StaminaBar));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.WeaponCrosshair));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.VisionAid));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.CameraShake));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.ScoreTable));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.DeathMessages));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.VonID));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.MapContentEnemy));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.MapContentFriendly));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.MapContentMines));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.AutoReport));
            Assert.That(ProfileCfgArrays.EnabledStrings.Contains(_p.MultipleSaves));
            Assert.That(ProfileCfgArrays.TacticalPingStrings.Contains(_p.TacticalPing));
            Assert.That(!string.IsNullOrWhiteSpace(_p.ArmaProfileContent));
            Assert.That(_p.PrecisionAi, Is.Not.Null);
            Assert.That(_p.SkillAi, Is.Not.Null);
        }

        [Test()]
        public void Arma3ProfileSetData()
        {
            Assert.DoesNotThrow(() => _p.AiLevelPreset = ProfileCfgArrays.AiPresetStrings[0]);
            Assert.DoesNotThrow(() => _p.ThirdPersonView = ProfileCfgArrays.ThirdPersonStrings[0]);
            Assert.DoesNotThrow(() => _p.ReducedDamage = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.GroupIndicators = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => _p.FriendlyTags = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => _p.EnemyTags = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => _p.DetectedMines = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => _p.Commands = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => _p.Waypoints = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => _p.WeaponInfo = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => _p.StanceIndicator = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => _p.StaminaBar = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.WeaponCrosshair = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.VisionAid = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.CameraShake = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.ScoreTable = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.DeathMessages = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.VonID = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.MapContentEnemy = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.MapContentFriendly = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.MapContentMines = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.AutoReport = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.MultipleSaves = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => _p.TacticalPing = ProfileCfgArrays.TacticalPingStrings[0]);
            Assert.DoesNotThrow(() => _p.SkillAi = 0.5);
            Assert.DoesNotThrow(() => _p.PrecisionAi = 0.5);
        }
    }
}