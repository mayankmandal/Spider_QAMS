using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spider_QAMS.Pages.Account
{
    [Authorize(Policy = "PageAccess")]
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            HttpContext.Session.Remove("Email");
            return RedirectToPage("Login");
        }
    }
}
