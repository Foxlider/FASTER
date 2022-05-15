using System;
using NUnit.Framework;
using System.IO;
using System.Reflection;
// ReSharper disable CheckNamespace

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class SteamModTests
    {
        [Test()]
        public void SteamIdFromUrlTest()
        {
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?id=463939057"));
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?id=463939057&l=french"));
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/sharedfiles/filedetails/?l=german&id=463939057"));
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?l=french&id=463939057"));
            Assert.AreEqual(463939057, SteamMod.SteamIdFromUrl("https://steamcommunity.com/workshop/filedetails/?l=english&id=463939057&l=french"));
        }


        [Test()]
        public void GetModInfoTest()
        {
            Tuple<string, string, int> expected = new Tuple<string, string, int>("ace", "acemod", 1577907553);
            Tuple<string, string, int> res = null;
            Assert.DoesNotThrow(() => { res = SteamMod.GetModInfo(463939057); });
            Assert.AreEqual(expected.Item1, res.Item1, "The expected mod name was wrong");
            Assert.AreEqual(expected.Item2, res.Item2, "The expected creator name was wrong");
            Assert.GreaterOrEqual(res.Item3, expected.Item3, "The expected update time was wrong");
        }
    }

    [TestFixture()]
    public class ArmaProfileFileParsingTests
    {
        readonly string _armaProfileName = "FASTER_Testing.html";
        readonly string _armaProfileContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?><html> <head> <meta name=\"arma:Type\" content=\"preset\"/> <meta name=\"arma:PresetName\" content=\"Arma 3 Vanilla plus CBA\"/> <meta name=\"generator\" content=\"Arma 3 Launcher - https://arma3.com\"/> <title>Arma 3</title> <link href=\"https://fonts.googleapis.com/css?family=Roboto\" rel=\"stylesheet\" type=\"text/css\"/> <style>body{margin: 0;padding: 0;color: #fff;background: #000;}body, th, td{font: 95%/1.3 Roboto, Segoe UI, Tahoma, Arial, Helvetica, sans-serif;}td{padding: 3px 30px 3px 0;}h1{padding: 20px 20px 0 20px; color: white; font-weight: 200; font-family: segoe ui; font-size: 3em; margin: 0;}em{font-variant: italic; color:silver;}.before-list{padding: 5px 20px 10px 20px;}.mod-list{background: #222222; padding: 20px;}.dlc-list{background: #222222; padding: 20px;}.footer{padding: 20px; color:gray;}.whups{color:gray;}a{color: #D18F21; text-decoration: underline;}a:hover{color:#F1AF41; text-decoration: none;}.from-steam{color: #449EBD;}.from-local{color: gray;}</style> </head> <body> <h1>Arma 3 - Preset <strong>Arma 3 Vanilla plus CBA</strong></h1> <p class=\"before-list\"> <em>Drag this file or link to it to Arma 3 Launcher or open it Mods / Preset / Import.</em> </p><div class=\"mod-list\"> <table> <tr data-type=\"ModContainer\"> <td data-type=\"DisplayName\">CBA_A3</td><td> <span class=\"from-steam\">Steam</span> </td><td> <a href=\"http://steamcommunity.com/sharedfiles/filedetails/?id=450814997\" data-type=\"Link\">http://steamcommunity.com/sharedfiles/filedetails/?id=450814997</a> </td></tr></table> </div><div class=\"dlc-list\"> <table> <tr data-type=\"DlcContainer\"> <td data-type=\"DisplayName\">Global Mobilization</td><td> <a href=\"https://store.steampowered.com/app/1042220\" data-type=\"Link\">https://store.steampowered.com/app/1042220</a> </td></tr><tr data-type=\"DlcContainer\"> <td data-type=\"DisplayName\">S.O.G. Prairie Fire</td><td> <a href=\"https://store.steampowered.com/app/1227700\" data-type=\"Link\">https://store.steampowered.com/app/1227700</a> </td></tr><tr data-type=\"DlcContainer\"> <td data-type=\"DisplayName\">CSLA Iron Curtain</td><td> <a href=\"https://store.steampowered.com/app/1294440\" data-type=\"Link\">https://store.steampowered.com/app/1294440</a> </td></tr><tr data-type=\"DlcContainer\"> <td data-type=\"DisplayName\">Western Sahara</td><td> <a href=\"https://store.steampowered.com/app/1681170\" data-type=\"Link\">https://store.steampowered.com/app/1681170</a> </td></tr></table> </div><div class=\"footer\"> <span>Created by Arma 3 Launcher by Bohemia Interactive.</span> </div></body></html>";

        [Test()]
        public void ParseArmaProfileFile()
        {
            var fullPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _armaProfileName);
            if (!File.Exists(fullPath))
                File.WriteAllText(fullPath, _armaProfileContent);

            var modList = ModUtilities.ParseModsFromArmaProfileFile(fullPath);
            Assert.IsNotNull(modList);
            Assert.AreEqual(1, modList.Count);
            Assert.AreEqual("CBA_A3", modList[0].Name);
            Assert.AreEqual(ModUtilities.GetCompareString("@CBA_A3"), ModUtilities.GetCompareString(modList[0].Name));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}