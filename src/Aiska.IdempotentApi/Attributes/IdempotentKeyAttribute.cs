namespace Aiska.IdempotentApi.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class IdempotentKeyAttribute : Attribute
    {
    }
}
