namespace Aiska.IdempotentApi.Extensions
{
    internal static class StringHelper
    {
        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Create a char array to build the new string
            char[] charArray = new char[input.Length];
            Span<char> charSpan = charArray.AsSpan();

            // Convert the first character to lowercase and place it in the span
            charSpan[0] = char.ToLowerInvariant(input[0]);

            // Copy the rest of the string into the span
            input.AsSpan(1).CopyTo(charSpan[1..]);

            // Create a new string from the modified char array
            return new string(charArray);
        }
    }
}
