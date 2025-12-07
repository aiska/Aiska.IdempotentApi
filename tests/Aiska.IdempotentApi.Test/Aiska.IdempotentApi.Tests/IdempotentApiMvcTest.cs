extern alias SampleMvc;

using Bogus;
using SampleMvc::Aiska.IdempotentApi.SampleMvc;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace Aiska.IdempotentApi.Tests
{
    [TestClass]
    public class IdempotentApiMvcTest
    {
        private bool isGenerated;
        private DataTransfer[] users = [];
        public DataTransfer[] SampleData
        {
            get
            {
                if (!isGenerated)
                {
                    isGenerated = true;
                    var userFaker = new Faker<DataTransfer>()
                        .RuleFor(u => u.IssuerName, f => f.Name.FirstName())
                        .RuleFor(u => u.AquirerName, f => f.Name.FullName())
                        .RuleFor(u => u.Money, f => f.Finance.Amount())
                        .RuleFor(u => u.Timestamp, f => f.Date.Future());

                    users = userFaker.Generate(100).ToArray();
                }

                return users;
            }
        }

        private HttpClient? _client;
        private CustomWebApplicationFactory<SampleMvc.Program>? _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = new CustomWebApplicationFactory<SampleMvc.Program>();
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
            ArgumentNullException.ThrowIfNull(_client);
            HttpContent httpContent = new StringContent(
                content: JsonSerializer.Serialize(SampleData[0]),
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );

            var response = await _client.PostAsync("/TransferMoney", httpContent, TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AppIdempotentApiJsonSuccessTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            HttpContent httpContent = new StringContent(
                content: JsonSerializer.Serialize(SampleData[0]),
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );
            httpContent.Headers.Add("X-Idempotency-Key", Guid.NewGuid().ToString());

            var response = await _client.PostAsync("/TransferMoney", httpContent);
            response.EnsureSuccessStatusCode();

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task AppIdempotentApiRetriedTest()
        {
            ArgumentNullException.ThrowIfNull(_client);
            var key = Guid.NewGuid().ToString();

            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[100];

            for (int i = 0; i < 100; i++)
            {
                HttpContent httpContent = new StringContent(
                    content: JsonSerializer.Serialize(SampleData[1]),
                    encoding: Encoding.UTF8,
                    mediaType: MediaTypeNames.Application.Json
                );
                httpContent.Headers.Add("X-Idempotency-Key", key);
                tasks[i] = _client.PostAsync("/TransferMoney", httpContent);
            }

            await Task.WhenAll(tasks);

            int successCount = tasks.Where(t => t.Result.StatusCode == HttpStatusCode.Created).Count();
            Assert.AreEqual(1, successCount);
            int failCount = tasks.Where(t => t.Result.StatusCode == HttpStatusCode.Conflict).Count();
            Assert.AreEqual(99, failCount);

        }

        [TestMethod]
        public async Task AppIdempotentApiReusedTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            var key = Guid.NewGuid().ToString();

            Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[100];

            for (int i = 0; i < 100; i++)
            {
                HttpContent httpContent = new StringContent(
                    content: JsonSerializer.Serialize(SampleData[i]),
                    encoding: Encoding.UTF8,
                    mediaType: MediaTypeNames.Application.Json
                );
                httpContent.Headers.Add("X-Idempotency-Key", key);
                tasks[i] = _client.PostAsync("/TransferMoney", httpContent);
            }

            Task.WaitAll(tasks, TestContext.CancellationToken);

            int failCount = tasks.Where(t => t.Result.StatusCode == HttpStatusCode.UnprocessableEntity).Count();
            Assert.AreEqual(99, failCount);
            int successCount = tasks.Where(t => t.Result.StatusCode == HttpStatusCode.Created).Count();
            Assert.AreEqual(1, successCount);
        }


        [TestMethod]
        public async Task AppIdempotentApiJsonCachedTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            var key = Guid.NewGuid().ToString();

            HttpContent httpContent = new StringContent(
                content: JsonSerializer.Serialize(SampleData[0]),
                encoding: Encoding.UTF8,
                mediaType: MediaTypeNames.Application.Json
            );

            httpContent.Headers.Add("X-Idempotency-Key", key);

            var response = await _client.PostAsync("/TransferMoney", httpContent, TestContext.CancellationToken);
            response = await _client.PostAsync("/TransferMoney", httpContent, TestContext.CancellationToken);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        public TestContext TestContext { get; set; }
    }
}
