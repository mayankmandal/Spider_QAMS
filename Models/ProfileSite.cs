using Spider_QAMS.Models.IModels;

namespace Spider_QAMS.Models
{
    public class ProfileSite : IAudittable
    {
        public int ProfileId { get; set; }
        public string ProfileName { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateUserId { get; set; }
    }
}
