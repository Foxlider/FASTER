using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class BasicCfgTests
    {
        BasicCfg _cfg;

        [OneTimeSetUp]
        public void BasicCfgSetUp()
        {
            _cfg = new BasicCfg();
        }

        [Test()]
        public void BasicCfgGetTest()
        {
            Assert.That(_cfg.BasicContent, Is.Not.Null);
            Assert.That(_cfg.MaxBandwidth, Is.Not.Null);
            Assert.That(_cfg.MaxCustomFileSize, Is.Not.Null);
            Assert.That(_cfg.MaxMsgSend, Is.Not.Null);
            Assert.That(_cfg.MaxPacketSize, Is.Not.Null);
            Assert.That(_cfg.MaxSizeGuaranteed, Is.Not.Null);
            Assert.That(_cfg.MaxSizeNonGuaranteed, Is.Not.Null);
            Assert.That(_cfg.MinBandwidth, Is.Not.Null);
            Assert.That(_cfg.MinErrorToSend, Is.Not.Null);
            Assert.That(_cfg.MinErrorToSend, Is.Not.Null);
            Assert.That(_cfg.MinErrorToSendNear, Is.Not.Null);
            Assert.That(_cfg.PerfPreset, Is.EqualTo("Custom"));
            Assert.That(_cfg.TerrainGrid, Is.Not.Null);
            Assert.That(_cfg.ViewDistance, Is.Not.Null);
        }

        [Test()]
        public void BasicCfgSetTest()
        {
            Assert.DoesNotThrow(() => _cfg.PerfPreset = BasicCfgArrays.PerfPresets[1]);
            Assert.DoesNotThrow(() => _cfg.PerfPreset = BasicCfgArrays.PerfPresets[2]);
            Assert.DoesNotThrow(() => _cfg.PerfPreset = BasicCfgArrays.PerfPresets[3]);
            Assert.DoesNotThrow(() => _cfg.PerfPreset = BasicCfgArrays.PerfPresets[4]);
            Assert.DoesNotThrow(() => _cfg.ViewDistance = 3);
            Assert.DoesNotThrow(() => _cfg.TerrainGrid = BasicCfgArrays.TerrainGrids[0]);
            Assert.DoesNotThrow(() => _cfg.MaxBandwidth = ulong.MaxValue);
        }
    }
}