using System.ComponentModel;

namespace Spider_QAMS.Models.ViewModels
{
    public class VisitStatusVM
    {
        [DisplayName("Visit Status ID")]
        public int VisitStatusID { get; set; }

        [DisplayName("Visit Status")]
        public string? VisitStatus { get; set; }
    }
}
