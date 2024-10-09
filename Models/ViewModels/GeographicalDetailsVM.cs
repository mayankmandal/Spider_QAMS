using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class GeographicalDetailsVM
    {
        [DisplayName("Nearest Landmark")]
        public string? NearestLandmark { get; set; } 

        [DisplayName("Number of Km to Nearest City")]
        public string? NumberOfKmNearestCity { get; set; } 

        [DisplayName("Branch Construction Type")]
        public string? BranchConstructionType { get; set; } 

        [DisplayName("Branch Is Located At")]
        public string? BranchIsLocatedAt { get; set; } 

        [DisplayName("How To Reach There")]
        public string? HowToReachThere { get; set; } 

        [DisplayName("Site is on Service Road")]
        public bool SiteIsOnServiceRoad { get; set; } = false;

        [DisplayName("How to Get There")]
        public string? HowToGetThere { get; set; } 
    }
}
