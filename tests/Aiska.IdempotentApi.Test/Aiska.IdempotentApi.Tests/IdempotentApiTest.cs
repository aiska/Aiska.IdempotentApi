extern alias SampleMinimalApi;

using Microsoft.AspNetCore.Http.HttpResults;
using System.Net;

namespace Aiska.IdempotentApi.Tests
{
    [TestClass]
    public class IdempotentApiTest
    {
        private HttpClient? _client;
        private CustomWebApplicationFactory<SampleMinimalApi.Program>? _factory;

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
        public async Task AppDefaultGetTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            // Act
            var response = await _client.GetAsync("/todos");
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseString);
        }

        [TestMethod]
        public async Task AppDefaultGetWithParamTest()
        {
            ArgumentNullException.ThrowIfNull(_client);

            // Act
            var response = await _client.GetAsync("/todos/1");
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseString);
        }

        [TestMethod]
        public async Task AppDefaultGetWithParamTestNotFound()
        {
            ArgumentNullException.ThrowIfNull(_client);

            // Act
            var response = await _client.GetAsync("/todos/10");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
