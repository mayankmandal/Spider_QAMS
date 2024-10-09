using Microsoft.AspNetCore.Mvc.Rendering;
using Spider_QAMS.Utilities.ValidationAttributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models.ViewModels
{
    public class SiteDetailVM
    {
        [DisplayName("Site ID")]
        public long SiteID { get; set; }

        [DisplayName("Site Code")]
        [CheckUniquenessinDB("SiteCode")]
        [Required(ErrorMessage = "Site Code is required")]
        public string SiteCode { get; set; }

        [DisplayName("Site Name")]
        [CheckUniquenessinDB("SiteName")]
        [Required(ErrorMessage = "Site Name is required")]
        public string SiteName { get; set; }

        [DisplayName("Site Category")]
        public string SiteCategory { get; set; }

        [DisplayName("Sponsor ID")]
        public int SponsorID { get; set; }

        [DisplayName("Region ID")]
        public int RegionID { get; set; }

        [DisplayName("City ID")]
        public int CityID { get; set; }

        [DisplayName("Location ID")]
        public int LocationID { get; set; }

        [DisplayName("Contact ID")]
        public int ContactID { get; set; }

        [DisplayName("Site Type ID")]
        public int SiteTypeID { get; set; }

        [DisplayName("GPS Longitude")]
        [CheckUniquenessinDB("GPSLong", "GPSLatt")]
        public string GPSLong { get; set; }

        [DisplayName("GPS Latitude")]
        [CheckUniquenessinDB("GPSLatt", "GPSLong")]
        public string GPSLatt { get; set; }

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
        public bool IsActive { get; set; } = false;

        [DisplayName("Branch Number")]
        public string BranchNo { get; set; }

        [DisplayName("Branch Type")]
        public int? BranchTypeId { get; set; }

        [DisplayName("ATM Class")]
        public string? AtmClass { get; set; }

        // Referencing Other View Models
        public List<SitePicturesVM> SitePicturesLst { get; set; }
        public SiteContactInformationVM SiteContactInformation { get; set; }
        public GeographicalDetailsVM GeographicalDetails { get; set; }
        public SiteBranchFacilitiesVM SiteBranchFacilities { get; set; }
        public SiteDataCenterVM SiteDataCenter { get; set; }
        public SignBoardTypeVM SignBoardType { get; set; }
        public SiteMiscInformationVM SiteMiscInformation { get; set; }
        public BranchMiscInformationVM BranchMiscInformation { get; set; }
    }
}
