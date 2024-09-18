using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spider_QAMS.Pages
{
    [Authorize(Policy = "PageAccess")]
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
        {
            TempData["Email"] = HttpContext.Session.GetString("Email");
            return Page();
        }
    }
}
