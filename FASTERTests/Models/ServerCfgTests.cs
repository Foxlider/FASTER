using NUnit.Framework;

using System.Collections.Generic;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ServerCfgTests
    {
        [Test()]
        public void RequiredBuildLineIsWrittenWithSemicolon()
        {
            var cfg = new ServerCfg { RequiredBuildChecked = true, RequiredBuild = 12345 };
            Assert.That(cfg.ProcessFile(), Does.Contain("requiredBuild = 12345;"));
        }

        [Test()]
        public void RequiredBuildLineIsOmittedWhenUnchecked()
        {
            var cfg = new ServerCfg { RequiredBuild = 12345 };
            Assert.That(cfg.ProcessFile(), Does.Not.Contain("requiredBuild"));
        }

        [Test()]
        public void QuotesInTextFieldsAreEscaped()
        {
            var cfg = new ServerCfg
            {
                Hostname      = "My \"Best\" Server",
                Password      = "pa\"ss",
                PasswordAdmin = "ad\"min"
            };
            var output = cfg.ProcessFile();

            Assert.That(output, Does.Contain("hostname = \"My \"\"Best\"\" Server\";"));
            Assert.That(output, Does.Contain("password = \"pa\"\"ss\";"));
            Assert.That(output, Does.Contain("passwordAdmin = \"ad\"\"min\";"));
        }

        [Test()]
        public void AdminsAndMotdAreEscaped()
        {
            var cfg = new ServerCfg { Admins = "12\"34", Motd = "say \"hi\"" };
            var output = cfg.ProcessFile();

            Assert.That(output, Does.Contain("\"12\"\"34\""));
            Assert.That(output, Does.Contain("\"say \"\"hi\"\"\""));
        }

        [Test()]
        public void DefaultsMatchDocumentedValues()
        {
            var output = new ServerCfg().ProcessFile();

            Assert.That(output, Does.Contain("upnp = 0;"));
            Assert.That(output, Does.Contain("voteThreshold = 0;"));
            Assert.That(output, Does.Contain("maxPlayers = 32;"));
            Assert.That(output, Does.Contain("verifySignatures = 0;"));
        }

        [Test()]
        public void UnknownEnumValueKeepsCurrentSetting()
        {
            var cfg = new ServerCfg { AllowedFilePatching = "All Clients" };
            cfg.AllowedFilePatching = "garbage";

            Assert.That(cfg.AllowedFilePatching, Is.EqualTo("All Clients"));
        }

        [Test()]
        public void OnlyCheckedMissionsAreListed()
        {
            var cfg = new ServerCfg
            {
                Missions = new List<ProfileMission>
                {
                    new ProfileMission { Name = "Alpha", Path = "Alpha.pbo", MissionChecked = true },
                    new ProfileMission { Name = "Bravo", Path = "Bravo.pbo", MissionChecked = false }
                }
            };
            var output = cfg.ProcessFile();

            Assert.That(output, Does.Contain("template = \"Alpha\""));
            Assert.That(output, Does.Not.Contain("template = \"Bravo\""));
        }

        [Test()]
        public void ChangingDifficultyUpdatesMissionBlock()
        {
            var cfg = new ServerCfg
            {
                Missions = new List<ProfileMission>
                {
                    new ProfileMission { Name = "Alpha", Path = "Alpha.pbo", MissionChecked = true }
                }
            };

            cfg.Difficulty = "Recruit";
            Assert.That(cfg.ProcessFile(), Does.Contain("difficulty = \"Recruit\""));

            // Recruit and Veteran are both 7 characters long
            cfg.Difficulty = "Veteran";
            var output = cfg.ProcessFile();
            Assert.That(output, Does.Contain("difficulty = \"Veteran\""));
            Assert.That(output, Does.Not.Contain("difficulty = \"Recruit\""));
        }

        [Test()]
        public void SwappingMissionsOfSameNameLengthUpdatesMissionBlock()
        {
            var alpha = new ProfileMission { Name = "Alpha", Path = "Alpha.pbo", MissionChecked = true };
            var bravo = new ProfileMission { Name = "Bravo", Path = "Bravo.pbo", MissionChecked = false };
            var cfg = new ServerCfg { Missions = new List<ProfileMission> { alpha, bravo } };

            Assert.That(cfg.ProcessFile(), Does.Contain("template = \"Alpha\""));

            alpha.MissionChecked = false;
            bravo.MissionChecked = true;

            var output = cfg.ProcessFile();
            Assert.That(output, Does.Contain("template = \"Bravo\""));
            Assert.That(output, Does.Not.Contain("template = \"Alpha\""));
        }
    }
}