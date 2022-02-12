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
            Assert.IsNotNull(_cfg.BasicContent);
            Assert.IsNotNull(_cfg.MaxBandwidth);
            Assert.IsNotNull(_cfg.MaxCustomFileSize);
            Assert.IsNotNull(_cfg.MaxMsgSend);
            Assert.IsNotNull(_cfg.MaxPacketSize);
            Assert.IsNotNull(_cfg.MaxSizeGuaranteed);
            Assert.IsNotNull(_cfg.MaxSizeNonGuaranteed);
            Assert.IsNotNull(_cfg.MinBandwidth);
            Assert.IsNotNull(_cfg.MinErrorToSend);
            Assert.IsNotNull(_cfg.MinErrorToSend);
            Assert.IsNotNull(_cfg.MinErrorToSendNear);
            Assert.AreEqual("Custom", _cfg.PerfPreset);
            Assert.IsNotNull(_cfg.TerrainGrid);
            Assert.IsNotNull(_cfg.ViewDistance);
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