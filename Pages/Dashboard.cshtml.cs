using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spider_QAMS.Pages
{
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
        {
            TempData["Email"] = HttpContext.Session.GetString("Email");
            return Page();
        }
    }
}
