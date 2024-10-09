namespace Spider_QAMS.Models
{
    public class SiteDataCenter
    {
        public long SiteID { get; set; }
        public string? UPSBrand { get; set; }
        public string? UPSCapacity { get; set; }
        public string? PABXBrand { get; set; }
        public string? StabilizerBrand { get; set; }
        public string? StabilizerCapacity { get; set; }
        public string? SecurityAccessSystemBrand { get; set; }
    }
}
