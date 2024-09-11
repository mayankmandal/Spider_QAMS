using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class CredentialViewModel
    {
        [Required]
        [DisplayName("Email Address")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DisplayName("Remember Me")]
        public bool RememberMe { get; set; }
    }
}
