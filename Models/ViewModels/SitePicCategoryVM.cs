using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SitePicCategoryVM
    {
        [DisplayName("Pic Category ID")]
        public int PicCatID { get; set; }

        [DisplayName("Site Type ID")]
        public int SiteTypeID { get; set; }

        [DisplayName("Sponsor ID")]
        public int? SponsorID { get; set; }

        [DisplayName("Description")]
        public string? Description { get; set; }
    }
}
