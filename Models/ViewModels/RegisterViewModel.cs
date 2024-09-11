using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name", Description = "Full Name must contain only alphabets and spaces", Prompt = "Enter Full Name")]
        [StringLength(200, ErrorMessage = "Full Name must be 200 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Full Name must contain only alphabets and spaces")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-Mail Address is required")]
        [Display(Name = "E-Mail Address", Description = "Valid E-Mail Address must be 100 characters or fewer", Prompt = "Enter E-Mail Address")]
        [StringLength(100, ErrorMessage = "E-Mail Address must be 100 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid E-Mail Address")]
        // [CheckUniquenessinDB("Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "ReType E-Mail Address is required")]
        [Display(Name = "ReType E-Mail Address", Description = "Valid ReType E-Mail Address must be 100 characters or fewer", Prompt = "Enter ReType E-Mail Address")]
        [StringLength(100, ErrorMessage = "E-Mail Address must be 100 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid ReType E-Mail Address")]
        [Compare("Email", ErrorMessage = "E-Mail Address and ReType E-Mail Address do not match.")]
        public string ReTypeEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DisplayName("Password")]
        [Display(Name = "Password", Description = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Password must be at least 8 characters and at most 16 characters long", Prompt = "Enter Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Password must be at least 8 characters and at most 16 characters long")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ReType Password is required")]
        [Display(Name = "ReType Password", Description = "Password and ReType Password must match.", Prompt = "Enter ReType Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Password must be at least 8 characters and at most 16 characters long")]
        [Compare("Password", ErrorMessage = "Password and ReType Password do not match.")]
        public string ReTypePassword { get; set; }
    }
}
