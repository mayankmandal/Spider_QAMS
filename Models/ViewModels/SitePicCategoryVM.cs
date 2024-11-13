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
        public List<ImageViewModel> Images { get; set; } = new List<ImageViewModel>();
    }

    public class ImageViewModel
    {
        public int? SitePicID { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        [Base64ImageValidation(2 * 1024 * 1024, "jpg,jpeg,png", ErrorMessage = "Invalid image file (only jpg, jpeg, png allowed and max size 2MB).")]
        public string? ImageFile { get; set; }
        public bool? IsDeleted { get; set; }
        public string? FileDescription { get; set; }
    }
}
