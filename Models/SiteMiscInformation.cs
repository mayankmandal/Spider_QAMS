namespace Spider_QAMS.Models
{
    public class SiteMiscInformation
    {
        public long SiteID { get; set; }
        public string? TypeOfATMLocation { get; set; }
        public int? NoOfExternalCameras { get; set; }
        public int? NoOfInternalCameras { get; set; }
        public string? TrackingSystem { get; set; }
    }
}
