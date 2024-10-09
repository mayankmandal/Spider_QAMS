using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteBranchFacilitiesVM
    {
        [DisplayName("Parking Available")]
        public bool Parking { get; set; } = false;

        [DisplayName("Landscape Available")]
        public bool Landscape { get; set; } = false;

        [DisplayName("Elevator Available")]
        public bool Elevator { get; set; } = false;

        [DisplayName("VIP Section Available")]
        public bool VIPSection { get; set; } = false;

        [DisplayName("Safe Box Available")]
        public bool SafeBox { get; set; } = false;

        [DisplayName("ICAP Available")]
        public bool ICAP { get; set; } = false;
    }
}
