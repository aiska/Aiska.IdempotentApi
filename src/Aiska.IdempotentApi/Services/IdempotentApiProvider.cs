using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Configuration;
using Aiska.IdempotentApi.Extensions;
using Aiska.IdempotentApi.Logging;
using Aiska.IdempotentApi.Models;
using Aiska.IdempotentApi.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Aiska.IdempotentApi.Services
{
    internal sealed class IdempotentApiProvider(IIdempotentCache cache,
            IOptions<IdempotentApiOptions> options,
            ILogger<IdempotentApiProvider> logger,
            IOptions<JsonOptions> jsonOptions) : IIdempotentApiProvider
    {

        async Task<IdempotentEnumResult> IIdempotentApiProvider.ProcessIdempotentAsync(IdempotentRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.IdempotentHeader == string.Empty)
            {
                logger.MissingIdempotencyKeyHeader();
                return IdempotentEnumResult.HeaderMissing;
            }

            request.RequestData = await GetIdempotentContent(request);
            request.HashValue = GetHashContentSHA256(request.IdempotentHeader, request.RequestData);

            if (!cache.TryGetValue(request.IdempotentHeader, out IdempotentData? cacheData))
            {
                using var accessLock = await AccessLock.CreateAsync(request.IdempotentHeader);
                if (!cache.TryGetValue(request.IdempotentHeader, out cacheData))
                {
                    if (logger.IsEnabled(LogLevel.Information))
                    {
                        logger.IdempotencyKeyCacheMiss(request.IdempotentHeader.SanitizeInput());
                    }
                    cacheData = new IdempotentData
                    {
                        HashValue = GetHashContentSHA256(request.IdempotentHeader, request.RequestData)
                    };
                    cache.CreateEntry(request.IdempotentHeader);
                    cache.SetCache(request.IdempotentHeader, cacheData);
                    return IdempotentEnumResult.Success;
                }
            }
            request.CacheData = cacheData;


            if (request.CacheData is not null && request.CacheData?.HashValue != request.HashValue)
            {
                logger.IdempotencyKeyReuse(request.IdempotentHeader.SanitizeInput());
                return IdempotentEnumResult.Reuse;
            }
            else if (request?.CacheData?.ResponseCache is null)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.IdempotencyKeyRetried(request?.IdempotentHeader.SanitizeInput() ?? string.Empty);
                }
                return IdempotentEnumResult.Retried;
            }
            else if (request.CacheData?.ResponseCache is not null)
            {
                logger.IdempotencyKeyCacheHit(request.IdempotentHeader.SanitizeInput());
                return IdempotentEnumResult.Idempotent;
            }
            return IdempotentEnumResult.Continue;
        }

        private string GetHashContentSHA256(string IdempotencyKey, string requestData)
        {
            string input = IdempotencyKey + requestData;

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

        private async ValueTask<string> GetIdempotentContent(IdempotentRequest request)
        {
            if (request != null && request.Parameters.Count > 0)
            {
                if (request.ContentType.StartsWith(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var parameter in request.Parameters)
                    {
                        string json = JsonSerializer.Serialize(parameter.Value, parameter.Type, jsonOptions.Value.SerializerOptions);
                        if (parameter.Excludes.Length > 0)
                        {
                            JsonObject? jsonObject = JsonObject.Parse(json)?.AsObject();
                            if (jsonObject != null)
                            {
                                foreach (var item in parameter.Excludes)
                                {
                                    jsonObject.Remove(item);
                                    string lower = item.FirstCharToLower();
                                    jsonObject.Remove(lower);
                                }
                                return jsonObject.ToJsonString();
                            }
                        }
                        return json;
                    }
                }
                else if (request.ContentType.StartsWith(MediaTypeNames.Application.FormUrlEncoded, StringComparison.OrdinalIgnoreCase))
                {
                    Dictionary<string, object?> data = [];
                    foreach (var item in request.Parameters)
                    {
                        if (item.Value != null && !item.Excludes.Contains(item.Name, StringComparer.OrdinalIgnoreCase))
                        {
                            data.Add(item.Name, item.Value);
                        }
                    }
                    return JsonSerializer.Serialize(data, jsonOptions.Value.SerializerOptions);
                }
            }
            return string.Empty;
        }

        bool IIdempotentApiProvider.IsValidIdempotent(HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(cache);

            if (request.Method != HttpMethods.Post && request.Method != HttpMethods.Patch)
                return false;
            return true;
        }

        async Task IIdempotentApiProvider.CacheAsync(string cacheKey, object? result)
        {
            if (cache.TryGetValue(cacheKey, out IdempotentData? cacheData))
            {
                if (cacheData is not null)
                {
                    cacheData.ResponseCache = result;
                    cache.SetCache(cacheKey, cacheData);
                }
            }
        }

        IdempotentErrorMessage IIdempotentApiProvider.MissingHeaderError()
        {
            return new IdempotentErrorMessage(
                options.Value.Errors.MissingHeaderType,
                options.Value.Errors.MissingHeaderTitle,
                options.Value.Errors.MissingHeaderDetail
            );
        }
        IdempotentErrorMessage IIdempotentApiProvider.RetriedError()
        {
            return new IdempotentErrorMessage(
                options.Value.Errors.RetriedType,
                options.Value.Errors.RetriedTitle,
                options.Value.Errors.RetriedDetail
            );
        }
        IdempotentErrorMessage IIdempotentApiProvider.ReuseError()
        {
            return new IdempotentErrorMessage(
                options.Value.Errors.ReuseType,
                options.Value.Errors.ReuseTitle,
                options.Value.Errors.ReuseDetail);
        }
    }
}
