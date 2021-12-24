using NUnit.Framework;
using FASTER.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class Arma3ProfileTests
    {
        Arma3Profile p;
        [OneTimeSetUp]
        public void Arma3ProfileSetUp()
        { Assert.DoesNotThrow(() => p = new Arma3Profile()); }

        [Test()]
        public void Arma3ProfileCorrectData()
        {
            Assert.IsTrue(ProfileCfgArrays.AiPresetStrings.Contains(p.AiLevelPreset));
            Assert.IsTrue(ProfileCfgArrays.ThirdPersonStrings.Contains(p.ThirdPersonView));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.ReducedDamage));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(p.GroupIndicators));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(p.FriendlyTags));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(p.EnemyTags));
            Assert.IsTrue(ProfileCfgArrays.LimitedDistanceStrings.Contains(p.DetectedMines));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(p.Commands));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(p.Waypoints));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(p.WeaponInfo));
            Assert.IsTrue(ProfileCfgArrays.FadeOutStrings.Contains(p.StanceIndicator));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.StaminaBar));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.WeaponCrosshair));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.VisionAid));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.CameraShake));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.ScoreTable));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.DeathMessages));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.VonID));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.MapContentEnemy));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.MapContentFriendly));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.MapContentMines));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.AutoReport));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.MultipleSaves));
            Assert.IsTrue(ProfileCfgArrays.EnabledStrings.Contains(p.TacticalPing));
            Assert.IsFalse(string.IsNullOrWhiteSpace(p.ArmaProfileContent));
            Assert.IsNotNull(p.PrecisionAi);
            Assert.IsNotNull(p.SkillAi);
        }

        [Test()]
        public void Arma3ProfileSetData()
        {
            Assert.DoesNotThrow(() => p.AiLevelPreset = ProfileCfgArrays.AiPresetStrings[0]);
            Assert.DoesNotThrow(() => p.ThirdPersonView = ProfileCfgArrays.ThirdPersonStrings[0]);
            Assert.DoesNotThrow(() => p.ReducedDamage = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.GroupIndicators = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => p.FriendlyTags = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => p.EnemyTags = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => p.DetectedMines = ProfileCfgArrays.LimitedDistanceStrings[0]);
            Assert.DoesNotThrow(() => p.Commands = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => p.Waypoints = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => p.WeaponInfo = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => p.StanceIndicator = ProfileCfgArrays.FadeOutStrings[0]);
            Assert.DoesNotThrow(() => p.StaminaBar = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.WeaponCrosshair = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.VisionAid = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.CameraShake = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.ScoreTable = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.DeathMessages = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.VonID = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.MapContentEnemy = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.MapContentFriendly = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.MapContentMines = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.AutoReport = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.MultipleSaves = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.TacticalPing = ProfileCfgArrays.EnabledStrings[0]);
            Assert.DoesNotThrow(() => p.SkillAi = 0.5);
            Assert.DoesNotThrow(() => p.PrecisionAi = 0.5);
        }
    }
}