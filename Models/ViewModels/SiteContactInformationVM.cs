using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteContactInformationVM
    {
        [DisplayName("Branch Telephone Number")]
        public string? BranchTelephoneNumber { get; set; }

        [DisplayName("Branch Fax Number")]
        public string? BranchFaxNumber { get; set; }

        [DisplayName("Email Address")]
        [EmailAddress]
        public string? EmailAddress { get; set; }
    }
}
