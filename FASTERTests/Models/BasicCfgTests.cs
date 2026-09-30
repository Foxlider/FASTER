using NUnit.Framework;

using System.Globalization;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class BasicCfgTests
    {
        BasicCfg _cfg;

        [SetUp]
        public void BasicCfgSetUp()
        { _cfg = new BasicCfg(); }

        [Test()]
        public void BasicCfgDefaults()
        {
            Assert.That(_cfg.BasicContent, Is.Not.Empty);
            Assert.That(_cfg.PerfPreset, Is.EqualTo("Custom"));
            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(128));
            Assert.That(_cfg.MaxSizeGuaranteed, Is.EqualTo(512));
            Assert.That(_cfg.MaxSizeNonGuaranteed, Is.EqualTo(256));
            Assert.That(_cfg.MinBandwidth, Is.EqualTo(131072));
            Assert.That(_cfg.MaxBandwidth, Is.EqualTo(int.MaxValue));
            Assert.That(_cfg.MinErrorToSend, Is.EqualTo(0.001));
            Assert.That(_cfg.MinErrorToSendNear, Is.EqualTo(0.01));
            Assert.That(_cfg.MaxCustomFileSize, Is.EqualTo(1024));
            Assert.That(_cfg.MaxPacketSize, Is.EqualTo(1400));
            Assert.That(_cfg.TerrainGrid, Is.EqualTo(25));
            Assert.That(_cfg.ViewDistance, Is.EqualTo(2000));
            Assert.That(_cfg.Language, Is.EqualTo("English"));
        }

        [Test()]
        public void BasicCfgSetTest()
        {
            Assert.DoesNotThrow(() => _cfg.ViewDistance = 3);
            Assert.That(_cfg.ViewDistance, Is.EqualTo(3));

            Assert.DoesNotThrow(() => _cfg.TerrainGrid = BasicCfgArrays.TerrainGrids[0]);
            Assert.That(_cfg.TerrainGrid, Is.EqualTo(BasicCfgArrays.TerrainGrids[0]));

            Assert.DoesNotThrow(() => _cfg.MaxBandwidth = ulong.MaxValue);
            Assert.That(_cfg.MaxBandwidth, Is.EqualTo(ulong.MaxValue));
        }

        [Test()]
        public void PerfPresetsApplyExpectedValues()
        {
            _cfg.PerfPreset = "Arma3 Defaults";
            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(128));
            Assert.That(_cfg.MinBandwidth, Is.EqualTo(131072));

            _cfg.PerfPreset = "1Mb Preset";
            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(256));
            Assert.That(_cfg.MinBandwidth, Is.EqualTo(1000000));

            _cfg.PerfPreset = "250Mb Preset";
            Assert.That(_cfg.MinBandwidth, Is.EqualTo(250000000));

            _cfg.PerfPreset = "1Gb Preset";
            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(512));
            Assert.That(_cfg.MinBandwidth, Is.EqualTo(1000000000));
        }

        [Test()]
        public void CustomPresetLeavesValuesAlone()
        {
            _cfg.MinBandwidth = 42;
            _cfg.PerfPreset = "Custom";
            Assert.That(_cfg.MinBandwidth, Is.EqualTo(42));
        }

        [Test()]
        public void TerrainGridIgnoresCurrentCulture()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                _cfg.TerrainGrid = 12.5;
                Assert.That(_cfg.ProcessFile(), Does.Contain("terrainGrid=12.5;"));
            }
            finally
            { CultureInfo.CurrentCulture = previous; }
        }
    }
}