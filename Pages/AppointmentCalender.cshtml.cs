using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Repositories.Skeleton;

namespace Spider_QAMS.Pages
{
    [Authorize(Policy = "PageAccess")]
    public class AppointmentCalenderModel : PageModel
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentCalenderModel(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }
        public void OnGet()
        {

        }
    }
}
