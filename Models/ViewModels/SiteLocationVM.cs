using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteLocationVM
    {
        [DisplayName("Location")]
        public int LocationId { get; set; }
        [DisplayName("Location Name")]
        [Required(ErrorMessage = "Location is required")]
        public string? Location { get; set; }
        [DisplayName("Street Name")]
        public string? StreetName { get; set; }
        [DisplayName("City")]
        [Required(ErrorMessage = "City is required")]
        public int CityId { get; set; }
        [DisplayName("City Name")]
        public string? CityName { get; set; }
        [DisplayName("Region")]
        [Required(ErrorMessage = "Region is required")]
        public int RegionId { get; set; }
        [DisplayName("Region Name")]
        public string? RegionName { get; set; }
        [DisplayName("District Name")]
        public string? DistrictName { get; set; }
        [DisplayName("Branch name")]
        [StringLength(50, ErrorMessage = "District Name must be 50 characters or fewer")]
        public string? BranchName { get; set; }
        [DisplayName("Sponsor")]
        [Required(ErrorMessage = "Sponsor is required")]
        public int SponsorId { get; set; }
        [DisplayName("Sponsor Name")]
        public string? SponsorName { get; set; }
    }
}
