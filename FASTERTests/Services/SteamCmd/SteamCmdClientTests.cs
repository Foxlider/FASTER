using FASTER.Services.SteamCmd;
using NUnit.Framework;

namespace FASTERTests.Services.SteamCmd;

[TestFixture]
public sealed class SteamCmdClientTests
{
    [TestCase("ERROR! Download item 42 failed (Failure).")]
    [TestCase("Failure")]
    [TestCase("ERROR! Timeout downloading item 42")]
    [TestCase("Steam service unavailable; try again later")]
    [TestCase("Rate Limit Exceeded")]
    [TestCase("No connection to content servers")]
    [TestCase("Temporary network failure")]
    public void IsLikelyTransientWorkshopFailure_AcceptsOnlyRetryableFailures(string error)
    {
        Assert.That(SteamCmdClient.IsLikelyTransientWorkshopFailure(error), Is.True);
    }

    [TestCase("Access denied")]
    [TestCase("Network request failed: Access Denied")]
    [TestCase("No subscription")]
    [TestCase("The user does not own a license")]
    [TestCase("Invalid Password")]
    [TestCase("Steam Guard code required")]
    [TestCase("Disk write failure")]
    [TestCase("Disk failure")]
    [TestCase("Missing file privileges")]
    [TestCase("Permission failure")]
    [TestCase("Invalid configuration")]
    [TestCase("Missing decryption key")]
    [TestCase("No match")]
    [TestCase("Unexpected output")]
    [TestCase("")]
    [TestCase(null)]
    public void IsLikelyTransientWorkshopFailure_RejectsPermanentOrUnknownFailures(string error)
    {
        Assert.That(SteamCmdClient.IsLikelyTransientWorkshopFailure(error), Is.False);
    }
}
