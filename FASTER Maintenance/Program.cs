using FASTER.Models;
using FASTER_Maintenance.Models._16Models;
using FASTER_Maintenance.Models._17Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using ServerProfile = FASTER.Models.ServerProfile;

namespace FASTER_Maintenance
{
    static class Program
    {
        private static readonly string Version = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}." +
                                                 $"{Assembly.GetExecutingAssembly().GetName().Version.Minor}." +
                                                 $"{Assembly.GetExecutingAssembly().GetName().Version.Build} " +
                                                 $"({Assembly.GetExecutingAssembly().GetName().Version.Revision})";

        private static int _exitCode = -1;

        [STAThread]
        static void Main()
        {
            do
            {
                ClearConsole();
                ShowMainMenu();
            } while (_exitCode == -1);

            Environment.ExitCode = _exitCode;
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static void ShowMainMenu()
        {
            Console.WriteLine("\n\n" +
                              " [ MAIN MENU ]\n" +
                              "1. FASTER settings backup\n" +
                              "2. SteamCMD Cleanup\n" +
                              "3. Set Install Environment Variable\n" +
                              "4.\n" +
                              "5. Migration to FASTER 1.7\n" +
                              "\n0. Quit\n");
            ConsoleKey rk = Console.ReadKey(true).Key;

            switch (rk)
            {
                case ConsoleKey.D0:
                case ConsoleKey.NumPad0:
                    _exitCode = 0;
                    return;
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    BackupSettings();
                    return;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    SteamCmdCleanup();
                    return;
                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    SetEnvVar();
                    return;
                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    MigrateTo17();
                    return;
                default: 
                    return;
            }
        }

        private static void SteamCmdCleanup()
        {
            Console.WriteLine("\n\nUNHANDLED YET ! PLEASE CONTACT THE DEV FOR AN UPDATE.");
            _exitCode = 1;
        }

        private static void SetEnvVar()
        {
            using var fbd = new FolderBrowserDialog
            {
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                Description = "Select FASTER's new install folder"
            };

            if (fbd.ShowDialog() != DialogResult.OK)
            { return; }

            ClearConsole();
            Console.WriteLine("\n\n");
            var folder = fbd.SelectedPath;
            if (!Directory.Exists(folder))
            {
                Console.WriteLine("The selected directory does not exist on the disk.");
                _exitCode = 102;
                return;
            }

            try
            { 
                Environment.SetEnvironmentVariable("DOTNET_BUNDLE_EXTRACT_BASE_DIR", folder, EnvironmentVariableTarget.Machine); 
                Console.WriteLine($"Set Environsment Variable DOTNET_BUNDLE_EXTRACT_BASE_DIR to {folder} successfully.");
                _exitCode = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not set environment variables : {e.Message}");
                _exitCode = 205;
            }
        }

        private static void MigrateTo17()
        {
            var sourcePath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData") ?? throw new InvalidOperationException(), "FoxliCorp", "FASTER_StrongName_r3kmcr0zqf35dnhwrlga5cvn2azjfziz");
            var versions = Directory.GetDirectories(sourcePath).ToList();
            Console.WriteLine("Select your version to update from");
            foreach (var path in versions)
            {
                var version = path.Replace(sourcePath, "").Replace("\\", "");
                if(version.StartsWith("1.7"))
                    continue;
                Console.WriteLine($"{versions.IndexOf(path)} : v{version} ({Directory.GetLastWriteTime(path):dd:MM::yyyy}");
            }
            var key = Console.ReadKey(true).KeyChar;
            switch (key)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    var selected = versions[int.Parse(key.ToString())];
                    if (selected == null || selected.Contains("1.7.")) return;

                    Console.WriteLine("\n\nDo you want to backup your data ? (Y/n)");
                    var backupKey = Console.ReadKey(true).Key;
                    if (backupKey != ConsoleKey.N)
                    {
                        BackupSettings();
                        if(_exitCode == 0)
                            _exitCode = -1;
                        else
                            return;
                    }

                    Console.WriteLine("Press any key to start the migration process...");
                    Console.ReadKey();

                    Console.WriteLine("Migrating...");
                    XmlSerializer                  serializer16 = new XmlSerializer(typeof(Models._16Models.Configuration));
                    XmlSerializer                  serializer17 = new XmlSerializer(typeof(Models._17Models.Configuration));
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    Models._16Models.Configuration conf16;
                    Models._17Models.Configuration conf17 = new Models._17Models.Configuration();

                    using (Stream reader = new FileStream($"{selected}\\user.config", FileMode.Open))
                    {
                        // Call the Deserialize method to restore the object's state.
                        conf16 = (Models._16Models.Configuration) serializer16.Deserialize(reader);
                    }
                    Console.WriteLine($"\tRead config from '{selected}\\user.config'");

                    var servers = conf16.UserSettings.Settings.Setting.FirstOrDefault(s => s.Name == "Servers");
                    conf16.UserSettings.Settings.Setting.Remove(servers);
                    conf17.UserSettings = new Models._17Models.UserSettings { Settings = new Models._17Models.Settings { Setting = conf16.UserSettings.Settings.Setting } };
                    Console.WriteLine("\tConverted standard values to 1.7");

                    Debug.Assert(servers != null, nameof(servers) + " != null");
                    var node = ((XmlNode[]) servers.Value)[0];
                    var ser = new XmlSerializer(typeof(ServerCollection));
                    MemoryStream stm = new MemoryStream();

                    StreamWriter stw = new StreamWriter(stm);
                    stw.Write(node.OuterXml);
                    stw.Flush();

                    stm.Position = 0;
                    ServerCollection collec = ser.Deserialize(stm) as ServerCollection;
                    Console.WriteLine("\tExtracted profiles");

                    var newProfiles = new List<ServerProfile>();
                    Debug.Assert(collec != null, nameof(collec) + " != null");
                    foreach (var profile in collec.ServerProfile)
                    {
                        var newProfile = ConvertProfile(profile);
                        newProfiles.Add(newProfile);
                    }
                    
                    conf17.UserSettings.Settings.Setting.Add(new Setting
                    {
                        Name = "Profiles",
                        SerializeAs = "Xml",
                        Value = new Value
                        {
                            ArrayOfServerProfile = new ArrayOfServerProfile
                            {
                                ServerProfile = newProfiles
                            }
                        }
                    }); 
                    
                    Console.WriteLine("\tAdded new profiles to settings");
                    Console.WriteLine("Started serialization process...");
                   
                    
                    using (MemoryStream stream = new MemoryStream())
                    using (XmlTextWriter tw = new XmlTextWriter( stream, Encoding.UTF8))
                    {
                        tw.Formatting  = Formatting.Indented;
                        tw.Indentation = 4;
                        serializer17.Serialize(tw, conf17, ns);
                        tw.BaseStream.Position = 0;
                        Console.WriteLine("Serialization process complete.");

                        using StreamReader reader = new StreamReader(stream);
                        string text   = reader.ReadToEnd();
                        string output = Path.Combine(sourcePath, "1.7.1.0");
                        if (!Directory.Exists(output))
                            Directory.CreateDirectory(output);
                        File.WriteAllText(Path.Combine(output, "user.config"), text);
                        Console.WriteLine($"Settings were written to {output}\\user.config");
                    }
                    _exitCode = 0;
                    break;
                default :
                    Console.WriteLine("Invalid selection.");
                    break;
            }
        }

