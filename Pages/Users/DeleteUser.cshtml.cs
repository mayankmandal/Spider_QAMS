using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using System.Text.Json;
using System.Text;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;

namespace Spider_QAMS.Pages.Users
{
    [Authorize(Policy = "PageAccess")]
    public class DeleteUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private ProfileUserAPIVM _profileUserData { get; set; }
        public DeleteUserModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public UpdateProfileUserVM ProfileUsersData { get; set; }
        public string UserProfilePathUrl = string.Empty;

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToPage("/Error");
                }

                ProfileUsersData = await GetUserProfileDataAsync(userId);
                UserProfilePathUrl = Path.Combine(_configuration["BaseUrl"], _configuration["UserProfileImgPath"], _profileUserData.ProfilePicName ?? string.Empty);
                return Page();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Error occurred while loading profile data.");
            }
        }

        private async Task<UpdateProfileUserVM> GetUserProfileDataAsync(string userId)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchUserRecord";
            var requestBody = new UserIdRequest { UserId = userId };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _profileUserData = JsonSerializer.Deserialize<ProfileUserAPIVM>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                ProfileSiteVM profileSiteVM = new ProfileSiteVM
                {
                    ProfileId = _profileUserData.ProfileSiteData.ProfileId,
                    ProfileName = _profileUserData.ProfileSiteData.ProfileName
                };

                return new UpdateProfileUserVM
                {
                    UserId = _profileUserData.UserId,
                    UserName = _profileUserData.UserName,
                    EmailID = _profileUserData.EmailID,
                    Designation = _profileUserData.Designation,
                    PhoneNumber = _profileUserData.PhoneNumber,
                    FullName = _profileUserData.FullName,
                    IsActive = _profileUserData.IsActive,
                    IsADUser = _profileUserData.IsADUser,
                    ProfileSiteData = profileSiteVM,
                    Password = _profileUserData.Password,
                    ReTypePassword = _profileUserData.Password
                };
            }
            else
            {
                throw new Exception($"Error fetching user record: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        
        public async Task<IActionResult> OnPostAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["error"] = "User ID is required.";
                return RedirectToPage("/Error");
            }
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/DeleteEntity?deleteId={userId}&deleteType={DeleteEntityType.User}";
                HttpResponseMessage response;
                response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{DeleteEntityType.User} - {userId} deleted Successfully";
                    return RedirectToPage("/Users/ManageUser");
                }
                else
                {
                    TempData["error"] = $"{DeleteEntityType.User} - {userId} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                return HandleError(ex, "An unexpected error occurred.");
            }
        }
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            TempData["error"] = $"{ProfileUsersData.FullName} - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }
    }
}
