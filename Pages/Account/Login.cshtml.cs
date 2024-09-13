using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Controllers;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Utilities;
using System.Security.Claims;

namespace Spider_QAMS.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        public LoginModel(ApplicationUserBusinessLogic applicationUserBusinessLogic)
        {
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
        }

        [BindProperty]
        public CredentialViewModel CredentialData { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Invalid login attempt. Please check your input and try again.";
                return Page();
            }
            var user = await _applicationUserBusinessLogic.AuthenticateUserAsync(CredentialData.Email, CredentialData.Password);
            if(user == null)
            {
                TempData["error"] = "Invalid login attempt. Please check your input and try again.";
                return Page();
            }

            if (user.EmailConfirmed == false)
            {
                TempData["error"] = $"Email is not confirmed for {user.EmailID}. Please confirm your email.";
                ModelState.AddModelError("Login", "You must have a confirmed email to log in.");
                return Page();
            }

            await _applicationUserBusinessLogic.SignOutAsync();

            await ManageUserClaimsAndPermissions(user);

            if (user.UserVerificationSetupEnabled == null || user.UserVerificationSetupEnabled == true)
            {
                TempData["success"] = "Please complete your user verification setup.";
                return RedirectToPage("/Account/UserVerificationSetup");
            }
            else if (user.UserVerificationSetupEnabled == true && user.RoleAssignmentEnabled == false)
            {
                TempData["success"] = "Firstly User need to be assigned with Appropriate Role for further accessibility";
                return RedirectToPage("/Account/UserRoleAssignment");
            }
            else if (user.UserVerificationSetupEnabled == true && user.RoleAssignmentEnabled == true)
            {
                TempData["success"] = "Login successful.";
                return RedirectToPage("/Dashboard");
            }
            return Page();
        }
        private async Task ManageUserClaimsAndPermissions(ApplicationUser user)
        {
            // Role and claim management
            var claims = await _applicationUserBusinessLogic.GetCurrentUserClaimsAsync(user);
            var accessToken = _applicationUserBusinessLogic.GenerateJSONWebToken(claims);
            _applicationUserBusinessLogic.SetJWTCookie(accessToken, Constants.JwtCookieName);

            // Fetch and cache user permissions from API
            // await _currentUserService.FetchAndCacheUserPermissions(accessToken);
        }
    }
}
