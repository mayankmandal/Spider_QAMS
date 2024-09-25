namespace Spider_QAMS.Models
{
    public class SiteLocation
    {
        public int LocationId { get; set; }
        public string Location { get; set; }
        public string StreetName { get; set; }
        public City City { get; set; }
        public string DistrictName { get; set; }
        public string BranchName { get; set; }
        public int SponsorId { get; set; }
    }
}