        private static ServerProfile ConvertProfile(Models._16Models.ServerProfile profile)
        {
            int    i;
            int    y;
            string label;
            Console.WriteLine($"\t ┌─Converting profile {profile.DisplayName}...");
            i     = 0;
            y     = 8;
            label = "Setting base values";
            var newProfile = new ServerProfile();
            ConvertBaseValues(profile, i, y, label, newProfile);
            Console.Write("\r\t │ Setting base values\n");

            i     = 0;
            y     = 23;
            label = "Setting Arma3Profile";
            Console.Write("\t └─Setting Arma3Profile");
            ConvertArmaProfile(profile, newProfile, i, y, label);
            Console.Write("\r\t │ Setting Arma3Profile\n");

            i     = 0;
            y     = 10;
            label = "Setting Basic.cfg";
            Console.Write("\t └─Setting Basic.cfg");
            ConvertBasic(profile, newProfile, i, y, label);
            Console.Write("\r\t │ Setting Basic.cfg\n");


            i     = 0;
            y     = 25;
            label = "Setting Server.cfg";
            Console.Write("\t └─Setting Server.cfg");
            ConvertServer(profile, newProfile, i, y, label);
            Console.Write("\r\t │ Setting Server.cfg\n");

            Console.Write($"\t └─Done converting {profile.DisplayName} to {newProfile.Id}\n");
            return newProfile;
        }

