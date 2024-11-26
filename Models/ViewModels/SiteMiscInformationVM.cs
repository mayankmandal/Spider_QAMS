using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteMiscInformationVM
    {
        [DisplayName("Type of ATM Location")]
        public string? TypeOfATMLocation { get; set; }

        [DisplayName("Number of External Cameras")]
        [Range(0, 1000)]
        public int? NoOfExternalCameras { get; set; }

        [DisplayName("Number of Internal Cameras")]
        [Range(0, 1000)]
        public int? NoOfInternalCameras { get; set; }

        [DisplayName("Tracking System")]
        public string? TrackingSystem { get; set; }
    }
}
