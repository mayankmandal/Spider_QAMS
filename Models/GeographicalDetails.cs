namespace Spider_QAMS.Models
{
    public class GeographicalDetails
    {
        public long SiteID { get; set; }
        public string? NearestLandmark { get; set; }
        public string? NumberOfKmNearestCity { get; set; }
        public string? BranchConstructionType { get; set; }
        public string? BranchIsLocatedAt { get; set; }
        public string? HowToReachThere { get; set; }
        public bool SiteIsOnServiceRoad { get; set; }
        public string? HowToGetThere { get; set; }
    }
}
