using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static Spider_QAMS.Utilities.Constants;

namespace Spider_QAMS.Pages.Users
{
    [Authorize(Policy = "PageAccess")]
    public class UpdateUserModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private ProfileUserAPIVM _profileUserData { get; set; }
        public UpdateUserModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        [BindProperty]
        public UpdateProfileUserVM ProfileUsersData { get; set; }

        public List<ProfileSiteVM>? ProfilesData { get; set; }
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
                await LoadAllProfilesData();
                UserProfilePathUrl = Path.Combine( _configuration["BaseUrl"], _configuration["UserProfileImgPath"], _profileUserData.ProfilePictureFile ?? string.Empty);
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
            var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
            var requestBody = new Record { RecordId = Convert.ToInt32(userId), RecordType = (int)FetchRecordByIdOrTextEnum.GetCurrentUserDetails };
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

        private async Task LoadAllProfilesData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllProfiles");
            ProfilesData = string.IsNullOrEmpty(response) ? new List<ProfileSiteVM>() : JsonSerializer.Deserialize<List<ProfileSiteVM>>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool isProfilePhotoReUpload = true;

            if (ProfileUsersData.UserId != null)
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
                // Create a request with the user ID in the body
                var requestBody = new Record { RecordId = Convert.ToInt32(ProfileUsersData.UserId), RecordType = (int)FetchRecordByIdOrTextEnum.GetCurrentUserDetails };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _profileUserData = JsonSerializer.Deserialize<ProfileUserAPIVM>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    await LoadAllProfilesData(); // Reload ProfilesData if there's a validation error
                    ModelState.AddModelError("ProfileUsersData.UserId", $"Error fetching user record for {ProfileUsersData.UserId}. Please ensure the UserId is correct.");
                    TempData["error"] = $"Model State Validation Failed. Response status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }

                if (_profileUserData.UserName == ProfileUsersData.UserName)
                {
                    ModelState.Remove("ProfileUsersData.UserName");
                }

                if (_profileUserData.EmailID == ProfileUsersData.EmailID)
                {
                    ModelState.Remove("ProfileUsersData.EmailID");
                }

                if (_profileUserData.Designation == ProfileUsersData.Designation)
                {
                    ModelState.Remove("ProfileUsersData.Designation");
                }

                if (_profileUserData.PhoneNumber == ProfileUsersData.PhoneNumber)
                {
                    ModelState.Remove("ProfileUsersData.PhoneNumber");
                }
            }

            if (ProfileUsersData.PhotoFile == null)
            {
                ModelState.Remove("ProfileUsersData.PhotoFile");
                isProfilePhotoReUpload = false;
            }

            if (ProfileUsersData.Password == null || ProfileUsersData.ReTypePassword == null)
            {
                ModelState.Remove("ProfileUsersData.Password");
                ModelState.Remove("ProfileUsersData.ReTypePassword");
            }

            ModelState.Remove("ProfileUsersData.ProfileSiteData.ProfileName");

            if (!ModelState.IsValid)
            {
                await LoadAllProfilesData(); // Reload ProfilesData if there's a validation error
                TempData["error"] = "Model State Validation Failed.";
                UserProfilePathUrl = Path.Combine(_configuration["UserProfileImgPath"], _profileUserData.ProfilePictureFile);
                if (isProfilePhotoReUpload)
                {
                    ModelState.AddModelError("ProfileUsersData.PhotoFile", "Please upload profile picture again.");
                }
                return Page();
            }
            try
            {
                string base64String = null;
                if (ProfileUsersData.PhotoFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await ProfileUsersData.PhotoFile.CopyToAsync(memoryStream);
                        base64String = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }

                ProfileUserAPIVM profileUserAPIVM = new ProfileUserAPIVM
                {
                    UserId = ProfileUsersData.UserId,
                    Designation = ProfileUsersData.Designation,
                    EmailID = ProfileUsersData.EmailID,
                    PhoneNumber = ProfileUsersData.PhoneNumber,
                    FullName = ProfileUsersData.FullName,
                    Password = ProfileUsersData.Password ?? string.Empty,
                    UserName = ProfileUsersData.UserName,
                    ProfilePictureFile = ProfileUsersData.PhotoFile != null ? base64String : string.Empty,
                    ProfilePictureName = ProfileUsersData.PhotoFile != null ? ProfileUsersData.PhotoFile.FileName: string.Empty,
                    ProfileSiteData = new ProfileSite
                    {
                        ProfileId = ProfileUsersData.ProfileSiteData.ProfileId,
                        ProfileName = ProfileUsersData.ProfileSiteData.ProfileName,
                    },
                    IsActive = ProfileUsersData.IsActive,
                    IsADUser = ProfileUsersData.IsADUser,
                    Location = string.Empty,
                    CreateUserId = ProfileUsersData.CreateUserId,
                    UpdateUserId = ProfileUsersData.UpdateUserId,
                };

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateUser";
                var jsonContent = JsonSerializer.Serialize(profileUserAPIVM);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{ProfileUsersData.FullName} - Profile Updated Successfully";
                    return RedirectToPage("/Users/ManageUsers");
                }
                else
                {
                    await LoadAllProfilesData();
                    TempData["error"] = $"{ProfileUsersData.FullName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
                    UserProfilePathUrl = Path.Combine(_configuration["UserProfileImgPath"], _profileUserData.ProfilePictureFile);
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                return HandleError(ex, "Error occurred during HTTP request.");
            }
            catch (JsonException ex)
            {
                return HandleError(ex, "Error occurred while parsing JSON.");
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
