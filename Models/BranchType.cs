namespace Spider_QAMS.Models
{
    public class BranchType
    {
        public int BranchTypeId { get; set; }
        public string? Description { get; set; }
        public SiteType siteType { get; set; }
    }
}
