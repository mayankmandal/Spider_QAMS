using Spider_QAMS.Utilities.ValidationAttributes;

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
        [CompositeImageValidation(
        new string[] { ".jpg", ".png", ".jpeg" },
        1024 * 1024,  // Max size: 1 MB
        10,            // Max image count: 10
        ErrorMessage = "Invalid images uploaded.")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public List<string> ImagesPath { get; set; } = new List<string>();
        public List<string> ImageComments { get; set; } = new List<string>();
    }
}
