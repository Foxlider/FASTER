using NUnit.Framework;

using System.Collections.Generic;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class ServerProfileCommandLineTests
    {
        private static ServerProfile NewProfile() => new("Prod", false);

        [Test()]
        public void ClientModsAreOrderedByLoadPriority()
        {
            var p = NewProfile();
            p.ProfileMods = new List<ProfileMod>
            {
                new() { Id = 1, Name = "second", ClientSideChecked = true, LoadPriority = 2 },
                new() { Id = 2, Name = "first",  ClientSideChecked = true, LoadPriority = 1 },
                new() { Id = 3, Name = "off",    ClientSideChecked = false }
            };

            Assert.That(p.CommandLine, Does.Contain("-mod=@first;@second;"));
            Assert.That(p.CommandLine, Does.Not.Contain("@off"));
        }

        [Test()]
        public void ServerOnlyModsUseTheServerModArgument()
        {
            var p = NewProfile();
            p.ProfileMods = new List<ProfileMod> { new() { Id = 1, Name = "srv", ServerSideChecked = true } };

            Assert.That(p.CommandLine, Does.Contain("-serverMod=@srv;"));
            Assert.That(p.CommandLine, Does.Not.Contain("-mod="));
        }

        [Test()]
        public void DlcOrderIsPinned()
        {
            var p = NewProfile();
            p.ContactDLCChecked = true;
            p.GMDLCChecked      = true;
            p.PFDLCChecked      = true;
            p.CSLADLCChecked    = true;
            p.WSDLCChecked      = true;
            p.SPEDLCChecked     = true;
            p.RFDLCChecked      = true;
            p.EFDLCChecked      = true;
            p.ProfileMods = new List<ProfileMod> { new() { Id = 1, Name = "cba", ClientSideChecked = true } };

            Assert.That(p.CommandLine, Does.Contain("-mod=contact;gm;vn;csla;ws;spe;rf;ef;@cba;"));
        }

        [Test()]
        public void FilePatchingFlagOnlyWhenAllowed()
        {
            var p = NewProfile();
            Assert.That(p.CommandLine, Does.Not.Contain("-filePatching"));

            p.ServerCfg.AllowedFilePatching = "HC Only";
            Assert.That(p.CommandLine, Does.Contain("-filePatching"));
        }
    }
}