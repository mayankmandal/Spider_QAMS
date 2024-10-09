using Spider_QAMS.Utilities.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class RegionVM
    {
        [DisplayName("Region Id")]
        public int RegionId { get; set; }
        [Required(ErrorMessage = "Region Name is required")]
        [DisplayName("Region Name")]
        [StringLength(100, ErrorMessage = "Region Name must be 100 characters or fewer")]
        [CheckUniquenessinDB("RegionName")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Region Name must contain only alphabets and spaces")]
        public string RegionName { get; set; }
    }
}
