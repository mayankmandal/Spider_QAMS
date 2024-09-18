using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Skeleton;

namespace Spider_QAMS.Pages.Users
{
    [Authorize(Policy = "PageAccess")]
    public class DeleteUserModel : PageModel
    {
        private readonly INavigationRepository _navigationRepository;
        public DeleteUserModel(INavigationRepository navigationRepository)
        {
            _navigationRepository = navigationRepository;
        }
        [BindProperty]
        public ProfileUserVM User { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            /*User = await _userService.GetUserByIdAsync(id);
            if (User == null)
            {
                return NotFound();
            }*/

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            // await _userService.DeleteUserAsync(User.UserId);
            return RedirectToPage("ManageUsers");
        }
    }
}
