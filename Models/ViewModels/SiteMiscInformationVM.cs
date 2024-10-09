using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteMiscInformationVM
    {
        [DisplayName("Type of ATM Location")]
        public string? TypeOfATMLocation { get; set; }

        [DisplayName("Number of External Cameras")]
        public int? NoOfExternalCameras { get; set; }

        [DisplayName("Number of Internal Cameras")]
        public int? NoOfInternalCameras { get; set; }

        [DisplayName("Tracking System")]
        public string? TrackingSystem { get; set; }
    }
}
