using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

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
            Console.WriteLine("\n\nUNHANDLED YET ! PLEASE CONATCT THE DEV FOR AN UPDATE.");
            _exitCode = 1;
        }

        private static void SetEnvVar()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                fbd.Description  = "Select FASTER's new install folder";
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
        }

        private static void MigrateTo17()
        {
            throw new NotImplementedException();
        }

        private static void BackupSettings()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                fbd.Description  = "Select a folder";
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
                var destPath   = Path.Combine(folder, $"BACKUP-{DateTime.Now.Day}-{DateTime.Now.Month}-{DateTime.Now.Year}");
                if(!Directory.Exists(destPath))
                { Directory.CreateDirectory(destPath); }
                Console.Write("Copying files...");
                var  sourcefiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                var  errors      = new List<string>();
                uint i           = 1;
                foreach (var file in sourcefiles)
                {
                    try
                    {
                        string dest     = $"{destPath}{file.Replace(sourcePath, "")}";
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
