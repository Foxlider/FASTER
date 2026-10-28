using FASTER.ViewModel;
using NUnit.Framework;

namespace FASTER.Core.Tests;

[TestFixture]
public class UpdaterLifetimeTests
{
    [Test]
    public void DisposedUpdaterRejectsNewWorkAndToleratesRepeatedCleanup()
    {
        var updater = new SteamUpdaterViewModel();
        updater.UpdateCancelClick();
        updater.Dispose();

        Assert.DoesNotThrow(updater.Dispose);
        Assert.DoesNotThrow(updater.UpdateCancelClick);
        Assert.ThrowsAsync<ObjectDisposedException>(() => updater.SteamLogin());
        Assert.ThrowsAsync<ObjectDisposedException>(() => updater.RunModUpdater(0, "unused"));
    }
}
