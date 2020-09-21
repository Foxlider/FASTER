﻿using Microsoft.Win32;

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Serialization;

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

        public static T CloneObjectSerializable < T > (this T obj) where T: class {  
            MemoryStream    ms = new MemoryStream();  
            BinaryFormatter bf = new BinaryFormatter();  
            bf.Serialize(ms, obj);  
            ms.Position = 0;  
            object result = bf.Deserialize(ms);  
            ms.Close();  
            return (T) result;  
        }  

        public static StringCollection GetLinesCollectionFromTextBox(System.Windows.Controls.TextBox textBox)
        {
            var lines = new StringCollection();
            lines.AddRange(textBox.Text.Split('\n'));
            return lines;
        }

        public static string StringFromRichTextBox(System.Windows.Controls.RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            return textRange.Text;
        }

        // Retrieves mods from an XML file
        public static List<SteamMod> GetModsFromXml(string filename)
        {
            var xml = File.ReadAllText(filename);
            return Deserialize<List<SteamMod>>(xml);
        }

        public static void ExportModsToXml(string filename, List<SteamMod> mods)
        { File.WriteAllText(filename, Serialize(mods)); }

        // Serialise a class
        private static string Serialize<T>(T value)
        {
            if (value == null)
                return null;

            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                Encoding = new UnicodeEncoding(false, false),
                Indent = true,
                OmitXmlDeclaration = false
            };

            using var textWriter = new StringWriter();
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
            { serializer.Serialize(xmlWriter, value); }

            return textWriter.ToString();
        }

        // Deserialise a class
        private static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                return default;

            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlReaderSettings();

            using var textReader = new StringReader(xml);
            using XmlReader xmlReader = XmlReader.Create(textReader, settings);
            return (T)serializer.Deserialize(xmlReader);
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
            //input = input.Replace("@", ""); Local mods can have full paths, this is useless and only breaks them.
            if (ignoreWhiteSpace)//I'm assuming that for "ignore whitespace" we are meant to leave it untouched
            {
                // Theese are the only characters not allowed on windows. Anything else works fine.
                // Replacing characters that are legal in paths causes the local mods to stop being loaded properly.
                // Ideally this whole operation should happen only on workshop mods.
                input = Regex.Replace(input, "[<>\"/\\|?*]", replacement);
                input = input.Replace(replacement + replacement, replacement);//wtf is going on there?
                return input;
            }
            input = Regex.Replace(input, "[<>\"/\\|?*\\s]", replacement);
            input = input.Replace(replacement + replacement, replacement);//wtf is going on there?
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
            string rev = $"{(char)(Assembly.GetExecutingAssembly().GetName().Version.Build + 96)}";
            if (Assembly.GetExecutingAssembly().GetName().Version.Build == 0) rev = "ALPHA";
#if DEBUG
            rev += "-DEV";
#endif
            string version = $"{Assembly.GetExecutingAssembly().GetName().Version.Major}."
                             + $"{Assembly.GetExecutingAssembly().GetName().Version.Minor}"
                             + $"{rev}";
            return version;
        }
    }
}
