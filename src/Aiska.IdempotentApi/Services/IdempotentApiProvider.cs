using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Configuration;
using Aiska.IdempotentApi.Extensions;
using Aiska.IdempotentApi.Models;
using Aiska.IdempotentApi.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Aiska.IdempotentApi.Services
{
    public sealed class IdempotentApiProvider(IIdempotentCache cache,
        IOptions<IdempotentApiOptions> options,
        ILogger<IdempotentApiProvider> logger,
        IOptions<JsonOptions> jsonOptions
        ) : IIdempotentApiProvider
    {

        public async ValueTask<(IdempotentEnumResult, string, object?)> ProcessIdempotentAsync(EndpointFilterInvocationContext context)
        {
            if (!IsValidIdempotent(context.HttpContext.Request))
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Request is not valid for idempotency processing.");
                }
                return (IdempotentEnumResult.Continue, string.Empty, new { });
            }

            context.HttpContext.Request.Headers.TryGetValue(options.Value.KeyHeaderName, out var headerKey);

            string IdempotencyKey = headerKey.FirstOrDefault() ?? string.Empty;
            if (IdempotencyKey == string.Empty)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Idempotency-Key header is missing.");
                }
                return (IdempotentEnumResult.HeaderMissing, string.Empty, new { });
            }

            string requestHeader = GetIdempotentHeader(context);
            string requestData = await GetIdempotentContent(context);
            var hashValue = GetHashContentSHA256(string.Format("{0},{1},{2}", IdempotencyKey, requestHeader, requestData));

            if (!cache.TryGetValue(IdempotencyKey, out IdempotentData? cacheData))
            {
                using var accessLock = await AccessLock.CreateAsync(IdempotencyKey);
                if (!cache.TryGetValue(IdempotencyKey, out cacheData))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.LogInformation("Idempotency-Key: {IdempotencyKey} - Cache miss, proceeding to execute action.", IdempotencyKey);
                    }
                    cacheData = new IdempotentData
                    {
                        HashValue = hashValue
                    };
                    cache.CreateEntry(IdempotencyKey);
                    cache.Set(IdempotencyKey, cacheData);
                    return (IdempotentEnumResult.Success, IdempotencyKey, new { });
                }
            }

            if (cacheData is not null && cacheData?.HashValue != hashValue)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Idempotency-Key: {IdempotencyKey} - Reuse detected with different request data.", IdempotencyKey);
                }
                return (IdempotentEnumResult.Reuse, string.Empty, new { });
            }
            else if (cacheData?.ResponseCache is null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Idempotency-Key: {IdempotencyKey} - Retried request detected.", IdempotencyKey);
                }
                return (IdempotentEnumResult.Retried, string.Empty, new { });
            }
            else if (cacheData?.ResponseCache is not null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Idempotency-Key: {IdempotencyKey}, with same requestData: {RequestData} detected.", IdempotencyKey, requestData);
                }
                return (IdempotentEnumResult.Idempotent, string.Empty, cacheData.ResponseCache);
            }
            return (IdempotentEnumResult.Continue, string.Empty, new { });
        }

        private string GetIdempotentHeader(EndpointFilterInvocationContext context)
        {
            Dictionary<string, object?> list = [];
            if (context == null) return string.Empty;
            foreach (var header in context.HttpContext.Request.Headers)
            {
                bool include = options.Value.IncludeHeaders.Any(item => item.Contains(header.Key, StringComparison.OrdinalIgnoreCase));
                if (include)
                {
                    if ((header.Value.FirstOrDefault() ?? string.Empty) == string.Empty) continue;
                    list.Add(header.Key, header.Value.FirstOrDefault() ?? string.Empty);
                }
            }
            return JsonSerializer.Serialize(list, jsonOptions.Value.SerializerOptions);
        }

        private string GetHashContentSHA256(string input)
        {
            // === Allocate all necessary buffers on the stack using stackalloc ===

            // Determine the maximum byte count needed for the UTF-8 input string
            int maxInputByteCount = Encoding.UTF8.GetMaxByteCount(input.Length);

            // Allocate spans for input bytes, hash bytes (32 bytes), and the final Base64 chars
            Span<byte> inputBytes = stackalloc byte[maxInputByteCount];
            Span<byte> hashBytes = stackalloc byte[SHA256.HashSizeInBytes]; // HashSizeInBytes is 32

            // Calculate the exact size needed for the Base64 output string (no padding required)
            int base64CharCount = ((hashBytes.Length + 2) / 3) * 4;
            Span<char> base64Chars = stackalloc char[base64CharCount];

            // === Process data without creating intermediate strings on the heap ===

            // 1. Get UTF-8 bytes directly into the stack-allocated inputBytes span
            int writtenBytes = Encoding.UTF8.GetBytes(input, inputBytes);

            // 2. Slice the input span to the actual length written
            ReadOnlySpan<byte> actualInputSpan = inputBytes.Slice(0, writtenBytes);

            // 3. Compute the hash directly into the stack-allocated hashBytes span
            SHA256.HashData(actualInputSpan, hashBytes);

            // 4. Convert the hash bytes into the stack-allocated characters span using TryToBase64Chars
            Convert.TryToBase64Chars(hashBytes, base64Chars, out _);

            // 5. Create the final result string *once* from the stack-allocated char span
            string finalHashString = new string(base64Chars);

            // Optional: Output the result if running in a console app
            // Console.WriteLine($"High Performance SHA256 Hash (Base64): {finalHashString}");

            return finalHashString;
        }

        private async ValueTask<string> GetIdempotentContent(EndpointFilterInvocationContext context)
        {
            if (context.HttpContext.Request.HasJsonContentType())
            {
                foreach (var argument in context.Arguments)
                {
                    return JsonSerializer.Serialize(argument, jsonOptions.Value.SerializerOptions);
                }
            }
            else if (context.HttpContext.Request.HasFormContentType)
            {
                Dictionary<string, object?> dict = [];
                foreach (var item in context.HttpContext.Request.Form)
                {
                    dict.Add(item.Key, item.Value.FirstOrDefault() ?? null);
                }
                //var forms = context.HttpContext.Request.Form.Select(t => new KeyValuePair<string,object?>(t.Key, t.Value.FirstOrDefault() ?? string.Empty)).ToList();
                return JsonSerializer.Serialize(dict, jsonOptions.Value.SerializerOptions);
            }
            return string.Empty;
        }

        public bool IsValidIdempotent(HttpRequest request)
        {
            // If Cache is not configured
            if (cache == null)
            {
                throw new Exception("An cache is not configured.");
            }

            if (request.Method != HttpMethods.Post && request.Method != HttpMethods.Patch)
                return false;
            return true;
        }

        public IEnumerable<object?> getArguments(EndpointFilterInvocationContext context)
        {
            foreach (object? data in context.Arguments)
            {
                if (data != null)
                {
                    yield return data;
                }
            }
        }

        public IdempotentErrorMessage? GetError(IdempotentEnumResult error)
        {
            return error switch
            {
                IdempotentEnumResult.HeaderMissing => options.Value.Errors.Where(e => e.Key == IdempotentError.MissingHeader).Select(v => v.Value).FirstOrDefault(),
                IdempotentEnumResult.Reuse => options.Value.Errors.Where(e => e.Key == IdempotentError.Reuse).Select(v => v.Value).FirstOrDefault(),
                IdempotentEnumResult.Retried => options.Value.Errors.Where(e => e.Key == IdempotentError.Retried).Select(v => v.Value).FirstOrDefault(),
                _ => null
            };
        }

        public async Task CacheAsync(string cacheKey, object? result)
        {
            if (cache.TryGetValue(cacheKey, out IdempotentData? cacheData))
            {
                if (cacheData is not null)
                {
                    cacheData.ResponseCache = result;
                    cache.Set(cacheKey, cacheData);
                }
            }
        }
    }
}
