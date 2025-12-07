extern alias SampleMinimalApi;

using Bogus;
using SampleMinimalApi::Aiska.IdempotentApi.SampleMinimalApi;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Aiska.IdempotentApi.Tests
{
    [TestClass]
    public class IdempotentApiJsonTest
    {
        private HttpClient? _client;
        private CustomWebApplicationFactory<SampleMinimalApi.Program>? _factory;

        private bool isGenerated;
        private Todo[] todos = [];


        public class Todo
        {
            public int Id { get; set; }
            public string? Title { get; set; }
            public DateOnly? DueBy { get; set; }
            public bool IsComplete { get; set; }
        }

        public Todo[] SampleData
        {
            get
            {
                if (!isGenerated)
                {
                    isGenerated = true;
                    var userFaker = new Faker<Todo>()
                        .RuleFor(u => u.IsComplete, f => true)
                        .RuleFor(u => u.DueBy, f => f.Date.FutureDateOnly())
                        .RuleFor(u => u.Title, f => f.Name.FullName())
                        .RuleFor(u => u.Id, f => f.UniqueIndex);

                    todos = userFaker.Generate(100).ToArray();
                }

                return todos;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<SampleMinimalApi.Program>();
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

            int count = 2;

            var key = Guid.NewGuid().ToString();

            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[count];

            for (int i = 0; i < count; i++)
            {
                HttpContent httpContent = new StringContent(
                    content: JsonSerializer.Serialize(SampleData[i]),
                    encoding: Encoding.UTF8,
                    mediaType: MediaTypeNames.Application.Json
                );
                httpContent.Headers.Add("Idempotency-Key", key);
                tasks[i] = _client.PostAsync("/todos", httpContent);
            }

            Task.WaitAll(tasks, TestContext.CancellationToken);

            int failCount = tasks.Where(t => t.Result.StatusCode == HttpStatusCode.UnprocessableEntity).Count();
            Assert.AreEqual(count-1, failCount);
            int successCount = tasks.Where(t => t.Result.StatusCode == HttpStatusCode.Created).Count();
            Assert.AreEqual(1, successCount);
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
