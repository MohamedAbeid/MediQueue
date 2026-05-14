using MediQueue.BL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        private readonly IClinicService _clinicService;
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserService userService, 
            IClinicService clinicService, 
            IAppointmentService appointmentService,
            ILogger<AdminController> logger)
        {
            _userService = userService;
            _clinicService = clinicService;
            _appointmentService = appointmentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var clinics = await _clinicService.GetAllClinicsAsync();
                var appointments = await _appointmentService.GetAllAppointmentsAsync();

                var doctorsCount = users.Count(u => u.Specialty != null); // Approximate way to count doctors if roles are tricky to query directly here
                var patientsCount = users.Count() - doctorsCount;

                ViewBag.TotalUsers = users.Count();
                ViewBag.TotalDoctors = doctorsCount;
                ViewBag.TotalPatients = patientsCount;
                ViewBag.TotalClinics = clinics.Count();
                ViewBag.TotalAppointments = appointments.Count();
                
                var recentAppointments = appointments
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .Take(5)
                    .ToList();

                ViewBag.RecentAppointments = recentAppointments;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                return View("Error");
            }
        }
    }
}
