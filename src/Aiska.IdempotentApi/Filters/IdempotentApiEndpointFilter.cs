using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Models;
using Aiska.IdempotentApi.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Aiska.IdempotentApi.Filters
{
    internal sealed class IdempotentApiEndpointFilter(IServiceProvider serviceProvider, IOptions<IdempotentApiOptions> options, IOptions<JsonOptions> jsonOptions)
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next, IdempotentParameter parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);

            IIdempotentApiProvider provider = serviceProvider.GetRequiredService<IIdempotentApiProvider>();

            if (!provider.IsValidIdempotent(context.HttpContext.Request))
            {
                return await next(context);
            }

            string headerKey = context.HttpContext.Request.Headers[parameter.IdempotentHeaderName ?? options.Value.KeyHeaderName].ToString();
            if (string.IsNullOrEmpty(headerKey))
            {
                return TypedResults.BadRequest(provider.MissingHeaderError());
            }

            IdempotentRequest idempotentRequest = new()
            {
                IdempotentHeaderKey = headerKey,
                ExpirationFromMinutes = (parameter.ExpirationFromMinutes > 0) ? parameter.ExpirationFromMinutes : options.Value.ExpirationFromMinutes,
                RequestData = GetRequestData(parameter, context)
            };


            IdempotentResponse idempotentResult = await provider.ProcessIdempotentAsync(idempotentRequest);
            switch (idempotentResult.Type)
            {
                case IdempotentEnumResult.Reuse:
                    return TypedResults.UnprocessableEntity(provider.ReuseError());
                case IdempotentEnumResult.Retried:
                    return TypedResults.Conflict(provider.RetriedError());
                case IdempotentEnumResult.CacheMiss:
                    object? resultContext = await next(context).ConfigureAwait(false);
                    int? statusCodeInt = null;
                    object? valueToCache = null;
                    if (resultContext is IValueHttpResult valueResult)
                    {
                        valueToCache = valueResult.Value;
                    }

                    if (resultContext is IStatusCodeHttpResult result)
                    {
                        statusCodeInt = result.StatusCode;
                    }

                    IdempotentCacheData dataCache = new()
                    {
                        HashValue = idempotentResult.HashValue,
                        ResponseCache = valueToCache,
                        StatusCode = statusCodeInt,
                        IsCompleted = true
                    };
                    await provider.SetCacheAsync(headerKey, dataCache);
                    return resultContext;
                case IdempotentEnumResult.CacheHit:
                default:
                    int? statusCodeFromCacheInt = idempotentResult.statusCode;
                    object? cachedValue = idempotentResult.ResponseCache;
                    HttpStatusCode? statusCodeEnum = (HttpStatusCode?)statusCodeFromCacheInt;
                    return ParseResult(context, statusCodeEnum, cachedValue);
            }
        }

        private static object? ParseResult(EndpointFilterInvocationContext context, HttpStatusCode? statusCode, object? value)
        {
            HttpRequest request = context.HttpContext.Request;
            UriBuilder uriBuilder = new(
                request.Scheme,
                request.Host.Host,
                request.Host.Port ?? -1, // Use -1 to indicate the default port for the scheme
                request.PathBase + request.Path,
                request.QueryString.ToString()
            );
            Uri uri = uriBuilder.Uri;
            switch (statusCode)
            {
                case HttpStatusCode.OK:
                    return TypedResults.Ok(value);
                case HttpStatusCode.Created:
                    return TypedResults.Created(uri, value);
                case HttpStatusCode.Accepted:
                    return TypedResults.Accepted(uri, value);
                case HttpStatusCode.NoContent:
                    return TypedResults.NoContent();
                case HttpStatusCode.BadRequest:
                    return TypedResults.BadRequest(value);
                case HttpStatusCode.Unauthorized:
                    return TypedResults.Unauthorized();
                case HttpStatusCode.Forbidden:
                    return TypedResults.Forbid();
                case HttpStatusCode.NotFound:
                    return TypedResults.NotFound();
                case HttpStatusCode.Conflict:
                    return TypedResults.Conflict(value);
                default:
                    break;
            }
            return null;
        }

        private string GetRequestData(IdempotentParameter parameter, EndpointFilterInvocationContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            string contentType = request.ContentType ?? string.Empty;
            JsonSerializerOptions serializerOptions = jsonOptions.Value.SerializerOptions;

            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream, new JsonWriterOptions
            {
                Indented = false // Crucial for a consistent hash string
            });

            if (contentType.StartsWith(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
            {
                for (int i = 0; i < context.Arguments.Count; i++)
                {
                    if (i >= parameter.Attributes.Length)
                    {
                        continue;
                    }

                    JsonNode? node = JsonSerializer.SerializeToNode(context.Arguments[i], serializerOptions);
                    if (node is JsonObject jsonObject)
                    {
                        string[] excludes = parameter.Attributes[i].Excludes;
                        foreach (string? item in excludes.ToList())
                        {
                            jsonObject.Remove(item);
                        }
                        jsonObject.WriteTo(writer, serializerOptions);
                    }
                    else
                    {
                        node?.WriteTo(writer, serializerOptions);
                    }
                }
            }
            else if (contentType.StartsWith(MediaTypeNames.Application.FormUrlEncoded, StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, object?> dataResult = [];
                for (int i = 0; i < parameter.Attributes.Length; i++)
                {
                    if (!parameter.Attributes[i].Excludes.Contains(parameter.Attributes[i].Name, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!parameter.Attributes[i].Ignore)
                        {
                            dataResult.Add(parameter.Attributes[i].Name, context.Arguments[i]);
                        }
                    }
                }
                JsonSerializer.Serialize(writer, dataResult, serializerOptions);
            }

            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        //private string GetRequestData(IdempotentParameter parameter, EndpointFilterInvocationContext context)
        //{
        //    string contentType = context.HttpContext.Request.ContentType?.ToString() ?? string.Empty;
        //    StringBuilder sb = new();
        //    if (contentType.StartsWith(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
        //    {
        //        for (int i = 0; i < context.Arguments.Count; i++)
        //        {
        //            JsonNode? node = JsonSerializer.SerializeToNode(context.Arguments[i], jsonOptions.Value.SerializerOptions);
        //            if (node is JsonObject jsonObject)
        //            {
        //                foreach (var item in parameter.Attributes[i].Excludes)
        //                {
        //                    jsonObject.Remove(item);
        //                }
        //                sb.Append(node.ToJsonString());
        //            }

        //        }
        //    }
        //    else if (contentType.StartsWith(MediaTypeNames.Application.FormUrlEncoded, StringComparison.OrdinalIgnoreCase))
        //    {
        //        Dictionary<string, object?> dataResult = [];
        //        for (int i = 0; i < parameter.Attributes.Length; i++)
        //        {
        //            if (!parameter.Attributes[i].Excludes.Contains(parameter.Attributes[i].Name, StringComparer.OrdinalIgnoreCase))
        //            {
        //                if (!parameter.Attributes[i].Ignore)
        //                {
        //                    dataResult.Add(parameter.Attributes[i].Name, context.Arguments[i]);
        //                }
        //            }
        //        }
        //        sb.Append(JsonSerializer.Serialize(dataResult, jsonOptions.Value.SerializerOptions));
        //    }
        //    return sb.ToString();
        //}
    }
}
