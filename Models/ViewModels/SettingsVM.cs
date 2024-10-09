using Spider_QAMS.Utilities.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SettingsVM
    {
        [DisplayName("User Id")]
        public int SettingUserId { get; set; }
        [DisplayName("Full Name")]
        [StringLength(200, ErrorMessage = "Full Name must be 200 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z\s\-]*$", ErrorMessage = "Full Name must contain only alphabets, spaces and dashes")]
        public string? SettingFullName { get; set; }
        [DisplayName("Username")]
        [StringLength(100, ErrorMessage = "Username must be 100 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z0-9._ ]*$", ErrorMessage = "Username must contain only alphabets, numbers, spaces, period and underscores")]
        [CheckUniquenessinDB("UserName")]
        public string? SettingUserName { get; set; }
        [DisplayName("Email Address")]
        [StringLength(100, ErrorMessage = "Email Address must be 100 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [CheckUniquenessinDB("EmailID")]
        public string? SettingEmailID { get; set; }
        [DisplayName("User Photo")]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
        [MaxFileSize(20 * 1024, ErrorMessage = "Image size cannot exceed 20 KB")]
        public IFormFile? SettingPhotoFile { get; set; }

        [DisplayName("New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Password must be at least 8 characters and at most 16 characters long")]
        public string? Password { get; set; }
        [DisplayName("ReType New Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Password must be at least 8 characters and at most 16 characters long")]
        [Compare("Password", ErrorMessage = "New Password and ReType New Password do not match.")]
        public string? ReTypePassword { get; set; }
    }
}
