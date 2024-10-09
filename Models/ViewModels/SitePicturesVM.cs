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
    public class SitePicturesAssociation
    {
        public int SitePicID { get; set; }
        public string? Description { get; set; }
        public string? PicPath { get; set; }
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg" })]
        [MaxFileSize(1024 * 1024, ErrorMessage = "Image size cannot exceed 1 MB")]
        public IFormFile? PicPathFile { get; set; }
    }
}
