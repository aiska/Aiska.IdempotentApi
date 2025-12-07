using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Attributes;
using Aiska.IdempotentApi.Models;
using Aiska.IdempotentApi.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Aiska.IdempotentApi.Filters
{
    internal sealed class InternalIdempotenFilter(IIdempotentApiProvider provider,
        IOptions<IdempotentApiOptions> options,
        IOptions<JsonOptions> jsonOptions,
        IdempotentAttribute attribute
        ) : IActionFilter, IAsyncActionFilter, IAsyncResultFilter
    {
        private string headerKey = string.Empty;
        private IdempotentResponse idempotentResult = new();

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (provider.IsValidIdempotent(context.HttpContext.Request))
            {
                string headerKeyName = string.IsNullOrEmpty(attribute.HeaderKeyName) ? options.Value.KeyHeaderName : attribute.HeaderKeyName;
                headerKey = context.HttpContext.Request.Headers[headerKeyName].ToString();

                if (headerKey == string.Empty)
                {
                    context.Result = new BadRequestObjectResult(provider.MissingHeaderError());
                }
                else
                {
                    IdempotentParameter idempotentParameter = GetIdempotentParameter(context);
                    IdempotentRequest idempotentResquest = new()
                    {
                        ExpirationFromMinutes = idempotentParameter.ExpirationFromMinutes > 0 ? idempotentParameter.ExpirationFromMinutes : options.Value.ExpirationFromMinutes,
                        IdempotentHeaderKey = headerKey,
                        RequestData = GetRequestData(idempotentParameter, context)
                    };

                    idempotentResult = await provider.ProcessIdempotentAsync(idempotentResquest).ConfigureAwait(false);
                    switch (idempotentResult.Type)
                    {
                        case IdempotentEnumResult.Reuse:
                            context.Result = new UnprocessableEntityObjectResult(provider.ReuseError());
                            return;
                        case IdempotentEnumResult.Retried:
                            context.Result = new ConflictObjectResult(provider.RetriedError());
                            return;
                        case IdempotentEnumResult.CacheHit:
                            int? statusCodeFromCacheInt = idempotentResult.statusCode;
                            object? cachedValue = idempotentResult.ResponseCache;
                            HttpStatusCode? statusCodeEnum = (HttpStatusCode?)statusCodeFromCacheInt;
                            context.Result = ParseResult(context, statusCodeEnum, cachedValue);
                            return;
                        default:
                            await next();
                            break;
                    }
                }
            }
        }

        private string GetRequestData(IdempotentParameter parameter, ActionExecutingContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            string contentType = request.ContentType ?? string.Empty;
            JsonSerializerOptions serializerOptions = jsonOptions.Value.JsonSerializerOptions;

            using MemoryStream stream = new();
            using Utf8JsonWriter writer = new(stream, new JsonWriterOptions
            {
                Indented = false // Crucial for a consistent hash string
            });

            if (contentType.StartsWith(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
            {
                int i = 0;
                foreach (KeyValuePair<string, object?> argument in context.ActionArguments)
                {
                    JsonNode? node = JsonSerializer.SerializeToNode(argument, jsonOptions.Value.JsonSerializerOptions);

                    if (!parameter.Attributes[i].Ignore)
                    {
                        if (node is JsonObject jsonObject)
                        {
                            foreach (string attribute in parameter.Attributes[i].Excludes)
                            {
                                jsonObject.Remove(attribute);
                            }
                            jsonObject.WriteTo(writer, serializerOptions);
                        }
                        else
                        {
                            node?.WriteTo(writer, serializerOptions);
                        }
                        i++;
                    }
                }
            }
            else if (contentType.StartsWith(MediaTypeNames.Application.FormUrlEncoded, StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<string, object?> dataResult = [];
                for (int i = 0; i < parameter.Attributes.Length; i++)
                {
                    if (!parameter.Attributes[i].Ignore)
                    {
                        object? argument = context.ActionArguments[parameter.Attributes[i].Name];
                        if (!parameter.Attributes[i].Excludes.Contains(parameter.Attributes[i].Name, StringComparer.OrdinalIgnoreCase))
                        {
                            if (!parameter.Attributes[i].Ignore)
                            {
                                dataResult.Add(parameter.Attributes[i].Name, argument);
                            }
                        }
                    }
                }
                JsonSerializer.Serialize(writer, dataResult, serializerOptions);
            }
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (idempotentResult.Type == IdempotentEnumResult.CacheMiss)
            {
                object? value = null;
                int? statusCode = null;
                if (context.Result is ObjectResult valueResult)
                {
                    value = valueResult.Value;
                }

                if (context.Result is IStatusCodeActionResult statusResult)
                {
                    statusCode = statusResult.StatusCode;
                }

                IdempotentCacheData dataCache = new()
                {
                    HashValue = idempotentResult.HashValue,
                    ResponseCache = value,
                    StatusCode = statusCode,
                    IsCompleted = true,
                };
                await provider.SetCacheAsync(headerKey, dataCache).ConfigureAwait(false);
            }
            await next();
        }

        private static IActionResult? ParseResult(ActionExecutingContext context, HttpStatusCode? statusCode, object? value)
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
                    return new OkObjectResult(value);
                case HttpStatusCode.Created:
                    return new CreatedResult(uri, value);
                case HttpStatusCode.Accepted:
                    return new AcceptedResult(uri, value);
                case HttpStatusCode.NoContent:
                    return new NoContentResult();
                case HttpStatusCode.BadRequest:
                    return new BadRequestObjectResult(value);
                case HttpStatusCode.Unauthorized:
                    return new UnauthorizedObjectResult(value);
                case HttpStatusCode.Forbidden:
                    return new ForbidResult();
                case HttpStatusCode.NotFound:
                    return new NotFoundObjectResult(value);
                case HttpStatusCode.Conflict:
                    return new ConflictObjectResult(value);
                case HttpStatusCode.InternalServerError:
                    //return InternalServerError(value);
                    break;
                default:
                    break;
            }
            return null;
        }

        private static string[] GetExclude(ActionExecutingContext context)
        {
            if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            {
                foreach (ParameterDescriptor parameterDescriptor in controllerActionDescriptor.Parameters)
                {
                    if (parameterDescriptor is ControllerParameterDescriptor controllerParameterDescriptor)
                    {
                        ParameterInfo parameterInfo = controllerParameterDescriptor.ParameterInfo;
                        IdempotentExcludeAttribute? attr = parameterInfo.GetCustomAttribute<IdempotentExcludeAttribute>();
                        if (attr != null)
                        {
                            return attr.Exclude;
                        }
                    }
                }
            }
            return [];
        }

        private IdempotentParameter GetIdempotentParameter(ActionExecutingContext context)
        {
            IdempotentParamRequest[] parameters = [.. GetParamRequest(context)];

            IdempotentParameter idempotentParameter = new()
            {
                ContentType = context.HttpContext.Request.ContentType?.ToString() ?? MediaTypeNames.Application.Json,
                ExpirationFromMinutes = (attribute.ExpirationFromMinutes > 0) ? attribute.ExpirationFromMinutes : options.Value.ExpirationFromMinutes,
                Attributes = parameters
            };
            return idempotentParameter;
        }

        private static IEnumerable<IdempotentParamRequest> GetParamRequest(ActionExecutingContext context)
        {
            foreach (KeyValuePair<string, object?> argument in context.ActionArguments)
            {
                yield return new()
                {
                    Name = argument.Key,
                    Excludes = GetExclude(context),
                    Type = argument.Value?.GetType() ?? typeof(object)
                };
            }
        }
    }
}
