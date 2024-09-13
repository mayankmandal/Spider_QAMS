using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Controllers;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Spider_QAMS.Pages.Account
{
    public class UserRoleAssignmentModel : PageModel
    {
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private ProfileUserAPIVM CurrentUserDetailsData { get; set; }
        public UserRoleAssignmentModel(ApplicationUserBusinessLogic applicationUserBusinessLogic, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        [BindProperty]
        public UserRoleAssignmentVM UserRoleAssignmentVMData { get; set; }
        public List<ProfileSiteVM>? ProfilesData { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await _applicationUserBusinessLogic.RefreshCurrentUserAsync();
                var user = await _applicationUserBusinessLogic.GetCurrentUserAsync();

                if (user == null ||
                    user.EmailConfirmed != true ||
                    user.UserVerificationSetupEnabled != true)
                {
                    TempData["error"] = $"Invalid login attempt. Please check your input and try again.";
                    return RedirectToPage("/Account/Login");
                }
                if (user.RoleAssignmentEnabled == true)
                {
                    TempData["success"] = $"{user.FullName} logged in successfully.";
                    return RedirectToPage("/Dashboard");
                }

                await LoadCurrentProfileUserData();

                UserRoleAssignmentVMData = new UserRoleAssignmentVM
                {
                    EmailId = CurrentUserDetailsData.EmailID,
                    FullName = CurrentUserDetailsData.FullName,
                    ProfileSiteData = new ProfileSiteVM
                    {
                        ProfileName = CurrentUserDetailsData.ProfileSiteData.ProfileName,
                        ProfileId = CurrentUserDetailsData.ProfileSiteData.ProfileId
                    }
                };
                return Page();

            }
            catch (Exception ex)
            {
                return HandleError(ex, "Error occurred while loading profile data.");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Re-fetch profiles data for the dropdown in case of an invalid model state
                await LoadCurrentProfileUserData();
                return Page();
            }

            try
            {
                await _applicationUserBusinessLogic.RefreshCurrentUserAsync();
                var user = await _applicationUserBusinessLogic.GetCurrentUserAsync();

                if (user == null ||
                user.EmailConfirmed != true ||
                user.UserVerificationSetupEnabled != true)
                {
                    TempData["error"] = $"Invalid login attempt. Please check your input and try again.";
                    return RedirectToPage("/Account/Login");
                }

                if (user.RoleAssignmentEnabled == true)
                {
                    _applicationUserBusinessLogic.RefreshCurrentUserAsync();
                    TempData["success"] = $"{user.FullName} logged in successfully.";
                    return RedirectToPage("/Dashboard");
                }
                TempData["error"] = $"Proper Role is not yet Assigned. Contact the administrator.";
                return Page();
            }
            catch (Exception ex)
            {
                return HandleError(ex, "Error occurred while assigning profile.");
            }
        }
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            TempData["error"] = $"{CurrentUserDetailsData.FullName} - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }

        private async Task LoadCurrentProfileUserData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetCurrentUserDetails");
            CurrentUserDetailsData = JsonSerializer.Deserialize<ProfileUserAPIVM>(response);
        }
    }
}
