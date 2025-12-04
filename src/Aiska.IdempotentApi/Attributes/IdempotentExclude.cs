namespace Aiska.IdempotentApi.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class IdempotentExcludeAttribute : Attribute
    {
        public IdempotentExcludeAttribute(params string[] excludes)
        {
            var items = new List<string>(excludes.Length);
            foreach (var item in excludes)
            {
                items.AddRange(SplitString(item));
            }

            Exclude = [.. items];
        }

        private static string[] SplitString(string original)
               => original?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];

        public string[] Exclude { get; }
    }
}
