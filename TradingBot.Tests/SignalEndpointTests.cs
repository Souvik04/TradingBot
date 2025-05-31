using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;

namespace TradingBot.Api.Tests
{
    public class SignalEndpointTests : IClassFixture<WebApplicationFactory<TradingBot.Api.Program>>
    {
        private readonly HttpClient _client;

        public SignalEndpointTests(WebApplicationFactory<TradingBot.Api.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostSignal_Returns200AndExpectedStructure()
        {
            // Arrange
            var request = new
            {
                symbol = "AAPL"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/signal", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseString);

            // Check that "signal" property exists in response
            json.ContainsKey("signal").Should().BeTrue();

            // Optionally, check for other properties
            // json.ContainsKey("target").Should().BeTrue();
            // json.ContainsKey("stopLoss").Should().BeTrue();
        }
    }
}