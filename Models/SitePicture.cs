namespace Spider_QAMS.Models
{
    public class SitePicture
    {
        public int SitePicID { get; set; }
        public SiteDetail siteDetail { get; set; }
        public SitePicture sitePicture { get; set; }
        public string? Description { get; set; }
        public string? PicPath { get; set; }
    }
}
