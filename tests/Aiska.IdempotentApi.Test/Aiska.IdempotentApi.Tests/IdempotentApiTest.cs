namespace Aiska.IdempotentApi.Tests
{
    [TestClass]
    public class IdempotentApiTest
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
        public async Task AppDefaultGetTest()
        {
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
            // Act
            var response = await _client.GetAsync("/todos/1");
            response.EnsureSuccessStatusCode();

            // Assert
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.IsNotNull(responseString);
        }
    }
}
