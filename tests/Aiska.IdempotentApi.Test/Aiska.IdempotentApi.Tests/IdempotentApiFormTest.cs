using System.Net;

namespace Aiska.IdempotentApi.Tests
{
    [TestClass]
    public class IdempotentApiFormTest
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
        public async Task AppIdempotentApiFormBadRequestTest()
        {

            var formData = new Dictionary<string, string>{
                { "Id", "1" },
                { "Title", "Walk the dog" }
            };

            HttpContent httpContent = new FormUrlEncodedContent(formData);
            var response = await _client.PostAsync("/todos/form", httpContent);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task AppIdempotentApiFormSuccessTest()
        {

            var formData = new Dictionary<string, string>{
                { "Id", "1" },
                { "Title", "Walk the dog" }
            };

            HttpContent httpContent = new FormUrlEncodedContent(formData);
            httpContent.Headers.Add("Idempotency-Key",Guid.NewGuid().ToString());
            
            var response = await _client.PostAsync("/todos/form", httpContent);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task AppIdempotentApiRetriedTest()
        {
            var key = Guid.NewGuid().ToString();

            var formData = new Dictionary<string, string>{
                { "Id", "1" },
                { "Title", "Walk the dog" }
            };

            HttpContent httpContent = new FormUrlEncodedContent(formData);
            httpContent.Headers.Add("Idempotency-Key", key);

            var task = _client.PostAsync("/todos/form", httpContent);

            await Task.Delay(1000);

            var response = await _client.PostAsync("/todos/form", httpContent);

            await task.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }
        [TestMethod]
        public async Task AppIdempotentApiReusedTest()
        {
            var key = Guid.NewGuid().ToString();

            var formData = new Dictionary<string, string>{
                { "Id", "1" },
                { "Title", "Walk the dog" }
            };
            var formData2 = new Dictionary<string, string>{
                { "Id", "2" },
                { "Title", "Walk the dog" }
            };

            HttpContent httpContent = new FormUrlEncodedContent(formData);
            httpContent.Headers.Add("Idempotency-Key", key);

            var task = _client.PostAsync("/todos/form", httpContent);

            await Task.Delay(1000);

            HttpContent httpContent2 = new FormUrlEncodedContent(formData2);
            httpContent2.Headers.Add("Idempotency-Key", key);
            var response = await _client.PostAsync("/todos/form", httpContent2);

            await task.ConfigureAwait(false);

            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }


        [TestMethod]
        public async Task AppIdempotentApiFormCachedTest()
        {
            var key = Guid.NewGuid().ToString();

            var formData = new Dictionary<string, string>{
                { "Id", "1" },
                { "Title", "Walk the dog" }
            };

            HttpContent httpContent = new FormUrlEncodedContent(formData);
            httpContent.Headers.Add("Idempotency-Key", key);

            var response = await _client.PostAsync("/todos/form", httpContent);
            response = await _client.PostAsync("/todos/form", httpContent);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }
    }
}
