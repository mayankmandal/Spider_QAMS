using Spider_QAMS.Utilities.ValidationAttributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class CityVM
    {
        [DisplayName("City")]
        public int CityId { get; set; }
        [Required(ErrorMessage = "City is required")]
        [DisplayName("City Name")]
        [StringLength(100, ErrorMessage = "City Name must be 100 characters or fewer")]
        [CheckUniquenessinDB("CityName")]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "City Name must contain only alphabets and spaces")]
        public string CityName { get; set; }
        public RegionVM RegionData { get; set; }
    }
}
