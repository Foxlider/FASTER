using NUnit.Framework;
using System.Linq;
using System.IO;
using System.Xml.Serialization;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class Arma3ProfileTests
    {
        Arma3Profile _p;
        [SetUp]
        public void Arma3ProfileSetUp()
        { 
		    _p = new Arma3Profile(); 
        }

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
            Assert.That(_p.PrecisionAi, Is.EqualTo(0.5));
            Assert.That(_p.SkillAi, Is.EqualTo(0.5));
        }

        [Test()]
        public void Arma3ProfileSetData()
        {
            _p.AiLevelPreset = ProfileCfgArrays.AiPresetStrings[0];
            _p.ThirdPersonView = ProfileCfgArrays.ThirdPersonStrings[0];
            _p.ReducedDamage = ProfileCfgArrays.EnabledStrings[0];
            _p.GroupIndicators = ProfileCfgArrays.LimitedDistanceStrings[0];
            _p.FriendlyTags = ProfileCfgArrays.LimitedDistanceStrings[0];
            _p.EnemyTags = ProfileCfgArrays.LimitedDistanceStrings[0];
            _p.DetectedMines = ProfileCfgArrays.LimitedDistanceStrings[0];
            _p.Commands = ProfileCfgArrays.FadeOutStrings[0];
            _p.Waypoints = ProfileCfgArrays.FadeOutStrings[0];
            _p.WeaponInfo = ProfileCfgArrays.FadeOutStrings[0];
            _p.StanceIndicator = ProfileCfgArrays.FadeOutStrings[0];
            _p.StaminaBar = ProfileCfgArrays.EnabledStrings[0];
            _p.WeaponCrosshair = ProfileCfgArrays.EnabledStrings[0];
            _p.VisionAid = ProfileCfgArrays.EnabledStrings[0];
            _p.CameraShake = ProfileCfgArrays.EnabledStrings[0];
            _p.ScoreTable = ProfileCfgArrays.EnabledStrings[0];
            _p.DeathMessages = ProfileCfgArrays.EnabledStrings[0];
            _p.VonID = ProfileCfgArrays.EnabledStrings[0];
            _p.MapContentEnemy = ProfileCfgArrays.EnabledStrings[0];
            _p.MapContentFriendly = ProfileCfgArrays.EnabledStrings[0];
            _p.MapContentMines = ProfileCfgArrays.EnabledStrings[0];
            _p.AutoReport = ProfileCfgArrays.EnabledStrings[0];
            _p.MultipleSaves = ProfileCfgArrays.EnabledStrings[0];
            _p.TacticalPing = ProfileCfgArrays.TacticalPingStrings[0];
            _p.SkillAi = 0.5;
            _p.PrecisionAi = 0.5;
        }

		[Test()]
        public void Arma3ProfileDefaults()
        {
            Assert.That(_p.ReducedDamage, Is.EqualTo("Disabled"));
            Assert.That(_p.StaminaBar, Is.EqualTo("Enabled"));
            Assert.That(_p.AiLevelPreset, Is.EqualTo("Custom"));
            Assert.That(_p.TacticalPing, Is.EqualTo("3D only"));
        }

        [Test()]
        public void UnknownValueKeepsCurrentSetting()
        {
            _p.ReducedDamage = "Enabled";
            _p.ReducedDamage = "not a real option";
            Assert.That(_p.ReducedDamage, Is.EqualTo("Enabled"));

            _p.ReducedDamage = null;
            Assert.That(_p.ReducedDamage, Is.EqualTo("Enabled"));

            _p.TacticalPing = "Both";
            _p.TacticalPing = "nonsense";
            Assert.That(_p.TacticalPing, Is.EqualTo("Both"));
        }

        [Test()]
        public void OldProfileWithEnabledTacticalPingLoadsAs3dOnly()
        {
            const string oldXml = "<Arma3Profile><TacticalPing>Enabled</TacticalPing></Arma3Profile>";

            var serializer = new XmlSerializer(typeof(Arma3Profile));
            using var reader = new StringReader(oldXml);
            var loaded = (Arma3Profile) serializer.Deserialize(reader)!;

            Assert.That(loaded.TacticalPing, Is.EqualTo("3D only"));
        }

        [Test()]
        public void OldProfileWithDisabledTacticalPingStaysDisabled()
        {
           const string oldXml = "<Arma3Profile><TacticalPing>Disabled</TacticalPing></Arma3Profile>";

           var serializer = new XmlSerializer(typeof(Arma3Profile));
           using var reader = new StringReader(oldXml);
           var loaded = (Arma3Profile) serializer.Deserialize(reader)!;

           Assert.That(loaded.TacticalPing, Is.EqualTo("Disabled"));
        }
    }
}