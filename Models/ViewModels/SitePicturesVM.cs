using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SitePicturesVM
    {
        [DisplayName("Site Pic ID")]
        public int SitePicID { get; set; }

        [DisplayName("Site ID")]
        public long SiteID { get; set; }

        [DisplayName("Pic Category ID")]
        public int? PicCatID { get; set; }

        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Picture Path")]
        public string? PicPath { get; set; }
    }
}
