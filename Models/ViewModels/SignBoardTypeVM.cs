using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class SignBoardTypeVM
    {
        [DisplayName("Type of Sign Board")]
        public string? TypeOfSignBoard { get; set; }

        [DisplayName("Cylinder")]
        public bool Cylinder { get; set; } = false;

        [DisplayName("Straight or Totem")]
        public bool StraightOrTotem { get; set; } = false;
    }
}
