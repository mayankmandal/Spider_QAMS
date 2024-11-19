using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Spider_QAMS.Pages
{
    public class ViewUserProfileModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        public ViewUserProfileModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public ProfileUserAPIVM CurrentUserDetailsData { get; set; }
        public List<PageSiteVM>? CurrentPageSites { get; set; }
        public List<CategoryDisplayViewModel>? StructureData { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadCurrentProfileUserData();
                await LoadCurrentPageSites();
                await LoadCurrentCategoriesSetDTOs();

                return Page();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Error occurred while loading profile data.");
            }
        }
        private async Task LoadCurrentProfileUserData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetCurrentUserDetails");
            CurrentUserDetailsData = string.IsNullOrEmpty(response) ? new ProfileUserAPIVM() : JsonSerializer.Deserialize<ProfileUserAPIVM>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        private async Task LoadCurrentPageSites()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetCurrentUserPages");
            CurrentPageSites = string.IsNullOrEmpty(response) ? new List<PageSiteVM>() : JsonSerializer.Deserialize<List<PageSiteVM>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            CurrentPageSites = CurrentPageSites.OrderBy(page => page.PageDesc).ToList();
        }
        private async Task LoadCurrentCategoriesSetDTOs()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetCurrentUserCategories");
            StructureData = string.IsNullOrEmpty(response) ? new List<CategoryDisplayViewModel>() : JsonSerializer.Deserialize<List<CategoryDisplayViewModel>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            TempData["error"] = errorMessage + " Error details: " + ex.Message;
            return RedirectToPage("/Dashboard");
        }
    }
}
