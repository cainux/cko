using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bogus;
using PG.WebApi.Tests.Sandbox.Requests;

namespace PG.WebApi.Tests.Sandbox
{
    public abstract class TestBase
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        protected readonly Faker<ProcessPaymentRequest> RequestGenerator;
        protected readonly HttpClient HttpClient;

        protected TestBase(TestFixture fixture)
        {
            _jsonSerializerOptions = fixture.JsonSerializerOptions;
            RequestGenerator = fixture.RequestGenerator;
            HttpClient = fixture.HttpClient;
        }

        protected T DeserializeJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }

        protected async Task<T> DeserializeJsonAsync<T>(Stream utf8Json)
        {
            return await JsonSerializer.DeserializeAsync<T>(utf8Json, _jsonSerializerOptions);
        }
    }
}
