using Spider_QAMS.Utilities.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Spider_QAMS.Models.ViewModels
{
    public class SitePicCategoryVM
    {
        public int? PicCatID { get; set; }
        public string? Description { get; set; }
    }

    public class SiteImageUploaderVM
    {
        [Required(ErrorMessage = "Site ID is required.")]
        public long SiteId { get; set; }
        public List<SitePicCategoryViewModel> SitePicCategoryList { get; set; } = new List<SitePicCategoryViewModel>();
    }

    public class SitePicCategoryViewModel
    {
        public int PicCatId { get; set; }
        public string Description { get; set; }
        [Required]
        public List<ImageViewModel> Images { get; set; } = new List<ImageViewModel>();
    }

    public class ImageViewModel
    {
        public int? SitePicID { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        [FileExtensions(Extensions = "jpg,jpeg,png,gif", ErrorMessage = "Only image files are allowed.")]
        [MaxFileSize(2 * 1024 * 1024)] // 2 MB max
        public IFormFile ImageFile { get; set; }
        public bool IsDeleted { get; set; }
        public string FileDescription { get; set; }
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
        public List<string> ImagePaths { get; set; } = new List<string>();
        public List<string> ImageComments { get; set; } = new List<string>();
        public List<bool> ImageDeletes { get; set; } = new List<bool>();
        // Helper method to ensure ImagesPath and ImageComments are in sync with the uploaded images
        public void InitializeImageProperties(int imageCount)
        {
            // Ensure the lists match the number of images with default values
            ImagePaths = Enumerable.Repeat(string.Empty, imageCount).ToList();
            ImageComments = Enumerable.Repeat(string.Empty,imageCount).ToList();
            ImageDeletes = Enumerable.Repeat(false,imageCount).ToList();
        }
    }
}
