using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Spider_QAMS.Utilities.ValidationAttributes;

namespace Spider_QAMS.Models.ViewModels
{
    public class UpdateProfileUserVM
    {
        [DisplayName("User ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [DisplayName("Designation")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Designation must contain only alphabets")]
        public string? Designation { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [DisplayName("Full Name")]
        [StringLength(200, ErrorMessage = "Full Name must be 200 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z\s\-]*$", ErrorMessage = "Full Name must contain only alphabets, spaces and dashes")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [DisplayName("Email Address")]
        [StringLength(100, ErrorMessage = "Email Address must be 100 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [CheckUniquenessinDB("emailid")]
        public string? EmailID { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [DisplayName("Mobile Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile Number must be a 10-digit number")]
        [CheckUniquenessinDB("phonenumber")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Profile Data is required")]
        public ProfileSiteVM ProfileSiteData { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [DisplayName("Username")]
        [StringLength(100, ErrorMessage = "Username must be 100 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z0-9._ ]*$", ErrorMessage = "Username must contain only alphabets, numbers, spaces, period and underscores")]
        [CheckUniquenessinDB("username")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Please upload a profile picture.")]
        [DisplayName("User Photo")]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
        [MaxFileSize(20 * 1024, ErrorMessage = "Image size cannot exceed 20 KB")]
        public IFormFile? PhotoFile { get; set; }

        [DisplayName("New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Password must be at least 8 characters and at most 16 characters long")]
        public string? Password { get; set; }
        [DisplayName("ReType New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Password must be at least 8 characters and at most 16 characters long")]
        [Compare("Password", ErrorMessage = "New Password and ReType New Password do not match.")]
        public string? ReTypePassword { get; set; }

        [Required(ErrorMessage = "Is Active is required")]
        [DisplayName("Is Active User")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Is Active Directory User is required")]
        [DisplayName("Is Active Directory User")]
        public bool IsADUser { get; set; }

        public int CreateUserId { get; set; }

        public int UpdateUserId { get; set; }
    }
}
