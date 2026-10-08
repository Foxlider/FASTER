using System;
using NUnit.Framework;
using System.Linq;
using System.Text.RegularExpressions;

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

        [Test()]
        public void HeadlessClientsSurviveATrailingNewline()
        {
            var cfg = new ServerCfg { HeadlessClientEnabled = true, HeadlessClients = "10.0.0.5\n" };
            var output = cfg.ProcessFile();

            Assert.That(output, Does.Contain("headlessClients[]"));
            Assert.That(output, Does.Contain("\"10.0.0.5\""));
        }

        [Test()]
        public void LocalClientsSurviveATrailingNewline()
        {
            var cfg = new ServerCfg { HeadlessClientEnabled = true, LocalClient = "10.0.0.6\n" };
            var output = cfg.ProcessFile();

            Assert.That(output, Does.Contain("localClient[]"));
            Assert.That(output, Does.Contain("\"10.0.0.6\""));
        }

        [Test()]
        public void HeadlessLinesAreOmittedWhenDisabled()
        {
            var output = new ServerCfg().ProcessFile();

            Assert.That(output, Does.Not.Contain("headlessClients[]"));
            Assert.That(output, Does.Not.Contain("localClient[]"));
        }

        [Test()]
        public void BlankAdminLinesAreIgnored()
        {
            var cfg = new ServerCfg { Admins = "111\n\n222\n" };
            Assert.That(cfg.ProcessFile(), Does.Contain("\"111\",\n\t\"222\""));
        }

        [Test()]
        public void NoAdminsWritesAnEmptyArray()
        {
            Assert.That(new ServerCfg().ProcessFile(), Does.Contain("admins[] = {};"));
        }

		[Test()]
        public void ScriptingFieldsAreEscaped()
        {
            var cfg = new ServerCfg
            {
                OnUserConnected    = "diag_log \"joined\"",
                OnUserDisconnected = "diag_log \"left\"",
                DoubleIdDetected   = "diag_log \"dupe\"",
                OnUserKicked       = "diag_log \"kicked\"",
                OnUnsignedData     = "diag_log \"unsigned\"",
                OnHackedData       = "diag_log \"hacked\"",
                OnDifferentData    = "diag_log \"different\""
            };
            var output = cfg.ProcessFile();

            Assert.That(output, Does.Contain("onUserConnected = \"diag_log \"\"joined\"\"\";"));
            Assert.That(output, Does.Contain("onUserDisconnected = \"diag_log \"\"left\"\"\";"));
            Assert.That(output, Does.Contain("doubleIdDetected = \"diag_log \"\"dupe\"\"\";"));
            Assert.That(output, Does.Contain("onUserKicked = \"diag_log \"\"kicked\"\"\";"));
            Assert.That(output, Does.Contain("onUnsignedData = \"diag_log \"\"unsigned\"\"\";"));
            Assert.That(output, Does.Contain("onHackedData = \"diag_log \"\"hacked\"\"\";"));
            Assert.That(output, Does.Contain("onDifferentData = \"diag_log \"\"different\"\"\";"));
        }

        [Test()]
        public void MissionDownloadUrlIsEscaped()
        {
            var cfg = new ServerCfg { MissionHTTPDownloadBaseURL = "http://example.com/\"missions\"" };
            Assert.That(cfg.ProcessFile(), Does.Contain("missionHTTPDownloadBaseURL = \"http://example.com/\"\"missions\"\"\";"));
        }

        [Test()]
        public void DefaultScriptLinesAreUnchanged()
        {
            var output = new ServerCfg().ProcessFile();

            Assert.That(output, Does.Contain("onHackedData = \"kick (_this select 0)\";"));
            Assert.That(output, Does.Contain("onUnsignedData = \"kick (_this select 0)\";"));
        }

        [Test()]
        public void LegacyLayoutIsRegenerated()
        {
            var cfg = new ServerCfg { IgnoreMissionLoadErrors = true };
            cfg.ServerCfgContent = "class AdvancedOptions\r\n{\r\n\tLogObjectNotFound = True;\r\n};\r\nignoreMissionLoadErrors = True;\r\n";

            Assert.That(cfg.MigrateLegacyLayout(), Is.True);

            var output     = cfg.ServerCfgContent;
            var classStart = output.IndexOf("class AdvancedOptions", StringComparison.Ordinal);
            var classEnd   = output.IndexOf("};", classStart, StringComparison.Ordinal);
            var setting    = output.IndexOf("ignoreMissionLoadErrors = True;", StringComparison.Ordinal);
            Assert.That(setting, Is.GreaterThan(classStart).And.LessThan(classEnd));
        }

        [Test()]
        public void CurrentLayoutIsLeftAlone()
        {
            var cfg = new ServerCfg();
            cfg.ServerCfgContent = cfg.ProcessFile() + "// hand edit";

            Assert.That(cfg.MigrateLegacyLayout(), Is.False);
            Assert.That(cfg.ServerCfgContent, Does.EndWith("// hand edit"));
        }

        [Test()]
        public void IgnoreMissionLoadErrorsIsInsideAdvancedOptions()
        {
            var output = new ServerCfg { IgnoreMissionLoadErrors = true }.ProcessFile();

            var classStart = output.IndexOf("class AdvancedOptions", StringComparison.Ordinal);
            var classEnd   = output.IndexOf("};", classStart, StringComparison.Ordinal);
            var setting    = output.IndexOf("ignoreMissionLoadErrors = True;", StringComparison.Ordinal);

            Assert.That(classStart, Is.GreaterThanOrEqualTo(0));
            Assert.That(setting, Is.GreaterThan(classStart).And.LessThan(classEnd));
        }
        
        [Test()]
        public void MissionsWithSimilarNamesGetUniqueClassNames()
        {
            var cfg = new ServerCfg
            {
                Missions = new List<ProfileMission>
                {
                    new ProfileMission { Name = "A-B", Path = "A-B.pbo", MissionChecked = true },
                    new ProfileMission { Name = "A_B", Path = "A_B.pbo", MissionChecked = true },
                    new ProfileMission { Name = "Мод", Path = "Мод.pbo", MissionChecked = true },
                    new ProfileMission { Name = "Мир", Path = "Мир.pbo", MissionChecked = true }
                }
            };

            var classes = Regex.Matches(cfg.ProcessFile(), @"class (Mission_\S+)")
                               .Select(m => m.Groups[1].Value)
                               .ToList();

            Assert.That(classes, Has.Count.EqualTo(4));
            Assert.That(classes, Is.Unique);
        }
    }
}