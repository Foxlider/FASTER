using NUnit.Framework;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ServerCfgTests
    {
        ServerCfg _cfg;

        [SetUp]
        public void SetUp()
        { _cfg = new ServerCfg(); }

        #region requiredBuild (regression: missing semicolon)

        [Test()]
        public void RequiredBuildIsWrittenWithSemicolon()
        {
            _cfg.RequiredBuildChecked = true;
            _cfg.RequiredBuild = 123456789;

            Assert.That(_cfg.ProcessFile(), Does.Contain("requiredBuild = 123456789;"));
        }

        [Test()]
        public void RequiredBuildLineOmittedWhenUnchecked()
        {
            _cfg.RequiredBuildChecked = false;

            Assert.That(_cfg.ProcessFile(), Does.Not.Contain("requiredBuild ="));
        }

        #endregion

        #region onUserKicked (regression: box existed but was never written)

        [Test()]
        public void OnUserKickedIsWrittenToOutput()
        {
            _cfg.OnUserKicked = "kick (_this select 0)";

            Assert.That(_cfg.ProcessFile(), Does.Contain("onUserKicked = \"kick (_this select 0)\";"));
        }

        [Test()]
        public void OnUserKickedDefaultsToEmptyString()
        {
            Assert.That(_cfg.ProcessFile(), Does.Contain("onUserKicked = \"\";"));
        }

        #endregion

        #region LocalClient (regression: missing \r strip)

        [Test()]
        public void LocalClientStripsCarriageReturns()
        {
            _cfg.LocalClient = "127.0.0.1\r\n192.168.1.1\r\n10.0.0.1";

            Assert.That(_cfg.LocalClient, Does.Not.Contain("\r"));
            Assert.That(_cfg.LocalClient, Is.EqualTo("127.0.0.1\n192.168.1.1\n10.0.0.1"));
        }

        [Test()]
        public void LocalClientRoundTripsSingleEntry()
        {
            _cfg.LocalClient = "127.0.0.1";
            Assert.That(_cfg.LocalClient, Is.EqualTo("127.0.0.1"));
        }

        #endregion

        #region HeadlessClients / Admins (already correct — guard against regressing to match LocalClient's old bug)

        [Test()]
        public void HeadlessClientsStripsCarriageReturns()
        {
            _cfg.HeadlessClients = "127.0.0.1\r\n192.168.1.1";
            Assert.That(_cfg.HeadlessClients, Does.Not.Contain("\r"));
        }

        [Test()]
        public void AdminsStripsCarriageReturns()
        {
            _cfg.Admins = "76561198000000000\r\n76561198000000001";
            Assert.That(_cfg.Admins, Does.Not.Contain("\r"));
        }

        [Test()]
        public void EnablingHeadlessClientsDefaultsToLocalhost()
        {
            Assert.That(_cfg.HeadlessClientEnabled, Is.False);

            _cfg.HeadlessClientEnabled = true;

            Assert.That(_cfg.HeadlessClients, Is.EqualTo("127.0.0.1"));
        }

        [Test()]
        public void EnablingHeadlessClientsDoesNotOverwriteExistingList()
        {
            _cfg.HeadlessClients = "192.168.1.50";
            _cfg.HeadlessClientEnabled = true;

            Assert.That(_cfg.HeadlessClients, Is.EqualTo("192.168.1.50"));
        }

        #endregion

        #region Array-backed enum-style properties round-trip correctly

        [Test()]
        public void AllowedFilePatchingRoundTrips()
        {
            foreach (var value in ServerCfgArrays.AllowFilePatchingStrings)
            {
                _cfg.AllowedFilePatching = value;
                Assert.That(_cfg.AllowedFilePatching, Is.EqualTo(value));
            }
        }

        [Test()]
        public void VerifySignaturesRoundTrips()
        {
            foreach (var value in ServerCfgArrays.VerifySignaturesStrings)
            {
                _cfg.VerifySignatures = value;
                Assert.That(_cfg.VerifySignatures, Is.EqualTo(value));
            }
        }

        [Test()]
        public void VonCodecRoundTrips()
        {
            foreach (var value in ServerCfgArrays.VonCodecStrings)
            {
                _cfg.VonCodec = value;
                Assert.That(_cfg.VonCodec, Is.EqualTo(value));
            }
        }

        [Test()]
        public void TimeStampFormatDefaultsToShort()
        {
            Assert.That(_cfg.TimeStampFormat, Is.EqualTo("short"));
        }

        #endregion

        #region Boolean-backed properties

        [Test()]
        public void KickDuplicatesDefaultsToTrue()
        {
            Assert.That(_cfg.KickDuplicates, Is.True);
        }

        [Test()]
        public void KickDuplicatesRoundTrips()
        {
            _cfg.KickDuplicates = false;
            Assert.That(_cfg.KickDuplicates, Is.False);
            _cfg.KickDuplicates = true;
            Assert.That(_cfg.KickDuplicates, Is.True);
        }

        [Test()]
        public void BattlEyeDefaultsToTrue()
        {
            Assert.That(_cfg.BattlEye, Is.True);
        }

        [Test()]
        public void VonActivatedDefaultsToTrue()
        {
            Assert.That(_cfg.VonActivated, Is.True);
        }

        [Test()]
        public void DisablingPersistentAlsoDisablesAutoInit()
        {
            _cfg.Persistent = true;
            _cfg.AutoInit = true;
            Assert.That(_cfg.AutoInit, Is.True);

            _cfg.Persistent = false;

            Assert.That(_cfg.AutoInit, Is.False, "Turning off Persistent should force AutoInit off too.");
        }

        #endregion

        #region ProcessFile output sanity

        [Test()]
        public void ProcessFileNeverReturnsNullOrEmpty()
        {
            var output = _cfg.ProcessFile();
            Assert.That(output, Is.Not.Null.And.Not.Empty);
        }

        [Test()]
        public void ProcessFileIncludesHostnameAndPassword()
        {
            _cfg.Hostname = "My Test Server";
            _cfg.Password = "hunter2";

            var output = _cfg.ProcessFile();

            Assert.That(output, Does.Contain("hostname = \"My Test Server\";"));
            Assert.That(output, Does.Contain("password = \"hunter2\";"));
        }

        [Test()]
        public void ProcessFileOmitsHeadlessClientsBlockWhenDisabled()
        {
            _cfg.HeadlessClientEnabled = false;

            Assert.That(_cfg.ProcessFile(), Does.Not.Contain("headlessClients[]"));
        }

        [Test()]
        public void ProcessFileIncludesHeadlessClientsBlockWhenEnabled()
        {
            _cfg.HeadlessClientEnabled = true;

            Assert.That(_cfg.ProcessFile(), Does.Contain("headlessClients[]"));
        }

        [Test()]
        public void ChangingAPropertyRegeneratesServerCfgContent()
        {
            var before = _cfg.ServerCfgContent;

            _cfg.Hostname = "Changed";

            Assert.That(_cfg.ServerCfgContent, Is.Not.EqualTo(before));
            Assert.That(_cfg.ServerCfgContent, Does.Contain("hostname = \"Changed\";"));
        }

        #endregion
    }
}