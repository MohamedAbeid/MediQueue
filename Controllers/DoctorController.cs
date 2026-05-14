using MediQueue.BL;
using MediQueue.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IQueueService _queueService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<DoctorController> _logger;

        public DoctorController(
            IAppointmentService appointmentService,
            IQueueService queueService,
            UserManager<User> userManager,
            ILogger<DoctorController> logger)
        {
            _appointmentService = appointmentService;
            _queueService = queueService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var doctor = await _userManager.GetUserAsync(User);
                if (doctor == null) return NotFound("Doctor not found");

                var allAppointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctor.Id);
                var today = DateTime.Today;
                
                var todaysAppointments = allAppointments
                    .Where(a => a.AppointmentDate.Date == today && a.Status != AppointmentStatus.Cancelled)
                    .OrderBy(a => a.AppointmentTime)
                    .ToList();

                var upcomingAppointments = allAppointments
                    .Where(a => a.AppointmentDate.Date > today && a.Status == AppointmentStatus.Booked)
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .Take(5)
                    .ToList();

                ViewBag.TotalAppointments = allAppointments.Count();
                ViewBag.TodaysCount = todaysAppointments.Count;
                ViewBag.CompletedCount = allAppointments.Count(a => a.Status == AppointmentStatus.Completed);
                ViewBag.UpcomingAppointments = upcomingAppointments;

                return View(todaysAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor dashboard");
                return View("Error");
            }
        }

        public async Task<IActionResult> Queue()
        {
            try
            {
                var doctor = await _userManager.GetUserAsync(User);
                if (doctor == null) return NotFound("Doctor not found");

                var activeQueue = await _queueService.GetActiveQueuesAsync(doctor.Id);
                var sortedQueue = activeQueue.OrderBy(q => q.Position).ToList();

                return View(sortedQueue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor queue");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AdvanceQueue(int queueId)
        {
            try
            {
                var queueItem = await _queueService.GetQueueByIdAsync(queueId);
                if (queueItem != null)
                {
                    queueItem.IsActive = false;
                    await _queueService.UpdateQueueAsync(queueItem);

                    // Update appointment status to completed
                    var appointment = await _appointmentService.GetAppointmentByIdAsync(queueItem.AppointmentID);
                    if (appointment != null)
                    {
                        appointment.Status = AppointmentStatus.Completed;
                        await _appointmentService.UpdateAppointmentAsync(appointment);
                    }

                    var doctor = await _userManager.GetUserAsync(User);
                    await _queueService.ReorderQueueAsync(doctor.Id);
                    
                    TempData["SuccessMessage"] = "تم تحديث حالة المريض بنجاح.";
                }
                return RedirectToAction(nameof(Queue));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error advancing queue");
                TempData["ErrorMessage"] = "حدث خطأ أثناء تحديث حالة المريض.";
                return RedirectToAction(nameof(Queue));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                var doctor = await _userManager.GetUserAsync(User);
                if (doctor == null) return NotFound("Doctor not found");

                var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
                if (appointment != null && appointment.DoctorID == doctor.Id)
                {
                    // Cancel the appointment using the service
                    var success = await _appointmentService.CancelAppointmentAsync(appointmentId);
                    if (success)
                    {
                        // Remove from queue if it exists
                        var queueItem = await _queueService.GetQueueByAppointmentAsync(appointmentId);
                        if (queueItem != null)
                        {
                            await _queueService.DeleteQueueAsync(queueItem.QueueID);
                            await _queueService.ReorderQueueAsync(doctor.Id);
                        }

                        TempData["SuccessMessage"] = "تم إلغاء الموعد بنجاح.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "فشل في إلغاء الموعد.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "الموعد غير موجود أو لا تملك صلاحية لإلغائه.";
                }

                return RedirectToAction(nameof(Dashboard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling appointment");
                TempData["ErrorMessage"] = "حدث خطأ أثناء إلغاء الموعد.";
                return RedirectToAction(nameof(Dashboard));
            }
        }
    }
}
