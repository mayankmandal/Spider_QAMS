using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models
{
    public class SitePictures
    {
        public int SitePicID { get; set; }
        public long SiteID { get; set; }
        public string? Description { get; set; }
        public string? PicPath { get; set; }
        public SitePicCategory SitePicCategoryData { get; set; }
    }
}
