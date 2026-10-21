using NUnit.Framework;
using System.Threading.Tasks;

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

        [Test()]
        public void ConcurrentAppendsKeepEveryLine()
        {
            var model = new SteamUpdaterModel { Output = string.Empty };

            Parallel.For(0, 500, _ => model.AppendOutput("x"));

            Assert.That(model.Output.Length, Is.EqualTo(500));
        }

        [Test()]
        public void AppendKeepsOnlyTheNewestText()
        {
            var model = new SteamUpdaterModel { Output = new string('a', 199_990) };

            model.AppendOutput(new string('b', 20) + "END");

            Assert.That(model.Output.Length, Is.LessThanOrEqualTo(200_000));
            Assert.That(model.Output, Does.EndWith("END"));
        }
    }
}