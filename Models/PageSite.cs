namespace Spider_QAMS.Models
{
    public class PageSite
    {
        public int PageId { get; set; }
        public string PageUrl { get; set; }
        public string PageDesc { get; set; }
        public int? PageSeq { get; set; }
        public int? PageCatId { get; set; }
        public string PageImgUrl { get; set; }
        public string PageName { get; set; }
        public bool isSelected { get; set; }
    }
}
