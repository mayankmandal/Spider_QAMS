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
        public UniquenessCheckService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<bool> IsUniqueAsync(string field1, string value1, string? field2 = null, string? value2 = null)
        {
            // Prepare the HTTP client and token
            var jwtToken = JWTCookieHelper.GetJWTCookie(_httpContextAccessor.HttpContext);
            var client = _httpClientFactory.CreateClient();

            if (!string.IsNullOrEmpty(jwtToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
            // Create request payload
            var requestPayload = new UniquenessCheckRequest 
            { 
                Field1 = field1,
                Value1 = value1,
                Field2 = field2 == null ? string.Empty : field2,
                Value2 = value2 == null ? string.Empty : value2
            };
            var content = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_configuration["ApiBaseUrl"]}/Navigation/CheckUniqueness", content);

            if (!response.IsSuccessStatusCode)
            {
                // Handle unsuccessful response or log error
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
            }
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.IsUnique ?? false;
        }
        private class ApiResponse
        {
            public bool IsUnique { get; set; }
        }
    }
    public class UniquenessCheckRequest
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }
}
