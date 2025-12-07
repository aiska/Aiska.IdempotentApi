namespace Aiska.IdempotentApi.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class IdempotentIgnoreAttribute : Attribute
    {
    }
}
