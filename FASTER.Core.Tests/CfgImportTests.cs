using System.Globalization;
using FASTER.Models;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
public class CfgImportTests
{
    [Test]
    public void ServerCfgAppliesLiteralsArraysAndNestedOptionsWithoutRewritingThePaste()
    {
        const string text = """
            // maxPlayers = 32;
            hostname = "A ""quoted"" server"; maxPlayers = 225;
            admins[]={"123", /* second admin */ "456"}; motd[]={"Hello", "Welcome"};
            kickDuplicate=0; verifySignatures=2; allowedFilePatching=1;
            disableVoN=1; vonCodec=1; BattlEye=0; persistent=1;
            voteThreshold=0.6; requiredBuild=123456;
            class AdvancedOptions { LogObjectNotFound=False; SkipDescriptionParsing=True; ignoreMissionLoadErrors=False; };
            class AntiFlood { cycleTime=3; cycleLimit=200; cycleHardLimit=400; enableKick=1; };
            headlessClients[]={"127.0.0.1"}; localClient[]={"127.0.0.1"};
            class Missions { class Coop { template="my_mission.Altis"; difficulty="Custom"; }; };
            customSetting = 42;
            """;
        var cfg = new ServerCfg();
        Assert.That(cfg.ApplyContent(text), Is.GreaterThan(0));
        Assert.Multiple(() =>
        {
            Assert.That(cfg.Hostname, Is.EqualTo("A \"quoted\" server"));
            Assert.That(cfg.MaxPlayers, Is.EqualTo(225));
            Assert.That(cfg.Admins, Is.EqualTo("123\n456"));
            Assert.That(cfg.Motd, Is.EqualTo("Hello\nWelcome"));
            Assert.That(cfg.KickDuplicates, Is.False);
            Assert.That(cfg.VerifySignatures, Is.EqualTo("Activated"));
            Assert.That(cfg.AllowedFilePatching, Is.EqualTo("HC Only"));
            Assert.That(cfg.VonActivated, Is.False);
            Assert.That(cfg.VonCodec, Is.EqualTo("OPUS"));
            Assert.That(cfg.BattlEye, Is.False);
            Assert.That(cfg.Persistent, Is.True);
            Assert.That(cfg.VotingEnabled, Is.True);
            Assert.That(cfg.VoteThreshold, Is.EqualTo(0.6));
            Assert.That(cfg.RequiredBuildChecked, Is.True);
            Assert.That(cfg.RequiredBuild, Is.EqualTo(123456));
            Assert.That(cfg.skipDescriptionParsing, Is.True);
            Assert.That(cfg.AntiFloodEnabled, Is.True);
            Assert.That(cfg.AntiFloodCycleTime, Is.EqualTo(3));
            Assert.That(cfg.AntiFloodEnableKick, Is.True);
            Assert.That(cfg.HeadlessClientEnabled, Is.True);
            Assert.That(cfg.MissionChecked, Is.True);
            Assert.That(cfg.MissionContentOverride, Does.Contain("my_mission.Altis"));
            Assert.That(cfg.ServerCfgContent, Is.EqualTo(text));
        });
        cfg.MaxPlayers = 100;
        cfg.AntiFloodCycleLimit = 250;
        Assert.That(cfg.ServerCfgContent, Does.Contain("maxPlayers = 100;").And.Contain("customSetting = 42;")
            .And.Contain("// maxPlayers = 32;").And.Contain("cycleLimit=250;").And.Contain("my_mission.Altis"));
        var reimport = new ServerCfg();
        reimport.ApplyContent(cfg.ServerCfgContent);
        Assert.That(reimport.Hostname, Is.EqualTo(cfg.Hostname));
        Assert.That(reimport.MaxPlayers, Is.EqualTo(100));
    }