        private static void ConvertServer(Models._16Models.ServerProfile profile, ServerProfile newProfile, int i, int y, string label)
        {
            newProfile.ServerCfg = new FASTER.Models.ServerCfg();
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.AllowedFilePatching = profile.AllowFilePatching == "0"
                ? "No Clients"
                : "All Clients";
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.AutoInit = bool.Parse(profile.AutoInit);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.BattlEye = bool.Parse(profile.BattleEye);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.CommandLineParameters = profile.ExtraParams;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.Difficulty = profile.DifficultyPreset;
            UpdateStatus(ref i, y, label);
            int.TryParse(profile.DisconnectTimeout, out int disconnectTimeout);
            newProfile.ServerCfg.DisconnectTimeout = disconnectTimeout;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.DoubleIdDetected = profile.DoubleIdDetected;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.HeadlessClientEnabled = bool.Parse(profile.HeadlessClientEnabled);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.Hostname = profile.ServerName;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.Loopback = bool.Parse(profile.Loopback);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.MaxMem = uint.Parse(string.IsNullOrEmpty(profile.MaxMem)
                                                         ? "1024"
                                                         : profile.MaxMem);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.MaxPacketLoss = int.Parse(string.IsNullOrEmpty(profile.MaxPacketLoss)
                                                               ? "50"
                                                               : profile.MaxPacketLoss);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.MaxPing = int.Parse(string.IsNullOrEmpty(profile.MaxPing)
                                                         ? "200"
                                                         : profile.MaxPing);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.MaxPlayers = int.Parse(string.IsNullOrEmpty(profile.MaxPlayers)
                                                            ? "10"
                                                            : profile.MaxPlayers);
            UpdateStatus(ref i, y, label);
            int.TryParse(profile.MotdDelay, out int motdDelay);
            newProfile.ServerCfg.MotdInterval = motdDelay;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.Password = profile.Password;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.PasswordAdmin = profile.AdminPassword;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.Upnp = bool.Parse(profile.Upnp);
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.OnDifferentData = profile.OnDifferentData;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.OnHackedData = profile.OnHackedData;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.OnUnsignedData = profile.OnUnsignedData;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.OnUserConnected = profile.OnUserDisconnected;
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.VerifySignatures = profile.VerifySignatures == "true"
                ? "Activated"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ServerCfg.VotingEnabled = bool.Parse(profile.VotingEnabled);
            UpdateStatus(ref i, y, label);
        }

