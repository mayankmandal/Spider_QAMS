using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Spider_QAMS.Utilities.ValidationAttributes;

namespace Spider_QAMS.Models.ViewModels
{
    public class PageCategoryVM
    {
        public int PageCatId { get; set; }
        [DisplayName("Category Name")]
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_\s]*$", ErrorMessage = "Category Name can only contain alphabets, numbers, whitespaces and underscore.")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
        [CheckUniquenessinDB("CategoryName")]
        public string CategoryName { get; set; }
        // Navigation property for Pages
        public int PageId { get; set; }
    }
}
