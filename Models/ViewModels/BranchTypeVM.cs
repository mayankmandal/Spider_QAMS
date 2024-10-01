using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class BranchTypeVM
    {
        [DisplayName("Branch Type Id")]
        public int BranchTypeId { get; set; }

        [DisplayName("Description")]
        [StringLength(50)]
        public string? Description { get; set; }

        [DisplayName("Sponsor ID")]
        public int? SponsorID { get; set; }

        [DisplayName("Site Type ID")]
        public int? SiteTypeID { get; set; }
    }
}
