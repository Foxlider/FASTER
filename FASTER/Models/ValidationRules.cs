using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Controls;

namespace FASTER.Models
{
    public class ExeLocationRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public ExeLocationRule()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string exe = (string)value;

            if (!exe.Contains("arma3server") && !exe.EndsWith(".exe"))
            { return new ValidationResult(false, $"The file provided is not an arma3 server exe."); }
            return ValidationResult.ValidResult;
        }
    }
}
