namespace Spider_QAMS.Models
{
    public class SiteType
    {
        public int SiteTypeID { get; set; }
        public string Description { get; set; }
        public Sponsor sponsor { get; set; }
    }
}
