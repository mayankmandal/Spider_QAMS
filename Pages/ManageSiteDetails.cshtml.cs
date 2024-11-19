using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Spider_QAMS.Pages
{
    public class ManageSiteDetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        
        public ManageSiteDetailsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public List<SiteDetail> Sites { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await GetAllSitesDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Error occurred while loading profile data.");
            }
        }

        private async Task GetAllSitesDataAsync()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));

            var responseSites = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllSiteDetails");
            Sites = string.IsNullOrEmpty(responseSites) ? new List<SiteDetail>() : JsonSerializer.Deserialize<List<SiteDetail>>(responseSites, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            TempData["error"] = $"Error Message - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }
    }
}
