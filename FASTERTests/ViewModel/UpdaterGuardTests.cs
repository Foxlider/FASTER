using FASTER.Models;

using NUnit.Framework;

using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FASTER.ViewModel.Tests
{
    [TestFixture()]
    public class UpdaterGuardTests
    {
        [Test()]
        public void NothingRunningByDefault()
        {
            Assert.That(new SteamUpdaterViewModel().UpdateInProgress, Is.False);
        }

        [Test()]
        public void ServerUpdateCountsAsInProgress()
        {
            var vm = new SteamUpdaterViewModel();
            vm.Parameters.IsUpdating = true;

            Assert.That(vm.UpdateInProgress, Is.True);
        }

        [Test()]
        public async Task ModUpdateIsRefusedWhileAnotherRuns()
        {
            var vm = new SteamUpdaterViewModel();
            vm.Parameters.IsUpdating = true;

            var result = await vm.RunModsUpdater(new ObservableCollection<ArmaMod>());

            Assert.That(result, Is.EqualTo(UpdateState.Busy));
            Assert.That(vm.Parameters.Output, Does.Contain("already running"));
        }
    }
}