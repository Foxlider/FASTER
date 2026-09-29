using FASTER.Models;
using FASTER.Services;

using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
public class PlatformTests
{
    [Test]
    public void BinaryNamesMatchOs()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.That(Platform.Current.ServerBinaryName, Is.EqualTo("arma3server_x64.exe"));
            Assert.That(Platform.Current.SteamCmdBinaryName, Is.EqualTo("steamcmd.exe"));
            Assert.That(Platform.Current.ServerExecutableExtensionFilter, Is.EqualTo(".exe"));
        }
        else
        {
            Assert.That(Platform.Current.ServerBinaryName, Is.EqualTo("arma3server"));
            Assert.That(Platform.Current.SteamCmdBinaryName, Is.EqualTo("steamcmd.sh"));
            Assert.That(Platform.Current.ServerExecutableExtensionFilter, Is.Empty);
        }
    }

    [Test]
    public void IsServerExecutableAcceptsKnownBinaries()
    {
        Assert.That(Platform.Current.IsServerExecutable("/srv/arma3/arma3server"), Is.True);
        Assert.That(Platform.Current.IsServerExecutable("/srv/arma3/arma3server_x64"), Is.True);
        Assert.That(Platform.Current.IsServerExecutable(null), Is.False);
        Assert.That(Platform.Current.IsServerExecutable(string.Empty), Is.False);
        Assert.That(Platform.Current.IsServerExecutable("/usr/bin/notepad.exe"), Is.False);
    }

    [Test]
    public void IsServerExecutableMatchesLegacyWindowsRule()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Ignore("Windows-only rule check.");
            return;
        }

        Assert.That(Platform.Current.IsServerExecutable(@"D:\Arma\arma3server_x64.exe"), Is.True);
        Assert.That(Platform.Current.IsServerExecutable(@"D:\Arma\arma3server"), Is.False);
    }

    [Test]
    public void OpenFolderRejectsBlankPaths()
    {
        Assert.Throws<ArgumentException>(() => Platform.Current.OpenFolder(" "));
    }
}

[TestFixture]
public class MetricsTests
{
    [Test]
    public void LinuxMetricsReadSaneValues()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Ignore("Linux-only metrics check.");
            return;
        }

        ISystemMetrics metrics = new LinuxSystemMetrics();
        ulong total = metrics.GetTotalMemoryBytes();
        ulong available = metrics.GetAvailableMemoryBytes();

        Assert.That(total, Is.GreaterThan(0));
        Assert.That(available, Is.GreaterThan(0));
        Assert.That(available, Is.LessThanOrEqualTo(total));
        Assert.That(metrics.GetTotalCpuUsage(), Is.InRange(0, 100));
    }
}

[TestFixture]
public class UiBridgeTests
{
    [Test]
    public void NullBridgeNeverThrows()
    {
        Assert.DoesNotThrow(() => Ui.Current.DisplayMessage("hello"));
        Assert.DoesNotThrow(() => Ui.Current.NavigateToConsole());
        Assert.DoesNotThrow(() => Ui.Current.SyncProfileMenuName("id", "name"));
        Assert.DoesNotThrow(() => Ui.Current.ReloadServerProfiles());
        Assert.DoesNotThrow(() => Ui.Current.AppendUpdaterOutput("hello"));
    }

    [Test]
    public async Task NullBridgeUpdaterReportsError()
    {
        Assert.That(Ui.Current.IsUiLoaded(), Is.False);
        Assert.That(await Ui.Current.RunModUpdaterAsync(1, "/tmp"), Is.EqualTo(UpdateState.Error));
        Assert.That(await Ui.Current.GetSteamGuardCodeAsync("prompt"), Is.Empty);
        Assert.That(await Ui.Current.ConfirmPhoneAuthAsync(), Is.False);
    }
}

[TestFixture]
public class ModelTests
{
    [Test]
    public void EncryptionRoundTrips()
    {
        var encrypted = Encryption.Instance.EncryptData("SomeText");
        Assert.That(encrypted, Is.Not.EqualTo("SomeText"));
        Assert.That(Encryption.Instance.DecryptData(encrypted!), Is.EqualTo("SomeText"));
    }

    [Test]
    public void CompareStringIgnoresNoise()
    {
        Assert.That(ModUtilities.GetCompareString("@CBA_A3"), Is.EqualTo(ModUtilities.GetCompareString("CBA_A3")));
    }

    [Test]
    public void SteamIdParsesFromWorkshopUrls()
    {
        Assert.That(SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?id=463939057"), Is.EqualTo(463939057));
        Assert.That(SteamMod.SteamIdFromUrl("https://steamcommunity.com/sharedfiles/filedetails/?l=german&id=463939057"), Is.EqualTo(463939057));
    }

    [Test]
    public void ProfilePresetFilesParseWithoutNetwork()
    {
        var html = """
            <?xml version="1.0" encoding="utf-8"?><html><body><div class="mod-list"><table>
            <tr data-type="ModContainer"><td data-type="DisplayName">CBA_A3</td><td><span class="from-steam">Steam</span></td>
            <td><a href="http://steamcommunity.com/sharedfiles/filedetails/?id=450814997" data-type="Link">link</a></td></tr>
            </table></div></body></html>
            """;
        var path = Path.Combine(Path.GetTempPath(), $"preset-{Guid.NewGuid():N}.html");
        File.WriteAllText(path, html);
        try
        {
            var mods = ModUtilities.ParseModsFromArmaProfileFile(path);
            Assert.That(mods.Count, Is.EqualTo(1));
            Assert.That(mods[0].Name, Is.EqualTo("CBA_A3"));
            Assert.That(mods[0].IsLocal, Is.False);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

[TestFixture]
public class ProfileFilterTests
{
    private static ServerProfile ProfileWithMods()
    {
        var profile = new ServerProfile("filter-test", createFolder: false);
        profile.ProfileMods = new List<ProfileMod>
        {
            new() { Id = 1, Name = "CBA_A3" },
            new() { Id = 2, Name = "ACE3" },
            new() { Id = 3, Name = "CBA_A3_Optional" }
        };
        return profile;
    }

    [Test]
    public void EmptyFilterReturnsEveryMod()
    {
        var profile = ProfileWithMods();
        Assert.That(profile.FilteredProfileMods.Count, Is.EqualTo(3));
    }

    [Test]
    public void TextFilterNarrowsTheList()
    {
        var profile = ProfileWithMods();
        profile.ProfileModsFilter = "ACE";
        Assert.That(profile.FilteredProfileMods.Select(m => m.Name), Is.EqualTo(new[] { "ACE3" }));
    }

    [Test]
    public void BadRegexEmptiesTheListAndRaisesTheFlag()
    {
        var profile = ProfileWithMods();
        profile.ProfileModsFilterIsRegex = true;
        profile.ProfileModsFilter = "([";
        Assert.That(profile.FilteredProfileMods, Is.Empty);
        Assert.That(profile.ProfileModsFilterIsInvalid, Is.True);
    }

    [Test]
    public void RepeatedReadsReturnTheSameListUntilSomethingChanges()
    {
        var profile = ProfileWithMods();
        Assert.That(ReferenceEquals(profile.FilteredProfileMods, profile.FilteredProfileMods), Is.True);
        profile.ProfileModsFilter = "CBA";
        Assert.That(profile.FilteredProfileMods.Count, Is.EqualTo(2));
    }
}
