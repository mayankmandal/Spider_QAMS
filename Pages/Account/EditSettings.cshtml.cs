using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text;
using Spider_QAMS.Models.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Spider_QAMS.Pages.Account
{
    public class EditSettingsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private ProfileUserAPIVM _userSettings;
        public EditSettingsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
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
                UserProfilePathUrl = Path.Combine(_configuration["BaseUrl"], _configuration["UserProfileImgPath"], _userSettings.ProfilePicName ?? string.Empty);
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            bool isProfilePhotoReUpload = true;
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
                UserProfilePathUrl = Path.Combine(_configuration["UserProfileImgPath"], _userSettings.ProfilePicName);
                if (isProfilePhotoReUpload)
                {
                    ModelState.AddModelError("SettingsData.SettingPhotoFile", "Please upload profile picture again.");
                }
                return Page();
            }

            string uniqueFileName = null;
            string filePath = null;
            string uploadFolder = null;

            if (SettingsData.SettingPhotoFile != null)
            {
                uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"]);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + SettingsData.SettingPhotoFile.FileName;
                filePath = Path.Combine(uploadFolder, uniqueFileName);

                // FileStream is properly disposed of after use
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await SettingsData.SettingPhotoFile.CopyToAsync(fileStream);
                }
            }

            SettingsAPIVM userSettings = new SettingsAPIVM
            {
                SettingsUserId = SettingsData.SettingUserId,
                SettingsFullName = SettingsData.SettingFullName,
                SettingsEmailID = SettingsData.SettingEmailID,
                SettingsProfilePicName = SettingsData.SettingPhotoFile != null ? uniqueFileName : "",
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
                TempData["success"] = $"{SettingsData.SettingFullName} - Profile Updated Successfully";
                TempData.Remove("UserSettings");
                return RedirectToPage();
            }
            else
            {
                // Delete the uploaded image if the update fails
                if (SettingsData.SettingPhotoFile != null && !string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                TempData["error"] = $"{SettingsData.SettingFullName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
                TempData["UserSettings"] = JsonSerializer.Serialize(_userSettings);
                UserProfilePathUrl = Path.Combine(_configuration["UserProfileImgPath"], _userSettings.ProfilePicName);
                return Page();
            }
        }
    }
}
