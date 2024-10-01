using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteDetailVM
    {
        [DisplayName("Site ID")]
        public long SiteID { get; set; }

        [DisplayName("Site Code")]
        [Required(ErrorMessage = "Site Code is required")]
        public string SiteCode { get; set; }

        [DisplayName("Site Name")]
        [Required(ErrorMessage = "Site Name is required")]
        public string SiteName { get; set; }

        [DisplayName("Site Category")]
        public string? SiteCategory { get; set; }

        [DisplayName("Sponsor ID")]
        public int? SponsorID { get; set; }

        [DisplayName("Region ID")]
        public int? RegionID { get; set; }

        [DisplayName("City ID")]
        public int? CityID { get; set; }

        [DisplayName("Location ID")]
        public int? LocationID { get; set; }

        [DisplayName("Contact ID")]
        public int? ContactID { get; set; }

        [DisplayName("Site Type ID")]
        public int? SiteTypeID { get; set; }

        [DisplayName("GPS Longitude")]
        public string? GPSLong { get; set; }

        [DisplayName("GPS Latitude")]
        public string? GPSLatt { get; set; }

        [DisplayName("Visit User ID")]
        public int? VisitUserID { get; set; }

        [DisplayName("Visited Date")]
        public DateTime? VisitedDate { get; set; }

        [DisplayName("Approved User ID")]
        public int? ApprovedUserID { get; set; }

        [DisplayName("Approval Date")]
        public DateTime? ApprovalDate { get; set; }

        [DisplayName("Visit Status ID")]
        public int? VisitStatusID { get; set; }

        [DisplayName("Is Active")]
        public bool? IsActive { get; set; }

        [DisplayName("Branch No")]
        public string? BranchNo { get; set; }

        [DisplayName("Branch Type ID")]
        public int? BranchTypeId { get; set; }

        [DisplayName("ATM Class")]
        public char? AtmClass { get; set; }
    }
}
