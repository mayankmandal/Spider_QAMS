using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models.ViewModels;
using System.Text.Json;

namespace Spider_QAMS.Pages.Users
{
    [Authorize(Policy = "PageAccess")]
    public class ManageUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        [BindProperty]
        public List<ProfileUserVM> Users { get; set; }
        [BindProperty]
        public string SearchTerm { get; set; }
        public ManageUserModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Users = await GetAllUsersDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Error occurred while loading profile data.");
            }
        }

        private async Task<List<ProfileUserVM>> GetAllUsersDataAsync()
        {
            var client = _clientFactory.CreateClient();
            // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllUsers");
            var usersData = JsonSerializer.Deserialize<List<ProfileUserVM>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return usersData;
        }
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            TempData["error"] = $"Error Message - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }
    }
}
