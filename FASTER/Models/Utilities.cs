using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace FASTER.Models
{
    public static class ModUtilities
    {
        public static string GetCompareString(string input)
        {
            input = input.Replace("@", "");
            input = Regex.Replace(input, "[^a-zA-Z0-9]", string.Empty);
            return input;
        }

        // We're parsing to ArmaMod instead of ProfileMod because there's more info on the ArmaMod object and we can thus re-use this function
        public static List<ArmaMod> ParseModsFromArmaProfileFile(string filePath)
        {
            if (!File.Exists(filePath)) // This should never happen, but it pays to be safe.
                return new List<ArmaMod>();
            var lines = File.ReadAllText(filePath);

            List<ArmaMod> extractedModlist = new();
            XmlDocument doc = new();
            doc.LoadXml(lines);
            var modNodes = doc.SelectNodes("//tr[@data-type=\"ModContainer\"]");
            for (int i = 0; i < modNodes.Count; i++)
            {
                var modNode = modNodes.Item(i);
                var modName = modNode.SelectSingleNode("td[@data-type='DisplayName']").InnerText;
                var modIdNode = modNode.SelectSingleNode("td/a[@data-type='Link']");
                Random r = new();
                var modId = (uint)(uint.MaxValue - r.Next(ushort.MaxValue / 2));
                var modIdS = modId.ToString();
                if (modIdNode != null)
                {
                    modIdS = modIdNode.Attributes.GetNamedItem("href").Value.Split("?id=")[1].Split('"')[0];
                    uint.TryParse(modIdS, out modId);
                }

                ArmaMod mod = new()
                {
                    WorkshopId = modId,
                    Path = Path.Combine(Properties.Settings.Default.modStagingDirectory, modIdS),
                    Name = modName,
                    IsLocal = modIdNode == null,
                    Status = modIdNode == null ? ArmaModStatus.Local : ArmaModStatus.UpToDate,
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
                bool alwaysScrollToEnd = (e.NewValue != null) && (bool)e.NewValue;

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

            return (bool)textBox.GetValue(AlwaysScrollToEndProperty);
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
            ((TextBox)sender).ScrollToEnd();
        }
    }
}
