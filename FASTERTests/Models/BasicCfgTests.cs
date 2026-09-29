using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class BasicCfgTests
    {
        BasicCfg _cfg;

        [OneTimeSetUp]
        public void SetUp()
        { _cfg = new BasicCfg(); }

        #region Defaults match Arma's documented defaults (regression: swapped Guaranteed/NonGuaranteed)

        [Test()]
        public void MaxSizeGuaranteedDefaultsTo512()
        {
            Assert.That(_cfg.MaxSizeGuaranteed, Is.EqualTo(512));
        }

        [Test()]
        public void MaxSizeNonGuaranteedDefaultsTo256()
        {
            Assert.That(_cfg.MaxSizeNonGuaranteed, Is.EqualTo(256));
        }

        [Test()]
        public void MaxMsgSendDefaultsTo128()
        {
            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(128));
        }

        [Test()]
        public void MinBandwidthDefaultsTo131072()
        {
            Assert.That(_cfg.MinBandwidth, Is.EqualTo(131072));
        }

        [Test()]
        public void MinErrorToSendDefaultsToPoint001()
        {
            Assert.That(_cfg.MinErrorToSend, Is.EqualTo(0.001));
        }

        [Test()]
        public void MinErrorToSendNearDefaultsToPoint01()
        {
            Assert.That(_cfg.MinErrorToSendNear, Is.EqualTo(0.01));
        }

        [Test()]
        public void MaxCustomFileSizeDefaultsTo1024()
        {
            Assert.That(_cfg.MaxCustomFileSize, Is.EqualTo(1024));
        }

        [Test()]
        public void MaxPacketSizeDefaultsTo1400()
        {
            Assert.That(_cfg.MaxPacketSize, Is.EqualTo(1400));
        }

        [Test()]
        public void ViewDistanceDefaultsTo2000()
        {
            Assert.That(_cfg.ViewDistance, Is.EqualTo(2000));
        }

        [Test()]
        public void TerrainGridDefaultsTo25()
        {
            Assert.That(_cfg.TerrainGrid, Is.EqualTo(25));
        }

        [Test()]
        public void LanguageDefaultsToEnglish()
        {
            Assert.That(_cfg.Language, Is.EqualTo("English"));
        }

        #endregion

        #region Properties round-trip correctly (set then read back)

        [Test()]
        public void MaxBandwidthRoundTrips()
        {
            _cfg.MaxBandwidth = ulong.MaxValue;
            Assert.That(_cfg.MaxBandwidth, Is.EqualTo(ulong.MaxValue));
        }

        [Test()]
        public void ViewDistanceRoundTrips()
        {
            _cfg.ViewDistance = 3;
            Assert.That(_cfg.ViewDistance, Is.EqualTo(3));
        }

        [Test()]
        public void TerrainGridRoundTrips()
        {
            _cfg.TerrainGrid = BasicCfgArrays.TerrainGrids[0];
            Assert.That(_cfg.TerrainGrid, Is.EqualTo(BasicCfgArrays.TerrainGrids[0]));
        }

        [Test()]
        public void LanguageRoundTrips()
        {
            foreach (var language in BasicCfgArrays.Languages)
            {
                _cfg.Language = language;
                Assert.That(_cfg.Language, Is.EqualTo(language));
            }
        }

        #endregion

        #region PerfPreset (regression: "Custom" used to reset values to a baseline)

        [Test()]
        public void PerfPresetGetterAlwaysReturnsCustom()
        {
            Assert.That(_cfg.PerfPreset, Is.EqualTo("Custom"));
        }

        [Test()]
        public void SelectingCustomDoesNotResetManuallySetValues()
        {
            _cfg.MaxMsgSend = 999;
            _cfg.MinBandwidth = 42;

            _cfg.PerfPreset = "Custom";

            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(999));
            Assert.That(_cfg.MinBandwidth, Is.EqualTo((ulong)42));
        }

        [Test()]
        public void SelectingUnknownPresetDoesNotResetValues()
        {
            _cfg.MaxMsgSend = 999;

            _cfg.PerfPreset = "Not A Real Preset";

            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(999));
        }

        [Test()]
        public void Arma3DefaultsPresetSetsExpectedValues()
        {
            _cfg.PerfPreset = "Arma3 Defaults";

            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(128));
            Assert.That(_cfg.MinBandwidth, Is.EqualTo((ulong)131072));
            Assert.That(_cfg.MaxSizeGuaranteed, Is.EqualTo(512));
            Assert.That(_cfg.MaxSizeNonGuaranteed, Is.EqualTo(256));
        }

        [Test()]
        public void OneGbPresetSetsExpectedValues()
        {
            _cfg.PerfPreset = "1Gb Preset";

            Assert.That(_cfg.MaxMsgSend, Is.EqualTo(512));
            Assert.That(_cfg.MinBandwidth, Is.EqualTo((ulong)1000000000));
        }

        [Test()]
        public void EachNamedPresetProducesADifferentMinBandwidth()
        {
            var seen = new System.Collections.Generic.HashSet<ulong>();

            foreach (var preset in BasicCfgArrays.PerfPresets)
            {
                if (preset == "Custom") continue;

                _cfg.PerfPreset = preset;
                seen.Add(_cfg.MinBandwidth);
            }

            Assert.That(seen.Count, Is.EqualTo(BasicCfgArrays.PerfPresets.Length - 1),
                "Two different named presets produced the same MinBandwidth — likely a copy-paste error in the switch.");
        }

        #endregion

        #region ProcessFile output sanity

        [Test()]
        public void ProcessFileNeverReturnsNullOrEmpty()
        {
            Assert.That(_cfg.ProcessFile(), Is.Not.Null.And.Not.Empty);
        }

        [Test()]
        public void ProcessFileIncludesLanguageAndViewDistance()
        {
            _cfg.Language = "French";
            _cfg.ViewDistance = 1600;

            var output = _cfg.ProcessFile();

            Assert.That(output, Does.Contain("language=\"French\";"));
            Assert.That(output, Does.Contain("viewDistance = 1600;"));
        }

        [Test()]
        public void ProcessFileWritesMaxCustomFileSizeInBytes()
        {
            _cfg.MaxCustomFileSize = 160;

            // MaxCustomFileSize is stored in KB but written to the file in bytes (x1000).
            Assert.That(_cfg.ProcessFile(), Does.Contain("MaxCustomFileSize = 160000;"));
        }

        [Test()]
        public void ChangingAPropertyRegeneratesBasicContent()
        {
            var before = _cfg.BasicContent;

            _cfg.ViewDistance = 5000;

            Assert.That(_cfg.BasicContent, Is.Not.EqualTo(before));
            Assert.That(_cfg.BasicContent, Does.Contain("viewDistance = 5000;"));
        }

        #endregion
    }
}