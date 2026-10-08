using Microsoft.Win32;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FASTER.Models
{
    public static class Functions
    {
        public static void CheckSettings()
        {
            if (!Directory.Exists(Properties.Settings.Default.serverPath))
                Properties.Settings.Default.serverPath = string.Empty;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.steamCMDPath))
            {
                try
                {
                    Properties.Settings.Default.steamCMDPath = Path.GetFullPath(Properties.Settings.Default.steamCMDPath);
                }
                catch (Exception)
                {
                    Properties.Settings.Default.steamCMDPath = string.Empty;
                }
            }
        }

        public static string ParseFileSize(long size)
        {
            double fullSize = size;
            return ParseFileSize(fullSize);
        }

        public static string ParseFileSize(ulong size)
        {
            double fullSize = size;
            return ParseFileSize(fullSize);
        }

        public static string ParseFileSize(double fullSize)
        {
            string[] sizes    = {" B", "KB", "MB", "GB", "TB"};
            var      order    = 0;

            while (fullSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                fullSize /= 1024.0;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return $"{fullSize,7:F} {sizes[order],-2}";
        }

        public static string SelectFile(string filter)
        {
            OpenFileDialog openFileDialog = new() { Filter = filter };
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        // Takes any string and removes illegal characters
        public static string SafeName(string input, bool ignoreWhiteSpace = false, string replacement = "_")
        {
            input = input.Replace("@", "");
            if (ignoreWhiteSpace)
            {
                // input = Regex.Replace(input, "[^a-zA-Z0-9\-_\s]", replacement) >> "-" is allowed
                input = Regex.Replace(input, @"[^a-zA-Z0-9_\s]", replacement);
                input = input.Replace(replacement + replacement, replacement);
                return input;
            }
            input = Regex.Replace(input, "[^a-zA-Z0-9_]", replacement);
            input = input.Replace(replacement + replacement, replacement);
            return input;
        }

        // Folder/symlink name for a mod (without the leading "@").
        // Names with only ASCII letters/digits behave exactly like SafeName, so existing links keep working.
        // Names containing non-ASCII letters/digits would collapse into underscores and collide with
        // other mods, so the Workshop ID is added to keep them unique.
        public static string ModFolderName(string name, uint workshopId)
        {
            var safe = SafeName(name ?? string.Empty);

            // No usable name (Steam lookup failed, or the name was only "@"): use the Workshop ID
            if (safe.Length == 0)
                return $"mod_{workshopId}";

            if (!name.Any(c => c > 127 && char.IsLetterOrDigit(c)))
                return safe;

            safe = safe.Trim('_');
            return $"{(safe.Length == 0 ? "mod" : safe)}_{workshopId}";
        }

        //Opens a browser url
        public static void OpenBrowser(string url)
        { 
		    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); 
        }

        internal static string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName().Version;
            
            if (assembly == null) 
                return "UNKNOWN";
            
            string rev = $"{(char)(assembly.Build + 96)}";
            
            if (assembly.Build == 0) 
                rev = "ALPHA";
            if (assembly.Revision != 0)
            {
                string releaseType = (assembly.Revision / 100) switch
                                     {
                                         1 => "H",  // HOTFIX
                                         2 => "RC", // RELEASE CANDIDATE
                                         5 => "D",  // DEV
                                         _ => ""    // EMPTY RELEASE TYPE
                                     };
                if(releaseType != "")
                     rev += $" {releaseType}{int.Parse(assembly.Revision.ToString()[1..])}";
            }
#if DEBUG
            rev += "-DEV";
#endif
            string version = $"{assembly.Major}."
                             + $"{assembly.Minor}"
                             + $"{rev}";
            return version;
        }
        
        internal static string GetRawVersion()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName().Version;
            return assembly.ToString();
        }
    }
}
