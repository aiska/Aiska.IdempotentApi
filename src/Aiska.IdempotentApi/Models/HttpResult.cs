namespace Aiska.IdempotentApi.Models
{
    //public class HttpResult<T>
    //{
    //    public int? StatusCode { get; set; }
    //    public string? ContentType { get; set; }
    //    public T? Value { get; internal set; }
    //}
    public class HttpResult
    {
        public int? StatusCode { get; set; }
        public string? ContentType { get; set; }
        public object? Value { get; internal set; }
    }
}
