using System.Net;
using System.Net.Mime;
using System.Text;

namespace Aiska.IdempotentApi.Tests
{
    [TestClass]
    public class IdempotentApiJsonTest
    {
        private HttpClient _client;
        private CustomWebApplicationFactory<Program> _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client.Dispose();
            _factory.Dispose();
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

            var response = await _client.PostAsync("/todos", httpContent);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AppIdempotentApiJsonSuccessTest()
        {
            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());

            var response = await _client.PostAsync("/todos", httpContent);
            response.EnsureSuccessStatusCode();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task AppIdempotentApiRetriedTest()
        {
            var key = Guid.NewGuid().ToString();

            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("Idempotency-Key", key);

            var task = _client.PostAsync("/todos", httpContent);

            await Task.Delay(1000);

            var response = await _client.PostAsync("/todos", httpContent);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }
        [TestMethod]
        public async Task AppIdempotentApiReusedTest()
        {
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
            var key = Guid.NewGuid().ToString();

            string jsonPayload = "{\"id\": 1, \"title\": \"Buy Milk\"}";

            HttpContent httpContent = new StringContent(
                content: jsonPayload,
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("Idempotency-Key", key);

            var response = await _client.PostAsync("/todos/Json", httpContent);
            response = await _client.PostAsync("/todos", httpContent);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
