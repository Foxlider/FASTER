using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ServerProfileTests
    {
        [Test()]
        public void CloneAddsNumberedSuffix()
        {
            var p = new ServerProfile("Prod", false);
            Assert.That(p.Clone().Name, Is.EqualTo("Prod (2)"));
        }

        [Test()]
        public void CloneIncrementsExistingNumber()
        {
            var p = new ServerProfile("Prod (2)", false);
            Assert.That(p.Clone().Name, Is.EqualTo("Prod (3)"));
        }

        [Test()]
        public void CloneDoesNotCrashOnNonNumericBrackets()
        {
            var p = new ServerProfile("Prod (v2)", false);
            Assert.That(p.Clone().Name, Is.EqualTo("Prod (v2) (2)"));
        }

        [Test()]
        public void CloneAvoidsExistingNames()
        {
            var p = new ServerProfile("Prod", false);
            Assert.That(p.Clone(new[] { "Prod", "Prod (2)" }).Name, Is.EqualTo("Prod (3)"));
        }

        [Test()]
        public void CloneGetsNewId()
        {
            var p = new ServerProfile("Prod", false);
            Assert.That(p.Clone().Id, Is.Not.EqualTo(p.Id));
        }
    }
}