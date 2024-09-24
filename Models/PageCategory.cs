using Spider_QAMS.Models.IModels;

namespace Spider_QAMS.Models
{
    public class PageCategory
    {
        public int PageCatId { get; set; }
        public string CategoryName { get; set; }
        // Navigation property for Pages
        public int PageId { get; set; }
    }
}
