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
}
