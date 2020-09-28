using System.Text.RegularExpressions;

namespace FASTER.Models
{
    internal static class Functions
    {
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
    }
}
