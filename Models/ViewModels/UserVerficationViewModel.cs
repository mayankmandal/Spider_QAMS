using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class UserVerficationViewModel
    {
        [DisplayName("User ID")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Designation is required")]
        [Display(Name = "Designation", Description = "Designation must contain only alphabets and spaces, must be 100 characters or fewer", Prompt = "Enter Designation")]
        [StringLength(100, ErrorMessage = "Full Name must be 200 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Designation must contain only alphabets and spaces")]
        // [CheckUniquenessinDB("IdNumber")]
        public string Designation { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name", Description = "Full Name must contain only alphabets and spaces, must be 200 characters or fewer", Prompt = "Enter Full Name")]
        [StringLength(200, ErrorMessage = "Full Name must be 200 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Full Name must contain only alphabets and spaces")]
        public string FullName { get; set; }

        [Display(Name = "Email Address", Description = "Email Address must be 100 characters or fewer", Prompt = "Enter Email Address")]
        [StringLength(100, ErrorMessage = "Email Address must be 100 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile Number is required")]
        [Display(Name = "Mobile Number", Description = "Mobile Number must be a 10-digit number", Prompt = "Enter Mobile Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile Number must be a 10-digit number")]
        // [CheckUniquenessinDB("MobileNo")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username", Description = "Username must contain only alphabets, numbers, spaces, period and underscores, must be 100 characters or fewer", Prompt = "Enter Unique Username")]
        [StringLength(100, ErrorMessage = "Username must be 100 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z0-9._ ]*$", ErrorMessage = "Username must contain only alphabets, numbers, spaces, period and underscores")]
        // [CheckUniquenessinDB("Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please upload a profile picture.")]
        [Display(Name = "User Photo", Description = "Image size cannot exceed 20 KB", Prompt = "Upload User Photo")]
        // [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
        // [MaxFileSize(20 * 1024, ErrorMessage = "Image size cannot exceed 20 KB")]
        public IFormFile PhotoFile { get; set; }
        public int CreateUserId { get; set; }
        public int UpdateUserId { get; set; }
    }
}
