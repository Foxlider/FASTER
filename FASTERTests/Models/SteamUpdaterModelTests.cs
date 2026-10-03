using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class SteamUpdaterModelTests
    {
        [Test()]
        public void OutputKeepsOnlyTheNewestText()
        {
            var model = new SteamUpdaterModel();

            model.Output = new string('a', 300_000) + "END";

            Assert.That(model.Output.Length, Is.LessThanOrEqualTo(200_000));
            Assert.That(model.Output, Does.EndWith("END"));
        }
    }
}