        private static void ConvertBasic(Models._16Models.ServerProfile profile, ServerProfile newProfile, int i, int y, string label)
        {
            newProfile.BasicCfg = new FASTER.Models.BasicCfg();
            UpdateStatus(ref i, y, label);
            ulong.TryParse(profile.MaxBandwidth, out ulong maxBandwidth);
            newProfile.BasicCfg.MaxBandwidth = maxBandwidth;
            UpdateStatus(ref i, y, label);
            ushort.TryParse(profile.MaxCustomFileSize, out ushort maxCustomFileSize);
            newProfile.BasicCfg.MaxCustomFileSize = maxCustomFileSize;
            UpdateStatus(ref i, y, label);
            ushort.TryParse(profile.MaxMessagesSend, out ushort maxMsgSend);
            newProfile.BasicCfg.MaxMsgSend = maxMsgSend;
            UpdateStatus(ref i, y, label);
            ushort.TryParse(profile.MaxPacketSize, out ushort maxPacketSize);
            newProfile.BasicCfg.MaxPacketSize = maxPacketSize;
            UpdateStatus(ref i, y, label);
            ushort.TryParse(profile.MaxSizeGuaranteed, out ushort maxSizeGuaranteed);
            newProfile.BasicCfg.MaxSizeGuaranteed = maxSizeGuaranteed;
            UpdateStatus(ref i, y, label);
            ushort.TryParse(profile.MaxSizeNonguaranteed, out ushort maxSizeNonGuaranteed);
            newProfile.BasicCfg.MaxSizeNonGuaranteed = maxSizeNonGuaranteed;
            UpdateStatus(ref i, y, label);
            ulong.TryParse(profile.MinBandwidth, out ulong minBandwidth);
            newProfile.BasicCfg.MinBandwidth = minBandwidth;
            UpdateStatus(ref i, y, label);
            newProfile.BasicCfg.MinErrorToSend = double.Parse(profile.MinErrorToSend.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            UpdateStatus(ref i, y, label);
            newProfile.BasicCfg.MinErrorToSendNear = double.Parse(profile.MinErrorToSendNear.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            UpdateStatus(ref i, y, label);
        }

        private static void ConvertArmaProfile(Models._16Models.ServerProfile profile, ServerProfile newProfile, int i, int y, string label)
        {
            newProfile.ArmaProfile = new Arma3Profile();
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.AutoReport = profile.AutoReporting == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.CameraShake = profile.CameraShake == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.Commands = profile.Commands;
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.DetectedMines = profile.DetectedMines;
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.EnemyTags = profile.EnemyNameTags;
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.FriendlyTags = profile.FriendlyNameTags;
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.GroupIndicators = profile.GroupIndicators;
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.MapContentEnemy = profile.MapContentEnemy == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.MapContentFriendly = profile.MapContentFriendly == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.MapContentMines = profile.MapContentMines == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.MultipleSaves = profile.MultipleSaves == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.PrecisionAi = double.Parse(profile.AiAccuracy.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.ReducedDamage = profile.ReducedDamage == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.ScoreTable = profile.ScoreTable == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.SkillAi = double.Parse(profile.AiSkill.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture);
            newProfile.ArmaProfile.StaminaBar = profile.StaminaBar == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.StanceIndicator = profile.StanceIndicator;
            newProfile.ArmaProfile.TacticalPing = profile.TacticalPing == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.ThirdPersonView = profile.ThirdPerson == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.VisionAid = profile.VisualAids == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.VonID = profile.VonId == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.Waypoints = profile.Waypoints;
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.WeaponCrosshair = profile.Crosshair == "true"
                ? "Enabled"
                : "Disabled";
            UpdateStatus(ref i, y, label);
            newProfile.ArmaProfile.WeaponInfo = profile.WeaponInfo;
            UpdateStatus(ref i, y, label);
        }

        private static void ConvertBaseValues(Models._16Models.ServerProfile profile, int i, int y, string label, ServerProfile newProfile)
        {
            UpdateStatus(ref i, y, label);
            newProfile.EnableHyperThreading = bool.Parse(profile.EnableHyperThreading);
            UpdateStatus(ref i, y, label);
            newProfile.Executable = profile.Executable;
            UpdateStatus(ref i, y, label);
            newProfile.HeadlessNumber = int.Parse(profile.NoOfHeadlessClients);
            UpdateStatus(ref i, y, label);
            newProfile.Id = $"_{Guid.NewGuid():N}";
            UpdateStatus(ref i, y, label);
            newProfile.Name = profile.DisplayName;
            UpdateStatus(ref i, y, label);
            newProfile.Port = int.Parse(profile.Port);
            UpdateStatus(ref i, y, label);
            newProfile.RankingChecked = bool.Parse(profile.RankingEnabled);
            UpdateStatus(ref i, y, label);
        }

        private static void UpdateStatus(ref int i, int y, string label)
        {
            i += 1;
            Console.Write($"\r\t └─{label,-25}({i * 100 / y}%)");
            Thread.Sleep(25);
        }

        private static void BackupSettings()
        {
            using var fbd = new FolderBrowserDialog
            {
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                Description = "Select a folder to save the backups"
            };
            if (fbd.ShowDialog() != DialogResult.OK)
            { return; }

            ClearConsole();
            var folder = fbd.SelectedPath;
            if (!Directory.Exists(folder))
            {
                Console.WriteLine("The selected directory does not exist on the disk.");
                _exitCode = 101;
                return;
            }

            Console.WriteLine($"\nBacking up FASTER settings to \"{folder}\"...");
            var sourcePath = Path.Combine(Environment.GetEnvironmentVariable("LocalAppData") ?? throw new InvalidOperationException(), "FoxliCorp", "FASTER_StrongName_r3kmcr0zqf35dnhwrlga5cvn2azjfziz");
            var destPath = Path.Combine(folder, $"BACKUP-{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}");
            if (!Directory.Exists(destPath))
            { Directory.CreateDirectory(destPath); }
            Console.Write("Copying files...");
            var sourcefiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            var errors = new List<string>();
            uint i = 1;
            foreach (var file in sourcefiles)
            {
                try
                {
                    string dest = $"{destPath}{file.Replace(sourcePath, "")}";
                    string filename = Path.GetFileName(dest);
                    if (!Directory.Exists(dest.Replace(filename, "")))
                    { Directory.CreateDirectory(dest.Replace(filename, "")); }

                    File.Copy(file, dest, true);
                    Console.Write($"\rCopying files... {i * 100 / sourcefiles.Length}% ({i}/{sourcefiles.Length} files)");
                    i += 1;
                }
                catch
                { errors.Add($"Could not copy file {file} to {destPath}. Please try again manually..."); }
            }

            Console.WriteLine(string.Join("\n", errors));
            Console.WriteLine("Backup finished !");
            _exitCode = 0;
        }

        private static void ClearConsole()
        {
            Console.Clear();
            Console.WriteLine($"\t____[ FASTER MAINTENANCE v{Version} ]____");
            Console.WriteLine("\n\nWelcome to FASTER MAINTENANCE");
            Console.WriteLine("This tool may help in case of trouble with FASTER");
        }
    }
}
