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

            await _applicationUserBusinessLogic.SignOutAsync();

            await ManageUserClaimsAndPermissions(user);

            return RedirectToPage("/Dashboard");
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
