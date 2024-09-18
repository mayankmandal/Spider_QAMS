using Spider_QAMS.Models.IModels;

namespace Spider_QAMS.Models
{
    public class PageCategory: IAudittable
    {
        public int PageCatId { get; set; }
        public string CategoryName { get; set; }
        // Navigation property for Pages
        public int PageId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateUserId { get; set; }
        public DateTime UpdateDate { get; set; }
        public int UpdateUserId { get; set; }
    }
}
