namespace Aiska.IdempotentApi.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IdempotentExcludeAttribute : Attribute
    {
        public IdempotentExcludeAttribute(params string[] excludes)
        {
            List<string> items = new(excludes.Length);
            foreach (string item in excludes)
            {
                items.AddRange(SplitString(item));
            }

            Exclude = [.. items];
        }

        private static string[] SplitString(string original)
        {
            return original?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
        }

        public string[] Exclude { get; }
    }
}
