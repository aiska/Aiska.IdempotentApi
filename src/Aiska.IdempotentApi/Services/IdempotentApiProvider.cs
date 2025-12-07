using Aiska.IdempotentApi.Abtractions;
using Aiska.IdempotentApi.Logging;
using Aiska.IdempotentApi.Models;
using Aiska.IdempotentApi.Options;
using Aiska.IdempotentApi.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Aiska.IdempotentApi.Services
{
    internal sealed class IdempotentApiProvider(IIdempotentCache cache,
            IOptions<IdempotentApiOptions> options,
            ILogger<IdempotentApiProvider> logger) : IIdempotentApiProvider
    {

        async Task<IdempotentResponse> IIdempotentApiProvider.ProcessIdempotentAsync(IdempotentRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.IdempotentHeaderKey == string.Empty)
            {
                logger.MissingIdempotencyKeyHeader();
                return new()
                {
                    Type = IdempotentEnumResult.HeaderMissing
                };
            }

            string hashValue = GetHashContentSHA256(request.IdempotentHeaderKey, request.RequestData);

            IdempotentCacheData cacheData;
            using (AccessLock accessLock = await AccessLock.CreateAsync(request.IdempotentHeaderKey))
            {
                cacheData = await cache.GetOrCreateAsync(request.IdempotentHeaderKey);
                if (cacheData.HashValue == null)
                {
                    logger.IdempotencyKeyCacheMiss(request.IdempotentHeaderKey.SanitizeInput());
                    cacheData.HashValue = hashValue;
                    await cache.SetCacheAsync(request.IdempotentHeaderKey, cacheData);
                    return new()
                    {
                        HashValue = hashValue,
                        Type = IdempotentEnumResult.CacheMiss
                    };
                }
            }


            if (cacheData.HashValue is not null)
            {
                if (hashValue.Equals(cacheData.HashValue, StringComparison.Ordinal))
                {
                    if (cacheData.IsCompleted)
                    {
                        logger.IdempotencyKeyCacheHit(request.IdempotentHeaderKey.SanitizeInput());
                        return new()
                        {
                            HashValue = hashValue,
                            statusCode = cacheData.StatusCode,
                            ResponseCache = cacheData.ResponseCache,
                            Type = IdempotentEnumResult.CacheHit
                        };
                    }
                    else
                    {
                        logger.IdempotencyKeyRetried(request.IdempotentHeaderKey.SanitizeInput());
                        return new()
                        {
                            Type = IdempotentEnumResult.Retried
                        };
                    }
                }
                else
                {
                    logger.IdempotencyKeyReuse(request.IdempotentHeaderKey.SanitizeInput());
                    return new()
                    {
                        ResponseCache = cacheData.ResponseCache,
                        Type = IdempotentEnumResult.Reuse
                    };
                }

            }
            return new()
            {
                Type = IdempotentEnumResult.Continue
            };
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
            int base64CharCount = (hashBytes.Length + 2) / 3 * 4;
            Span<char> base64Chars = stackalloc char[base64CharCount];

            // === Process data without creating intermediate strings on the heap ===

            // 1. Get UTF-8 bytes directly into the stack-allocated inputBytes span
            int writtenBytes = Encoding.UTF8.GetBytes(input, inputBytes);

            // 2. Slice the input span to the actual length written
            ReadOnlySpan<byte> actualInputSpan = inputBytes[..writtenBytes];

            // 3. Compute the hash directly into the stack-allocated hashBytes span
            SHA256.HashData(actualInputSpan, hashBytes);

            // 4. Convert the hash bytes into the stack-allocated characters span using TryToBase64Chars
            Convert.TryToBase64Chars(hashBytes, base64Chars, out _);

            // 5. Create the final result string *once* from the stack-allocated char span
            string finalHashString = new(base64Chars);

            // Optional: Output the result if running in a console app
            // Console.WriteLine($"High Performance SHA256 Hash (Base64): {finalHashString}");

            return finalHashString;
        }

        bool IIdempotentApiProvider.IsValidIdempotent(HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(cache);

            return request.Method == HttpMethods.Post || request.Method == HttpMethods.Patch;
        }

        async Task IIdempotentApiProvider.SetCacheAsync(string key, IdempotentCacheData data)
        {
            await cache.SetCacheAsync(key, data);
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
