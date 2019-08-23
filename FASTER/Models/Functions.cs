using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
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
            if (!Directory.Exists(Properties.Options.Default.serverPath))
                Properties.Options.Default.serverPath = string.Empty;

            if (!Directory.Exists(Properties.Options.Default.steamCMDPath))
                Properties.Options.Default.steamCMDPath = string.Empty;
        }


        public static StringCollection GetLinesCollectionFromTextBox(System.Windows.Controls.TextBox textBox)
        {
            //var lines = new StringCollection();
            //int lineCount = textBox.LineCount;

            //for (var line = 0; line <= lineCount - 1; line++)
            //    lines.Add(textBox.GetLineText(line));

            //return lines;
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
        {
            File.WriteAllText(filename, Serialize(mods));
        }

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
            {
                serializer.Serialize(xmlWriter, value);
            }

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
            {
                Filter = filter
            };
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        // Takes any string and removes illegal characters
        public static string SafeName(string input, bool ignoreWhiteSpace = false, string replacement = "_")
        {
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
    }
}
