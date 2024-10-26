using Spider_QAMS.Utilities.ValidationAttributes;

namespace Spider_QAMS.Models.ViewModels
{
    public class SitePicturesVM
    {
        public int SitePicID { get; set; }
        public string? Description { get; set; }
        public string? PicPath { get; set; }
        public SitePicCategoryVM SitePicCategoryVMData { get; set; }
    }
}
