using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Spider_QAMS.Controllers;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Skeleton;
using System.Text.Encodings.Web;
using System.Text;

namespace Spider_QAMS.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;

        public RegisterModel(IConfiguration configuration, ApplicationUserBusinessLogic applicationUserBusinessLogic, IEmailService emailService)
        {
            _configuration = configuration;
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
            _emailService = emailService;
        }
        [BindProperty]
        public RegisterViewModel RegisterVM { get; set; }
        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = new ApplicationUser
            {
                EmailID = RegisterVM.Email,
                FullName = RegisterVM.FullName,
            };

            var actualUser = await _applicationUserBusinessLogic.RegisterUserAsync(user, RegisterVM.Password);
            if (actualUser != null)
            {
                var confirmationToken = _applicationUserBusinessLogic.GenerateEmailConfirmationToken(actualUser);

                var callbackUrl = Url.Page(
                    pageName: "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = actualUser.UserId, token = confirmationToken },
                    protocol: Request.Scheme
                    );

                await _emailService.SendAsync(_configuration["EmailServiceSender"], user.EmailID, "Please confirm your email", $"Please click on this link to confirm your email address: <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' target='_blank'>clicking here</a>.");

                return RedirectToPage("/Account/Login");
            }
            return Page();
        }
    }
}
