using Spider_QAMS.Models.IModels;

namespace Spider_QAMS.Models
{
    public class ProfileUserAPIVM: IAudittable
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string EmailID { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePictureFile { get; set; }
        public string ProfilePictureName { get; set; }
        public ProfileSite ProfileSiteData { get; set; }
        public string Designation { get; set; }
        public string Location { get; set; }
        public bool IsADUser { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateUserId { get; set; }
    }
}
