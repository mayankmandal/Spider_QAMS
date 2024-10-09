namespace Spider_QAMS.Models.ViewModels
{
    public class SitePicCategoryVM
    {
        public int? PicCatID { get; set; }
        public string? Description { get; set; }
    }
    public class SitePicCategoryVMAssociation
    {
        public int? PicCatID { get; set; }
        public string? Description { get; set; }
        public List<SitePicturesAssociation> Images { get; set; } = new List<SitePicturesAssociation>();
    }
}
