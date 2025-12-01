using Microsoft.Extensions.Primitives;

namespace Aiska.IdempotentApi.Models
{
    internal class IdempotentHttpResponseModel
    {
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public Dictionary<string, StringValues>? Headers { get; set; }
        public string? Body { get; set; } = null;
        //public string ResultType { get; set; }

    }
}
