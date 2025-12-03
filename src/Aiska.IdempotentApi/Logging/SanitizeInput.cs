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
                return "[empty]";
            }

            string sanitized = input.Replace("\r", "").Replace("\n", "");
            sanitized = AsciiOnly().Replace(sanitized, "");

            return $"[{sanitized}]";
        }

        [GeneratedRegex(@"[\x00-\x09\x0B-\x1F\x7F]")]
        [GeneratedRegex(@"[^ -~]")]
        private static partial Regex AsciiOnly();
    }
}
