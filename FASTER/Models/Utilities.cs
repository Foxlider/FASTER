using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace FASTER.Models
{
    public static class ModUtilities
    {
        // Accepts a plain Workshop ID or a steamcommunity.com link with ?id=...
        public static bool TryParseModId(string input, out uint modId)
        {
            modId = 0;
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim();

            if (uint.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out modId))
                return modId != 0;

            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri)
                || !(uri.Host.Equals("steamcommunity.com", StringComparison.OrdinalIgnoreCase)
                     || uri.Host.EndsWith(".steamcommunity.com", StringComparison.OrdinalIgnoreCase)))
                return false;

            var id = System.Web.HttpUtility.ParseQueryString(uri.Query).Get("id");
            return uint.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out modId) && modId != 0;
        }
		
        public static string GetCompareString(string input)
        {
            input = input.Replace("@", "");
            input = Regex.Replace(input, @"[^\p{L}\p{Nd}]", string.Empty);
            return input;
        }

        // True when two mod names are the same once punctuation is ignored.
        // A name that strips down to nothing never matches by name.
        public static bool NamesMatch(string a, string b)
        {
            var compareA = GetCompareString(a ?? string.Empty);
            return compareA.Length > 0 && compareA == GetCompareString(b ?? string.Empty);
        }

        // We're parsing to ArmaMod instead of ProfileMod because there's more info on the ArmaMod object and we can thus re-use this function
        public static List<ArmaMod> ParseModsFromArmaProfileFile(string filePath)
        {
            if (!File.Exists(filePath)) // This should never happen, but it pays to be safe.
                return new List<ArmaMod>();
            var lines = File.ReadAllText(filePath);

            List<ArmaMod> extractedModlist = new();
            XmlDocument doc = new();
            try
            { doc.LoadXml(lines); }
            catch (XmlException e)
            {
                Logger.Log($"ParseModsFromArmaProfileFile: could not parse {filePath}: {e.Message}");
                return extractedModlist;
            }
            var modNodes = doc.SelectNodes("//tr[@data-type=\"ModContainer\"]");
            if (modNodes == null)
                return extractedModlist;

            for (int i = 0; i < modNodes.Count; i++)
            {
                var modNode = modNodes.Item(i);
                var modName = modNode?.SelectSingleNode("td[@data-type='DisplayName']")?.InnerText;
                if (modNode == null || modName == null)
                    continue;

                var modIdNode  = modNode.SelectSingleNode("td/a[@data-type='Link']");
                var href       = modIdNode?.Attributes?.GetNamedItem("href")?.Value;
                var isSteamMod = TryParseModId(href, out var modId);
                if (!isSteamMod)
                    modId = (uint)(uint.MaxValue - Random.Shared.Next(ushort.MaxValue / 2));
                var modIdS = modId.ToString();

                ArmaMod mod = new()
                {
                    WorkshopId = modId,
                    Path = Path.Combine(Properties.Settings.Default.modStagingDirectory, modIdS),
                    Name = modName,
                    IsLocal = !isSteamMod,
                    Status = !isSteamMod ? ArmaModStatus.Local : ArmaModStatus.UpToDate,
                };
                extractedModlist.Add(mod);
            }

            return extractedModlist;
        }
    }
	
    public static class TextBoxUtilities
    {
        public static readonly DependencyProperty AlwaysScrollToEndProperty = DependencyProperty.RegisterAttached("AlwaysScrollToEnd",
                                                                                                                  typeof(bool),
                                                                                                                  typeof(TextBoxUtilities),
                                                                                                                  new PropertyMetadata(false, AlwaysScrollToEndChanged));

        private static void AlwaysScrollToEndChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                bool alwaysScrollToEnd = (e.NewValue != null) && (bool) e.NewValue;

                if (alwaysScrollToEnd)
                {
                    tb.ScrollToEnd();
                    tb.TextChanged += TextChanged;
                }
                else
                { tb.TextChanged -= TextChanged; }
            }
            else
            { throw new InvalidOperationException("The attached AlwaysScrollToEnd property can only be applied to TextBox instances."); }
        }

        public static bool GetAlwaysScrollToEnd(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            return (bool) textBox.GetValue(AlwaysScrollToEndProperty);
        }

        public static void SetAlwaysScrollToEnd(TextBox textBox, bool alwaysScrollToEnd)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException(nameof(textBox));
            }

            textBox.SetValue(AlwaysScrollToEndProperty, alwaysScrollToEnd);
        }

        private static void TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox) sender).ScrollToEnd();
        }
    }
}
