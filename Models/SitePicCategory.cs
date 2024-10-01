namespace Spider_QAMS.Models
{
    public class SitePicCategory
    {
        public int PicCatID { get; set; }
        public SiteType siteType { get; set; }
        public Sponsor sponsor { get; set; }
        public string? Description { get; set; }
    }
}
