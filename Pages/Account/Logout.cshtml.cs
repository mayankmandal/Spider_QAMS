using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Controllers;

namespace Spider_QAMS.Pages.Account
{
    [Authorize(Policy = "PageAccess")]
    public class LogoutModel : PageModel
    {
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        public LogoutModel(ApplicationUserBusinessLogic applicationUserBusinessLogic)
        {
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }
        public async Task <IActionResult> OnPostAsync()
        {
            // Call your custom SignOut method
            var result = await _applicationUserBusinessLogic.SignOutAsync();

            if(result.Succeeded)
            {
                // Clear the session and redirect to login
                HttpContext.Session.Clear();
                TempData["success"] = $"User logged out Successfully";
                return RedirectToPage("/Account/Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to log out.");
                TempData["error"] = $"Failed to log out.";
                return Page();
            }
        }
    }
}
