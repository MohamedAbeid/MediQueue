using MediQueue.BL;
using MediQueue.Models;
using MediQueue.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IUserService userService,
            IQueueService queueService,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _userService = userService;
            _queueService = queueService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all appointments");
                TempData["ErrorMessage"] = "Error fetching appointments";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching appointment with ID {id}");
                TempData["ErrorMessage"] = "Error fetching appointment";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> MyAppointments(string userId)
        {
            try
            {
                var appointments = await _appointmentService.GetAppointmentsByPatientAsync(userId);
                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching appointments for patient {userId}");
                TempData["ErrorMessage"] = "Error fetching your appointments";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> UpcomingAppointments(string userId)
        {
            try
            {
                var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(userId);
                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching upcoming appointments for user {userId}");
                TempData["ErrorMessage"] = "Error fetching upcoming appointments";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var doctors = await _userService.GetAllDoctorsAsync();
                ViewData["Doctors"] = doctors;
                return View(new AppointmentVM());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create form");
                TempData["ErrorMessage"] = "Error loading create form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] AppointmentVM appointmentVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var doctors = await _userService.GetAllDoctorsAsync();
                    ViewData["Doctors"] = doctors;
                    return View(appointmentVM);
                }

                var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(
                    appointmentVM.DoctorID,
                    appointmentVM.AppointmentDate,
                    appointmentVM.AppointmentTime);

                if (!isAvailable)
                {
                    var doctors = await _userService.GetAllDoctorsAsync();
                    ViewData["Doctors"] = doctors;
                    ModelState.AddModelError("", "The selected time slot is not available");
                    return View(appointmentVM);
                }

                var appointment = new Appointment
                {
                    PatientID = appointmentVM.PatientID,
                    DoctorID = appointmentVM.DoctorID,
                    AppointmentDate = appointmentVM.AppointmentDate,
                    AppointmentTime = appointmentVM.AppointmentTime,
                    Status = appointmentVM.Status
                };

                var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);

                var queue = new Queue
                {
                    AppointmentID = createdAppointment.AppointmentID,
                    DoctorID = appointmentVM.DoctorID,
                    Position = await _queueService.GetNextPositionAsync(appointmentVM.DoctorID),
                    EstimatedTime = appointmentVM.AppointmentDate.Add(appointmentVM.AppointmentTime),
                    IsActive = true
                };

                await _queueService.CreateQueueAsync(queue);

                TempData["SuccessMessage"] = "Appointment created successfully";
                return RedirectToAction(nameof(Details), new { id = createdAppointment.AppointmentID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment");
                TempData["ErrorMessage"] = "Error creating appointment";
                var doctors = await _userService.GetAllDoctorsAsync();
                ViewData["Doctors"] = doctors;
                return View(appointmentVM);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                var doctors = await _userService.GetAllDoctorsAsync();
                ViewData["Doctors"] = doctors;

                var appointmentVM = new AppointmentVM
                {
                    AppointmentID = appointment.AppointmentID,
                    PatientID = appointment.PatientID,
                    DoctorID = appointment.DoctorID,
                    AppointmentDate = appointment.AppointmentDate,
                    AppointmentTime = appointment.AppointmentTime,
                    Status = appointment.Status
                };

                return View(appointmentVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for appointment {id}");
                TempData["ErrorMessage"] = "Error loading edit form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] AppointmentVM appointmentVM)
        {
            try
            {
                if (id != appointmentVM.AppointmentID)
                {
                    TempData["ErrorMessage"] = "Invalid appointment ID";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    var doctors = await _userService.GetAllDoctorsAsync();
                    ViewData["Doctors"] = doctors;
                    return View(appointmentVM);
                }

                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }

                appointment.AppointmentDate = appointmentVM.AppointmentDate;
                appointment.AppointmentTime = appointmentVM.AppointmentTime;
                appointment.Status = appointmentVM.Status;

                await _appointmentService.UpdateAppointmentAsync(appointment);
                TempData["SuccessMessage"] = "Appointment updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating appointment with ID {id}");
                TempData["ErrorMessage"] = "Error updating appointment";
                var doctors = await _userService.GetAllDoctorsAsync();
                ViewData["Doctors"] = doctors;
                return View(appointmentVM);
            }
        }

        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading cancel confirmation for appointment {id}");
                TempData["ErrorMessage"] = "Error loading cancel confirmation";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            try
            {
                var result = await _appointmentService.CancelAppointmentAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "Appointment cancelled successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error cancelling appointment with ID {id}");
                TempData["ErrorMessage"] = "Error cancelling appointment";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    TempData["ErrorMessage"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for appointment {id}");
                TempData["ErrorMessage"] = "Error loading delete confirmation";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _appointmentService.DeleteAppointmentAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Appointment not found";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "Appointment deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting appointment with ID {id}");
                TempData["ErrorMessage"] = "Error deleting appointment";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
