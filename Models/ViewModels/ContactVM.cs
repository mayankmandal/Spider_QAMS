using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class ContactVM
    {
        [DisplayName("Contact Id")]
        public int ContactId { get; set; }
        [DisplayName("Name")]
        [Required(ErrorMessage = "Name is required")]
        [StringLength(200, ErrorMessage = "Name must be 200 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Name can only contain letters, numbers, and spaces.")]
        public string Name { get; set; }
        [DisplayName("Designation")]
        [Required(ErrorMessage = "Designation is required")]
        [StringLength(50, ErrorMessage = "Designation must be 50 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Designation can only contain letters, numbers, and spaces.")]
        public string Designation { get; set; }
        [DisplayName("Office Phone Number")]
        [Required(ErrorMessage = "Office Phone is required")]
        [StringLength(15, ErrorMessage = "Office Phone must be 15 characters or fewer")]
        [RegularExpression(@"^[+0-9 ]*$", ErrorMessage = "Office Phone Number can only contain plus, numbers and spaces.")]
        public string OfficePhone { get; set; }
        [DisplayName("Mobile Number")]
        [Required(ErrorMessage = "Mobile Number is required")]
        [StringLength(15, ErrorMessage = "Mobile Number must be 15 characters or fewer")]
        [RegularExpression(@"^[+0-9 ]*$", ErrorMessage = "Mobile Number can only contain plus, numbers and spaces.")]
        public string Mobile { get; set; }
        [DisplayName("Email Address")]
        [Required(ErrorMessage = "Email Address is required")]
        [StringLength(50, ErrorMessage = "Email Address must be 50 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailID { get; set; }
        [DisplayName("Fax Number")]
        [Required(ErrorMessage = "Fax Number is required")]
        [StringLength(15, ErrorMessage = "Fax Number must be 15 characters or fewer")]
        [RegularExpression(@"^[+0-9 ]*$", ErrorMessage = "Fax Number can only contain plus, numbers and spaces.")]
        public string Fax { get; set; }
        [DisplayName("Branch Name")]
        [Required(ErrorMessage = "Branch is required")]
        [StringLength(50, ErrorMessage = "Branch Name must be 50 characters or fewer")]
        [RegularExpression(@"^[a-zA-Z0-9 ]*$", ErrorMessage = "Branch Name can only contain letters, numbers, and spaces.")]
        public string BranchName { get; set; }
        [DisplayName("Sponsor Name")]
        [Required(ErrorMessage = "Sponsor is required")]
        public int SponsorId { get; set; }
        public string? SponsorName { get; set; }
    }
}
