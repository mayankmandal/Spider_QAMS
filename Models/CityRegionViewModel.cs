namespace Spider_QAMS.Models
{
    public class CityRegionViewModel
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
    }
    public class RegionAssociatedCities
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public List<CityViewModel> Cities { get; set; } = new List<CityViewModel>();
    }
    public class CityViewModel
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
    }
    public class SponsorGroup
    {
        public int SponsorId { get; set; }
        public string SponsorName { get; set; }
        public List<RegionGroup> Regions { get; set; } = new List<RegionGroup>();
        public List<SiteTypeGroup> SiteTypes { get; set; } = new List<SiteTypeGroup>(); // Added for site types
        public List<Contact> ContactsList { get; set; } = new List<Contact>();
    }
    public class RegionGroup
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public List<CityGroup> Cities { get; set; } = new List<CityGroup>();
    }

    public class CityGroup
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        public List<SiteLocation> Locations { get; set; } = new List<SiteLocation>();
    }
    public class SiteTypeGroup
    {
        public int SiteTypeId { get; set; }
        public string SiteTypeDescription { get; set; }
        public List<BranchType> BranchTypes { get; set; } = new List<BranchType>();
    }
    public class BranchTypeGroup
    {
        public int BranchTypeId { get; set; }
        public string? Description { get; set; }
    }
}
