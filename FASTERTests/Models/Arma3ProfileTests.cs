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
            Assert.IsTrue(ProfileCfgArrays.AiPresetStrings.Contains(_p.AiLevelPreset));
            Assert.IsTrue(ProfileCfgArrays.ThirdPersonStrings.Contains(_p.ThirdPersonView));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.ReducedDamage));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.GroupIndicators));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.FriendlyTags));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.EnemyTags));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(_p.DetectedMines));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(_p.Commands));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(_p.Waypoints));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(_p.WeaponInfo));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(_p.StanceIndicator));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.StaminaBar));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.WeaponCrosshair));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.VisionAid));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.CameraShake));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.ScoreTable));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.DeathMessages));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.VonID));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.MapContentEnemy));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.MapContentFriendly));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.MapContentMines));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.AutoReport));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(_p.MultipleSaves));
            Assert.IsTrue(ProfileCfgArrays.TacticalPingStrings.Contains(_p.TacticalPing));
            Assert.IsFalse(string.IsNullOrWhiteSpace(_p.ArmaProfileContent));
            Assert.IsNotNull(_p.PrecisionAi);
            Assert.IsNotNull(_p.SkillAi);
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