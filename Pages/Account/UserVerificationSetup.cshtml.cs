using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Controllers;
using Spider_QAMS.Models.ViewModels;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Spider_QAMS.Pages.Account
{
    public class UserVerificationSetupModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        public UserVerificationSetupModel(IConfiguration configuration, IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment, ApplicationUserBusinessLogic applicationUserBusinessLogic)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
        }
        [BindProperty]
        public UserVerficationViewModel UserVerficationData { get; set; }
        public string UserProfilePathUrl = string.Empty;
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var user = await _applicationUserBusinessLogic.GetCurrentUserAsync();
                if (user == null || user.EmailConfirmed == false)
                {
                    TempData["error"] = $"Invalid attempt. Please check your input and try again.";
                    return RedirectToPage("/Account/Login");
                }
                if (user.UserVerificationSetupEnabled == true)
                {
                    return RedirectToPage("/Account/UserRoleAssignment");
                }
                if (user != null)
                {
                    UserVerficationData = new UserVerficationViewModel()
                    {
                        UserId = user.UserId,
                        Designation = user.Designation,
                        Email = user.EmailID,
                        FullName = user.FullName,
                        MobileNo = user.PhoneNumber != null ? user.PhoneNumber : string.Empty,
                        Username = user.UserName != null ? user.UserName : string.Empty,
                        PhotoFile = null // Initially set to null for GET request
                    };
                    var userProfileImgPath = _configuration["UserProfileImgPath"];
                    var defaultUserImgPath = _configuration["DefaultUserImgPath"];

                    UserProfilePathUrl = !string.IsNullOrEmpty(user.ProfilePicName) ? $"/{Path.Combine(userProfileImgPath, user.ProfilePicName).Replace("\\", "/")}" : defaultUserImgPath;
                    return Page();
                }
                else
                {
                    return RedirectToPage("/Account/AccessDenied");
                }

            }
            catch (Exception ex)
            {
                return HandleError(ex, "Error occurred while loading profile data.");
            }
        }
        public async Task<IActionResult> OnPost()
        {
            bool isProfilePhotoReUpload = true;

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Model State Validation Failed.";
                UserProfilePathUrl = _configuration["DefaultUserImgPath"];
                if (isProfilePhotoReUpload)
                {
                    ModelState.AddModelError("UserVerficationData.PhotoFile", "Please upload profile picture again.");
                }
                return Page();
            }

            try
            {
                var user = await _applicationUserBusinessLogic.GetCurrentUserAsync();
                if (user == null || user.EmailConfirmed == false)
                {
                    TempData["error"] = $"Invalid attempt. Please check your input and try again.";
                    return RedirectToPage("/Account/Login");
                }
                if (user.UserVerificationSetupEnabled == true)
                {
                    return RedirectToPage("/Account/UserRoleAssignment");
                }

                string uniqueFileName = null;
                string filePath = null;
                string uploadFolder = null;
                if (UserVerficationData.PhotoFile != null)
                {
                    uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"]);
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + UserVerficationData.PhotoFile.FileName;
                    filePath = Path.Combine(uploadFolder, uniqueFileName);

                    // FileStream is properly disposed of after use
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await UserVerficationData.PhotoFile.CopyToAsync(fileStream);
                    }
                    isProfilePhotoReUpload = false;
                }

                UserVerifyApiVM profileUserAPIVM = new UserVerifyApiVM
                {
                    UserId = UserVerficationData.UserId,
                    Designation = UserVerficationData.Designation,
                    EmailID = UserVerficationData.Email,
                    PhoneNumber = UserVerficationData.MobileNo,
                    FullName = UserVerficationData.FullName,
                    UserName = UserVerficationData.Username,
                    ProfilePicName = uniqueFileName,
                    CreateUserId = UserVerficationData.CreateUserId,
                    UpdateUserId = UserVerficationData.UpdateUserId
                };

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateUserVerification";
                var jsonContent = JsonSerializer.Serialize(profileUserAPIVM);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    _applicationUserBusinessLogic.RefreshCurrentUserAsync();
                    TempData["success"] = $"{UserVerficationData.FullName} - Profile Created Successfully";
                    return RedirectToPage("/Account/UserRoleAssignment");
                }
                else
                {
                    TempData["error"] = $"{UserVerficationData.FullName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
            TempData["error"] = $"{UserVerficationData.FullName} - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }
    }
}
