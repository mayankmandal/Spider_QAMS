using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteDataCenterVM
    {
        [DisplayName("UPS Brand")]
        public string? UPSBrand { get; set; }

        [DisplayName("UPS Capacity")]
        public string? UPSCapacity { get; set; }

        [DisplayName("PABX Brand")]
        public string? PABXBrand { get; set; }

        [DisplayName("Stabilizer Brand")]
        public string? StabilizerBrand { get; set; }

        [DisplayName("Stabilizer Capacity")]
        public string? StabilizerCapacity { get; set; }

        [DisplayName("Security Access System Brand")]
        public string? SecurityAccessSystemBrand { get; set; }
    }
}
