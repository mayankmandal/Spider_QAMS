using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SignBoardTypeVM
    {
        [DisplayName("Cylinder")]
        public bool Cylinder { get; set; } = false;

        [DisplayName("Straight or Totem")]
        public bool StraightOrTotem { get; set; } = false;
    }
}
