using Microsoft.Win32;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Documents;

namespace FASTER.Models
{
    internal static class Functions
    {
        public static void CheckSettings()
        {
            if (!Directory.Exists(Properties.Settings.Default.serverPath))
                Properties.Settings.Default.serverPath = string.Empty;

            if (!Directory.Exists(Properties.Settings.Default.steamCMDPath))
                Properties.Settings.Default.steamCMDPath = string.Empty;
        }

        public static string ParseFileSize(long size)
        {
            double   fullSize = size;
            return ParseFileSize(fullSize);
        }

        public static string ParseFileSize(ulong size)
        {
            double   fullSize = size;
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

        public static string StringFromRichTextBox(System.Windows.Controls.RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            return textRange.Text;
        }

        public static string SelectFile(string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            { Filter = filter };
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
            // input = Regex.Replace(input, "[^a-zA-Z0-9\-_]", replacement) >> "-" is allowed
            input = Regex.Replace(input, "[^a-zA-Z0-9_]", replacement);
            input = input.Replace(replacement + replacement, replacement);
            return input;
        }

        //Opens a browser url
        public static void OpenBrowser(string url)
        {
            try
            { Process.Start(url); }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                { Process.Start("xdg-open", url); }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                { Process.Start("open", url); }
                else
                { throw; }
            }
        }

        internal static string GetVersion()
        {
            var    assembly                      = Assembly.GetExecutingAssembly().GetName().Version;
            if (assembly == null) return "UNKNOWN";
            string rev                           = $"{(char)(assembly.Build + 96)}";
            if (assembly.Build == 0) rev = "ALPHA";
            if (assembly.Revision != 0)
            {
                string releaseType = assembly.Revision.ToString().Substring(0, 1) switch
                                     {
                                         "1" => "H",  // HOTFIX
                                         "2" => "RC", // RELEASE CANDIDATE
                                         "5" => "D",  // DEV
                                         _   => ""    // EMPTY RELEASE TYPE
                                     };

                rev += $" {releaseType}{int.Parse(assembly.Revision.ToString().Substring(1))}";
            }
#if DEBUG
            rev += "-DEV";
#endif
            string version = $"{assembly.Major}."
                             + $"{assembly.Minor}"
                             + $"{rev}";
            return version;
        }
    }
}
