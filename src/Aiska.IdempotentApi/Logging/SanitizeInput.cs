using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Aiska.IdempotentApi.Logging
{
    public static partial class StringHelper
    {
        public static string SanitizeInput(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            string sanitized = input.Replace("\r", "").Replace("\n", "");
            AsciiOnly().Replace(sanitized, "");

            return sanitized;
        }

        [GeneratedRegex(@"[^ -~]")]
        private static partial Regex AsciiOnly();
    }
}
