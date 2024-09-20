using Spider_QAMS.Repositories.Skeleton;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Spider_QAMS.Repositories.Domain
{
    public class UniquenessCheckService : IUniquenessCheckService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string JwtToken { get; private set; }
        public UniquenessCheckService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> IsUniqueAsync(string field, string value)
        {
            JwtToken = JWTCookieHelper.GetJWTCookie(_httpContextAccessor.HttpContext);
            var client = _httpClientFactory.CreateClient();

            if (!string.IsNullOrEmpty(JwtToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
            }

            var requestPayload = new UniquenessCheckRequest { Field = field, Value = value };
            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_configuration["ApiBaseUrl"]}/Navigation/CheckUniqueness", content);

            if (!response.IsSuccessStatusCode)
            {
                // Handle unsuccessful response or log error
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result == null ? false : result.IsUnique;
        }
        private class ApiResponse
        {
            public bool IsUnique { get; set; }
        }
    }
    public class UniquenessCheckRequest
    {
        public string Field { get; set; }
        public string Value { get; set; }
    }
}
