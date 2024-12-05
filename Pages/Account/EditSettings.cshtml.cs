using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text;
using Spider_QAMS.Models.ViewModels;
using System.Text.Json;
using Spider_QAMS.Repositories.Domain;
using Microsoft.AspNetCore.Authorization;

namespace Spider_QAMS.Pages.Account
{
    [Authorize(Policy = "PageAccess")]
    public class EditSettingsModel : PageModel
    {
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private ProfileUserAPIVM _userSettings;
        public EditSettingsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory, ApplicationUserBusinessLogic applicationUserBusinessLogic)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
        }
        [BindProperty]
        public SettingsVM SettingsData { get; set; } = new SettingsVM();
        public string UserProfilePathUrl = string.Empty;
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCurrentUserData();
            // Store _userSettings in TempData for subsequent requests
            TempData["UserSettings"] = JsonSerializer.Serialize(_userSettings);
            return Page();
        }
        private async Task LoadCurrentUserData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetSettingsData");
            _userSettings = string.IsNullOrEmpty(response) ? new ProfileUserAPIVM() : JsonSerializer.Deserialize<ProfileUserAPIVM>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!string.IsNullOrEmpty(_userSettings.UserName))
            {
                // Assign values from userSettings to settingsViewModel
                SettingsData = new SettingsVM
                {
                    SettingUserId = _userSettings.UserId,
                    SettingFullName = _userSettings.FullName,
                    SettingUserName = _userSettings.UserName,
                    SettingEmailID = _userSettings.EmailID,
                    SettingPhotoFile = null,
                };
                UserProfilePathUrl = Path.Combine(_configuration["BaseUrl"], _configuration["UserProfileImgPath"], _userSettings.ProfilePictureFile ?? string.Empty);
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            bool isProfilePhotoReUpload = true;
            bool isEmailChanged = true;
            // Check if _userSettings is already in TempData
            if (TempData.ContainsKey("UserSettings"))
            {
                _userSettings = JsonSerializer.Deserialize<ProfileUserAPIVM>(TempData["UserSettings"].ToString(),new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (_userSettings.UserName == SettingsData.SettingUserName)
                {
                    ModelState.Remove("SettingsData.SettingUserName");
                }

                if (_userSettings.EmailID == SettingsData.SettingEmailID)
                {
                    ModelState.Remove("SettingsData.SettingEmailID");
                    isEmailChanged = false;
                }
            }

            if (SettingsData.SettingPhotoFile == null)
            {
                ModelState.Remove("SettingsData.SettingPhotoFile");
                isProfilePhotoReUpload = false;
            }

            if (SettingsData.Password == null || SettingsData.ReTypePassword == null)
            {
                ModelState.Remove("SettingsData.Password");
                ModelState.Remove("SettingsData.ReTypePassword");
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Model State Validation Failed.";
                TempData["UserSettings"] = JsonSerializer.Serialize(_userSettings);
                UserProfilePathUrl = Path.Combine(_configuration["UserProfileImgPath"], _userSettings.ProfilePictureFile);
                if (isProfilePhotoReUpload)
                {
                    ModelState.AddModelError("SettingsData.SettingPhotoFile", "Please upload profile picture again.");
                }
                return Page();
            }
            try
            {
                string base64String = null;
                if (SettingsData.SettingPhotoFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await SettingsData.SettingPhotoFile.CopyToAsync(memoryStream);
                        base64String = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }

                SettingsAPIVM userSettings = new SettingsAPIVM
                {
                    SettingsUserId = SettingsData.SettingUserId,
                    SettingsFullName = SettingsData.SettingFullName,
                    SettingsEmailID = SettingsData.SettingEmailID,
                    SettingsProfilePictureFile = SettingsData.SettingPhotoFile != null ? base64String : "",
                    SettingsProfilePictureName = SettingsData.SettingPhotoFile != null ? SettingsData.SettingPhotoFile.FileName : "",
                    SettingsUserName = SettingsData.SettingUserName,
                    SettingsPassword = SettingsData.Password != null ? SettingsData.Password : "",
                    SettingsReTypePassword = SettingsData.ReTypePassword != null ? SettingsData.ReTypePassword : ""
                };

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateSettingsData";
                var jsonContent = JsonSerializer.Serialize(userSettings);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response;
                response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData.Remove("UserSettings");

                    if (isEmailChanged)
                    {
                        // Call your custom SignOut method
                        var result = await _applicationUserBusinessLogic.SignOutAsync();

                        if (result.Succeeded)
                        {
                            // Clear the session and redirect to login
                            HttpContext.Session.Clear();
                            TempData["success"] = $"{SettingsData.SettingFullName} - Profile Updated Successfully.\nUser logged out Successfully";
                            return RedirectToPage("/Account/Login");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Failed to log out.");
                            TempData["error"] = $"Failed to log out.";
                            return Page();
                        }
                    }

                    TempData["success"] = $"{SettingsData.SettingFullName} - Profile Updated Successfully";
                    return RedirectToPage();
                }
                else
                {
                    TempData["error"] = $"{SettingsData.SettingFullName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
                    TempData["UserSettings"] = JsonSerializer.Serialize(_userSettings);
                    UserProfilePathUrl = Path.Combine(_configuration["UserProfileImgPath"], _userSettings.ProfilePictureFile);
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
            TempData["error"] = $"{SettingsData.SettingFullName} - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }
    }
}
