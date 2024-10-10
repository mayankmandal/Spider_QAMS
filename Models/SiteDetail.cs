namespace Spider_QAMS.Models
{
    public class SiteDetail
    {
        public long SiteID { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string SiteCategory { get; set; }
        public int SponsorID { get; set; }
        public int RegionID { get; set; }
        public int CityID { get; set; }
        public int LocationID { get; set; }
        public int ContactID { get; set; }
        public int SiteTypeID { get; set; }
        public string GPSLong { get; set; }
        public string GPSLatt { get; set; }
        public int? VisitUserID { get; set; }
        public DateTime? VisitedDate { get; set; }
        public int? ApprovedUserID { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int? VisitStatusID { get; set; }
        public bool IsActive { get; set; }
        public string BranchNo { get; set; }
        public int? BranchTypeId { get; set; }
        public string? AtmClass { get; set; }

        // Navigation Properties
        public List<SitePictures> SitePicturesLst { get; set; }
        public SiteContactInformation ContactInformation { get; set; }
        public GeographicalDetails GeographicalDetails { get; set; }
        public SiteBranchFacilities BranchFacilities { get; set; }
        public SiteDataCenter DataCenter { get; set; }
        public SignBoardType SignBoard { get; set; }
        public SiteMiscInformation MiscSiteInfo { get; set; }
        public BranchMiscInformation MiscBranchInfo { get; set; }
    }
}
