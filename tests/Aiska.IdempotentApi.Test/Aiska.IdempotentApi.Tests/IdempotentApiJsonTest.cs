using System.Net;
using System.Net.Mime;
using System.Text;

namespace Aiska.IdempotentApi.Tests
{
    [TestClass]
    public class IdempotentApiJsonTest
    {
        private HttpClient? _client;
        private CustomWebApplicationFactory<Program>? _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [TestMethod]
        public async Task AppIdempotentApiJsonBadRequestTest()
        {
            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );

            if (_client is not null)
            {
                var response = await _client.PostAsync("/todos", httpContent, TestContext.CancellationToken);

                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            }
        }

        [TestMethod]
        public async Task AppIdempotentApiJsonSuccessTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());

            var response = await _client.PostAsync("/todos", httpContent);
            response.EnsureSuccessStatusCode();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task AppIdempotentApiRetriedTest()
        {
            ArgumentNullException.ThrowIfNull(_client);
            var key = Guid.NewGuid().ToString();

            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("Idempotency-Key", key);

            var task = _client.PostAsync("/todos", httpContent, TestContext.CancellationToken);

            await Task.Delay(1000, TestContext.CancellationToken);

            var response = await _client.PostAsync("/todos", httpContent);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

            await task;
        }

        [TestMethod]
        public async Task AppIdempotentApiReusedTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            var key = Guid.NewGuid().ToString();

            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";
            string jsonPayload2 = "{\"id\": 2, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("Idempotency-Key", key);

            var task = _client.PostAsync("/todos", httpContent);

            await Task.Delay(1000);


            HttpContent httpContent2 = new StringContent(
                content: jsonPayload2,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent2.Headers.Add("Idempotency-Key", key);

            var response = await _client.PostAsync("/todos", httpContent2);

            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }


        [TestMethod]
        public async Task AppIdempotentApiJsonCachedTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            var key = Guid.NewGuid().ToString();

            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("Idempotency-Key", key);

            var response = await _client.PostAsync("/todos/Json", httpContent, TestContext.CancellationToken);
            response = await _client.PostAsync("/todos", httpContent, TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        public TestContext TestContext { get; set; }
    }
}
