using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteLocationVM
    {
        [DisplayName("Location Id")]
        public int LocationId { get; set; }
        [DisplayName("Location")]
        public string? Location { get; set; }
        [DisplayName("Street Name")]
        public string? StreetName { get; set; }
        [DisplayName("City Name")]
        [Required(ErrorMessage = "City is required")]
        public int CityId { get; set; }
        public string? CityName { get; set; }
        [DisplayName("Region Name")]
        [Required(ErrorMessage = "Region is required")]
        public int RegionId { get; set; }
        public string? RegionName { get; set; }
        [DisplayName("District Name")]
        public string? DistrictName { get; set; }
        [DisplayName("Branch name")]
        [StringLength(50, ErrorMessage = "District Name must be 500 characters or fewer")]
        public string? BranchName { get; set; }
        [DisplayName("Sponsor Id")]
        public int? SponsorId { get; set; }
    }
}
