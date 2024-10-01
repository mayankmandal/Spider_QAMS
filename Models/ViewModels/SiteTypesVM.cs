using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteTypesVM
    {
        [DisplayName("Site Type ID")]
        public int SiteTypeID { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Sponsor ID")]
        public int? SponsorID { get; set; }
    }
}