    [TestCase("hostname=\"Changed\"; maxPlayers=oops;")]
    [TestCase("hostname=\"Changed\"; verifySignatures=99;")]
    [TestCase("hostname=\"Changed\"; maxPlayers=99999999999999999;")]
    [TestCase("hostname=\"Changed\"; admins[]={\"one\" \"two\"};")]
    [TestCase("hostname=\"Unclosed;")]
    [TestCase("hostname=\"Changed\"; class AdvancedOptions { skipDescriptionParsing=true;")]
    [TestCase("hostname=\"Changed\"; /* unclosed")]
    [TestCase("hostname=\"Changed\"; maxPlayers=32")]
    [TestCase("hostname=\"Changed\"; voteThreshold=NaN;")]
    public void InvalidCfgDoesNotPartiallyUpdateTheForm(string text)
    {
        var cfg = new ServerCfg { Hostname = "Original" };
        cfg.ServerCfgContent = text;
        Assert.Throws<FormatException>(() => cfg.ApplyContent(text));
        Assert.That(cfg.Hostname, Is.EqualTo("Original"));
        Assert.That(cfg.ServerCfgContent, Is.EqualTo(text));
    }

    [Test]
    public void BasicCfgUsesInvariantNumbersAndConvertsCustomFileSizeFromBytes()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            const string text = "terrainGrid=3.125; viewDistance=4000; MinBandwidth=250000000; MinErrorToSend=0.002; MaxCustomFileSize=160000; class sockets { maxPacketSize=1200; custom=1; }; extra=123;";
            var cfg = new BasicCfg();
            cfg.ApplyContent(text);
            Assert.Multiple(() =>
            {
                Assert.That(cfg.TerrainGrid, Is.EqualTo(3.125));
                Assert.That(cfg.ViewDistance, Is.EqualTo(4000));
                Assert.That(cfg.MinBandwidth, Is.EqualTo(250000000));
                Assert.That(cfg.MinErrorToSend, Is.EqualTo(0.002));
                Assert.That(cfg.MaxCustomFileSize, Is.EqualTo(160));
                Assert.That(cfg.MaxPacketSize, Is.EqualTo(1200));
                Assert.That(cfg.BasicContent, Is.EqualTo(text));
            });
            cfg.MaxPacketSize = 1400;
            cfg.TerrainGrid = 6.25;
            Assert.That(cfg.BasicContent, Does.Contain("terrainGrid=6.25;").And.Contain("maxPacketSize=1400;")
                .And.Contain("custom=1;").And.Contain("extra=123;"));
        }
        finally { CultureInfo.CurrentCulture = originalCulture; }
    }

    [Test]
    public void BasicCfgRejectsUnrepresentableFileSizeWithoutPartialChanges()
    {
        var cfg = new BasicCfg();
        Assert.Throws<FormatException>(() => cfg.ApplyContent("viewDistance=5000; MaxCustomFileSize=1234;"));
        Assert.That(cfg.ViewDistance, Is.EqualTo(2000));
    }

    [Test]
    public void EmptyHeadlessClientListIsNotReplacedByTheDefaultAddress()
    {
        var cfg = new ServerCfg();
        cfg.ApplyContent("headlessClients[]={}; localClient[]={\"127.0.0.1\"};");
        Assert.That(cfg.HeadlessClientEnabled, Is.True);
        Assert.That(cfg.HeadlessClients, Is.Empty);
        Assert.That(cfg.LocalClient, Is.EqualTo("127.0.0.1"));
    }

    [Test]
    public void DuplicateAssignmentsUseLastValueAndStayConsistentAfterFormEdit()
    {
        var cfg = new ServerCfg();
        cfg.ApplyContent("maxPlayers=42; MAXPLAYERS=225; custom[]={1,2};");
        Assert.That(cfg.MaxPlayers, Is.EqualTo(225));
        cfg.MaxPlayers = 100;
        Assert.That(cfg.ServerCfgContent, Does.Contain("maxPlayers=100;").And.Contain("MAXPLAYERS=100;"));
    }

    [Test]
    public void OptionalSettingsCanBeAddedAndRemovedWithoutLosingCustomSettings()
    {
        var cfg = new ServerCfg();
        cfg.ApplyContent("maxPlayers=225; custom=1; class AdvancedOptions { extra=2; };");
        cfg.RequiredBuildChecked = true;
        cfg.RequiredBuild = 123;
        Assert.That(cfg.ServerCfgContent, Does.Contain("requiredBuild = 123;"));
        cfg.RequiredBuildChecked = false;
        cfg.skipDescriptionParsing = true;
        Assert.That(cfg.ServerCfgContent, Does.Not.Contain("requiredBuild ="));
        Assert.That(cfg.ServerCfgContent, Does.Contain("extra=2;").And.Contain("SkipDescriptionParsing = True;").And.Contain("custom=1;"));
    }

    [Test]
    public void ImportedTextAndFormValuesSurviveSettingsSerialization()
    {
        var server = new ServerCfg();
        server.ApplyContent("hostname=\"Imported\"; maxPlayers=225; motd[]={\"Say \"\"hello\"\"\"}; custom=42;");
        var copy = System.Text.Json.JsonSerializer.Deserialize<ServerCfg>(System.Text.Json.JsonSerializer.Serialize(server))!;
        Assert.That(copy.ServerCfgContent, Is.EqualTo(server.ServerCfgContent));
        Assert.That(copy.Motd, Is.EqualTo("Say \"hello\""));
        copy.MaxPlayers = 150;
        copy.Motd = "New \"message\"";
        Assert.That(copy.ServerCfgContent, Does.Contain("maxPlayers=150;").And.Contain("custom=42;"));
        var roundtrip = new ServerCfg();
        roundtrip.ApplyContent(copy.ServerCfgContent);
        Assert.That(roundtrip.Motd, Is.EqualTo(copy.Motd));

        var basic = new BasicCfg();
        basic.ApplyContent("viewDistance=4000; class sockets { maxPacketSize=1200; extra=1; }; custom=42;");
        var basicCopy = System.Text.Json.JsonSerializer.Deserialize<BasicCfg>(System.Text.Json.JsonSerializer.Serialize(basic))!;
        Assert.That(basicCopy.BasicContent, Is.EqualTo(basic.BasicContent));
        basicCopy.MaxPacketSize = 1300;
        Assert.That(basicCopy.BasicContent, Does.Contain("maxPacketSize=1300;").And.Contain("extra=1;").And.Contain("custom=42;"));
    }

    [Test]
    public void ValidPasteCanReplaceABrokenManualMissionBlock()
    {
        var cfg = new ServerCfg { MissionChecked = true, MissionContentOverride = "class Missions {" };
        const string corrected = "maxPlayers=225; class Missions {};";
        cfg.ApplyContent(corrected);
        Assert.That(cfg.MaxPlayers, Is.EqualTo(225));
        Assert.That(cfg.MissionContentOverride, Is.EqualTo("class Missions {};"));
        Assert.That(cfg.ServerCfgContent, Is.EqualTo(corrected));
    }

    [Test]
    public void IncompleteDraftIsPreservedDuringFormEdits()
    {
        var cfg = new ServerCfg { ServerCfgContent = "hostname=\"unfinished" };
        cfg.MaxPlayers = 225;
        Assert.That(cfg.ServerCfgContent, Is.EqualTo("hostname=\"unfinished"));
    }

    [Test]
    public void ProvidedCfgUpdatesServerForm()
    {
        string? path = Environment.GetEnvironmentVariable("FASTER_TEST_CFG");
        if (path == null) { Assert.Ignore("Set FASTER_TEST_CFG to check a private CFG without committing credentials."); return; }
        string text = File.ReadAllText(path);
        var cfg = new ServerCfg();
        cfg.ApplyContent(text);
        Assert.Multiple(() =>
        {
            Assert.That(cfg.Hostname, Is.EqualTo("[CSO] Game Server"));
            Assert.That(cfg.MaxPlayers, Is.EqualTo(225));
            Assert.That(cfg.Admins.Split('\n'), Has.Length.EqualTo(2));
            Assert.That(cfg.VonActivated, Is.False);
            Assert.That(cfg.BattlEye, Is.False);
            Assert.That(cfg.VerifySignatures, Is.EqualTo("Activated"));
            Assert.That(cfg.ForcedDifficulty, Is.EqualTo("Custom"));
            Assert.That(cfg.DisconnectTimeout, Is.EqualTo(5000));
            Assert.That(cfg.VotingEnabled, Is.False);
            Assert.That(cfg.AutoSelectMission, Is.True);
            Assert.That(cfg.ServerCfgContent, Is.EqualTo(text));
        });
        cfg.MaxPlayers = 100;
        var reimport = new ServerCfg();
        reimport.ApplyContent(cfg.ServerCfgContent);
        Assert.That(reimport.MaxPlayers, Is.EqualTo(100));
        Assert.That(reimport.Admins, Is.EqualTo(cfg.Admins));
    }
}
