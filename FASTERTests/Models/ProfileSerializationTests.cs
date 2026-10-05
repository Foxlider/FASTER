using NUnit.Framework;

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ProfileSerializationTests
    {
        private static T RoundTrip<T>(T value)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var writer = new StringWriter();
            serializer.Serialize(writer, value);
            using var reader = new StringReader(writer.ToString());
            return (T) serializer.Deserialize(reader)!;
        }

        [Test()]
        public void ProfileSurvivesSaveAndLoad()
        {
            var original = new ServerProfile("Prod", false)
            {
                Port                = 2402,
                HugePages           = true,
                LimitFPS            = 120,
                ExThreads           = 3,
                EnableSteamLogs     = true,
                LoadMissionToMemory = true,
                BePath              = @"C:\be",
                KeysFolder          = @"C:\keys",
                SPEDLCChecked       = true,
                RFDLCChecked        = true,
                EFDLCChecked        = true,
                ProfileMods         = new List<ProfileMod>
                {
                    new() { Id = 450814997, Name = "CBA_A3", ClientSideChecked = true, LoadPriority = 5 }
                }
            };
            original.ServerCfg.Hostname            = "My Server";
            original.ServerCfg.AntiFloodEnabled    = true;
            original.ServerCfg.AntiFloodCycleTime  = 7;
            original.BasicCfg.ViewDistance         = 3500;
            original.ArmaProfile.SkillAi           = 0.8;

            var loaded = RoundTrip(original);

            Assert.That(loaded.Id,                          Is.EqualTo(original.Id));
            Assert.That(loaded.Name,                        Is.EqualTo("Prod"));
            Assert.That(loaded.Port,                        Is.EqualTo(2402));
            Assert.That(loaded.HugePages,                   Is.True);
            Assert.That(loaded.LimitFPS,                    Is.EqualTo(120));
            Assert.That(loaded.ExThreads,                   Is.EqualTo(3));
            Assert.That(loaded.EnableSteamLogs,             Is.True);
            Assert.That(loaded.LoadMissionToMemory,         Is.True);
            Assert.That(loaded.BePath,                      Is.EqualTo(@"C:\be"));
            Assert.That(loaded.KeysFolder,                  Is.EqualTo(@"C:\keys"));
            Assert.That(loaded.SPEDLCChecked,               Is.True);
            Assert.That(loaded.RFDLCChecked,                Is.True);
            Assert.That(loaded.EFDLCChecked,                Is.True);
            Assert.That(loaded.ServerCfg.Hostname,          Is.EqualTo("My Server"));
            Assert.That(loaded.ServerCfg.AntiFloodEnabled,  Is.True);
            Assert.That(loaded.ServerCfg.AntiFloodCycleTime, Is.EqualTo(7));
            Assert.That(loaded.BasicCfg.ViewDistance,       Is.EqualTo(3500));
            Assert.That(loaded.ArmaProfile.SkillAi,         Is.EqualTo(0.8));
            Assert.That(loaded.ProfileMods, Has.Count.EqualTo(1));
            Assert.That(loaded.ProfileMods[0].Name,         Is.EqualTo("CBA_A3"));
            Assert.That(loaded.ProfileMods[0].LoadPriority, Is.EqualTo((ushort?) 5));
        }

        [Test()]
        public void ProfileFromAnOlderVersionLoadsWithDefaults()
        {
            // An old profile that has none of the fields added in later versions
            const string oldXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                                + "<ServerProfile><Id>_abc</Id><Name>Old profile</Name><Port>2310</Port></ServerProfile>";

            var serializer = new XmlSerializer(typeof(ServerProfile));
            using var reader = new StringReader(oldXml);
            var loaded = (ServerProfile) serializer.Deserialize(reader)!;

            Assert.That(loaded.Id,   Is.EqualTo("_abc"));
            Assert.That(loaded.Name, Is.EqualTo("Old profile"));
            Assert.That(loaded.Port, Is.EqualTo(2310));

            // Newer fields fall back to their defaults
            Assert.That(loaded.LimitFPS,                   Is.EqualTo(0));
            Assert.That(loaded.EFDLCChecked,               Is.False);
            Assert.That(loaded.ServerCfg.AntiFloodEnabled, Is.False);
            Assert.That(loaded.ServerCfg.MaxPlayers,       Is.EqualTo(32));
        }
    }
}