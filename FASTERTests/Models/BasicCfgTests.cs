using NUnit.Framework;
using FASTER.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class BasicCfgTests
    {
        BasicCfg cfg;

        [OneTimeSetUp]
        public void BasicCfgSetUp()
        {
            cfg = new BasicCfg();
        }

        [Test()]
        public void BasicCfgGetTest()
        {
            Assert.IsNotNull(cfg.BasicContent);
            Assert.IsNotNull(cfg.MaxBandwidth);
            Assert.IsNotNull(cfg.MaxCustomFileSize);
            Assert.IsNotNull(cfg.MaxMsgSend);
            Assert.IsNotNull(cfg.MaxPacketSize);
            Assert.IsNotNull(cfg.MaxSizeGuaranteed);
            Assert.IsNotNull(cfg.MaxSizeNonGuaranteed);
            Assert.IsNotNull(cfg.MinBandwidth);
            Assert.IsNotNull(cfg.MinErrorToSend);
            Assert.IsNotNull(cfg.MinErrorToSend);
            Assert.IsNotNull(cfg.MinErrorToSendNear);
            Assert.AreEqual("Custom", cfg.PerfPreset);
            Assert.IsNotNull(cfg.TerrainGrid);
            Assert.IsNotNull(cfg.ViewDistance);
        }

        [Test()]
        public void BasicCfgSetTest()
        {
            Assert.DoesNotThrow(() => cfg.PerfPreset = BasicCfgArrays.PerfPresets[1]);
            Assert.DoesNotThrow(() => cfg.PerfPreset = BasicCfgArrays.PerfPresets[2]);
            Assert.DoesNotThrow(() => cfg.PerfPreset = BasicCfgArrays.PerfPresets[3]);
            Assert.DoesNotThrow(() => cfg.PerfPreset = BasicCfgArrays.PerfPresets[4]);
            Assert.DoesNotThrow(() => cfg.ViewDistance = 3);
            Assert.DoesNotThrow(() => cfg.TerrainGrid = BasicCfgArrays.TerrainGrids[0]);
            Assert.DoesNotThrow(() => cfg.MaxBandwidth = ulong.MaxValue);
        }
    }
}