using Spider_QAMS.Models.ViewModels;

namespace Spider_QAMS.Models
{
    public class ProfilePagesAccessDTO
    {
        public ProfileSite ProfileData { get; set; }
        public List<PageSiteVM> PagesList { get; set; }
    }
}
