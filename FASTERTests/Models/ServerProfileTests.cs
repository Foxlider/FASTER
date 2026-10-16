using NUnit.Framework;
using System.Collections.Generic;

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

        [Test()]
        public void CloneKeepsSettingsAndMods()
        {
            var p = new ServerProfile("Prod", false) { Port = 2402, LimitFPS = 120 };
            p.ServerCfg.Hostname    = "My Server";
            p.BasicCfg.ViewDistance = 3500;
            p.ArmaProfile.SkillAi   = 0.8;
            p.ProfileMods = new List<ProfileMod>
            {
                new() { Id = 450814997, Name = "CBA_A3", ClientSideChecked = true, LoadPriority = 5 }
            };

            var clone = p.Clone();

            Assert.That(clone.Port,                      Is.EqualTo(2402));
            Assert.That(clone.LimitFPS,                  Is.EqualTo(120));
            Assert.That(clone.ServerCfg.Hostname,        Is.EqualTo("My Server"));
            Assert.That(clone.BasicCfg.ViewDistance,     Is.EqualTo(3500));
            Assert.That(clone.ArmaProfile.SkillAi,       Is.EqualTo(0.8));
            Assert.That(clone.ProfileMods,               Has.Count.EqualTo(1));
            Assert.That(clone.ProfileMods[0].Name,       Is.EqualTo("CBA_A3"));
            Assert.That(clone.ProfileMods[0].LoadPriority, Is.EqualTo((ushort?) 5));
        }
    }
}