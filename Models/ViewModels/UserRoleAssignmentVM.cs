using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class UserRoleAssignmentVM
    {
        [Required(ErrorMessage = "Full Name is required")]
        [DisplayName("Full Name")]
        [StringLength(200, ErrorMessage = "Full Name must be 200 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Full Name must contain only alphabets and spaces")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [DisplayName("Email Address")]
        [StringLength(100, ErrorMessage = "Email Address must be 100 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        // [CheckUniquenessinDB("Email")]
        public string EmailId { get; set; }
        [Required(ErrorMessage = "Profile Data is required")]
        public ProfileSiteVM ProfileSiteData { get; set; }
    }
}
