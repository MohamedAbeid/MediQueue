using MediQueue.BL;
using MediQueue.Models;
using MediQueue.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediQueue.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorAvailableSlotService _availableSlotService;
        private readonly UserManager<User> _userManager;
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;
        private readonly MediQueueContext _context;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IDoctorAvailableSlotService availableSlotService,
            UserManager<User> userManager,
            IUserService userService,
            IQueueService queueService,
            MediQueueContext context,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _availableSlotService = availableSlotService;
            _userManager = userManager;
            _userService = userService;
            _queueService = queueService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ManageAvailableSlots()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var slots = await _availableSlotService.GetAllSlotsByDoctorAsync(user.Id);
                return View(slots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available slots");
                TempData["ErrorMessage"] = "Error loading available slots";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public IActionResult AddAvailableSlot()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailableSlot(AvailableSlotViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                ModelState.Remove("SlotID");
                ModelState.Remove("DoctorID");
                ModelState.Remove("DoctorName");
                ModelState.Remove("AvailableSlotsCount");
                ModelState.Remove("CurrentBookings");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        _logger.LogError($"Model Error: {error.ErrorMessage}");
                    }
                    return View(model);
                }

                if (model.Date.Date < DateTime.Now.Date)
                {
                    ModelState.AddModelError("Date", "Cannot add slots for past dates");
                    return View(model);
                }

                if (model.EndTime <= model.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    return View(model);
                }

                var slot = new DoctorAvailableSlot
                {
                    DoctorID = user.Id,
                    Date = model.Date,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    MaxPatients = model.MaxPatients,
                    IsActive = true
                };

                await _availableSlotService.CreateSlotAsync(slot);
                TempData["SuccessMessage"] = "Available slot added successfully!";
                return RedirectToAction(nameof(ManageAvailableSlots));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding available slot");
                TempData["ErrorMessage"] = $"Error adding available slot: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> EditAvailableSlot(int id)
        {
            try
            {
                var slot = await _availableSlotService.GetSlotByIdAsync(id);
                if (slot == null)
                {
                    TempData["ErrorMessage"] = "Slot not found";
                    return RedirectToAction(nameof(ManageAvailableSlots));
                }

                var user = await _userManager.GetUserAsync(User);
                if (user?.Id != slot.DoctorID)
                {
                    return Unauthorized();
                }

                var model = new AvailableSlotViewModel
                {
                    SlotID = slot.SlotID,
                    Date = slot.Date,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    MaxPatients = slot.MaxPatients,
                    CurrentBookings = slot.CurrentBookings,
                    AvailableSlotsCount = slot.MaxPatients - slot.CurrentBookings
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit slot");
                TempData["ErrorMessage"] = "Error loading slot";
                return RedirectToAction(nameof(ManageAvailableSlots));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAvailableSlot(int id, AvailableSlotViewModel model)
        {
            try
            {
                if (id != model.SlotID)
                {
                    TempData["ErrorMessage"] = "Invalid slot ID";
                    return RedirectToAction(nameof(ManageAvailableSlots));
                }

                var slot = await _availableSlotService.GetSlotByIdAsync(id);
                if (slot == null)
                {
                    TempData["ErrorMessage"] = "Slot not found";
                    return RedirectToAction(nameof(ManageAvailableSlots));
                }

                var user = await _userManager.GetUserAsync(User);
                if (user?.Id != slot.DoctorID)
                {
                    return Unauthorized();
                }

                ModelState.Remove("DoctorID");
                ModelState.Remove("DoctorName");
                ModelState.Remove("AvailableSlotsCount");
                ModelState.Remove("CurrentBookings");

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        _logger.LogError($"Edit Model Error: {error.ErrorMessage}");
                    }
                    return View(model);
                }

                if (model.EndTime <= model.StartTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                    return View(model);
                }

                slot.Date = model.Date;
                slot.StartTime = model.StartTime;
                slot.EndTime = model.EndTime;
                slot.MaxPatients = model.MaxPatients;

                await _availableSlotService.UpdateSlotAsync(slot);
                TempData["SuccessMessage"] = "Slot updated successfully!";
                return RedirectToAction(nameof(ManageAvailableSlots));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slot");
                TempData["ErrorMessage"] = $"Error updating slot: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailableSlot(int id)
        {
            try
            {
                var slot = await _availableSlotService.GetSlotByIdAsync(id);
                if (slot == null)
                {
                    TempData["ErrorMessage"] = "Slot not found";
                    return RedirectToAction(nameof(ManageAvailableSlots));
                }

                var user = await _userManager.GetUserAsync(User);
                if (user?.Id != slot.DoctorID)
                {
                    return Unauthorized();
                }

                await _availableSlotService.DeleteSlotAsync(id);
                TempData["SuccessMessage"] = "تم حذف الموعد بنجاح!";
                return RedirectToAction(nameof(ManageAvailableSlots));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting slot");
                TempData["ErrorMessage"] = "حدث خطأ أثناء محاولة الحذف.";
                return RedirectToAction(nameof(ManageAvailableSlots));
            }
        }

        //[HttpPost]
        //[Authorize(Roles = "Doctor")]
        //public async Task<IActionResult> DeleteAvailableSlot(int id)
        //{
        //    try
        //    {
        //        var slot = await _availableSlotService.GetSlotByIdAsync(id);
        //        if (slot == null)
        //        {
        //            TempData["ErrorMessage"] = "Slot not found";
        //            return RedirectToAction(nameof(ManageAvailableSlots));
        //        }

        //        var user = await _userManager.GetUserAsync(User);
        //        if (user?.Id != slot.DoctorID)
        //        {
        //            return Unauthorized();
        //        }

        //        await _availableSlotService.DeleteSlotAsync(id);
        //        TempData["SuccessMessage"] = "Slot deleted successfully!";
        //        return RedirectToAction(nameof(ManageAvailableSlots));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error deleting slot");
        //        TempData["ErrorMessage"] = "Error deleting slot";
        //        return RedirectToAction(nameof(ManageAvailableSlots));
        //    }
        //}
        // GET: /Appointments/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            var appointments = await _appointmentService.GetAppointmentsByPatientAsync(user!.Id);
            return View(appointments);
        }
        [HttpGet]
        public async Task<IActionResult> GetSlots(string doctorId, DateTime date)
        {
            var doctor = await _context.Users
                .Include(d => d.AvailableSlots)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return NotFound();

            var slots = doctor.AvailableSlots
                .Where(s => s.Date.Date == date.Date && s.IsActive && s.CurrentBookings < s.MaxPatients)
                .Select(s => new
                {
                    time = s.StartTime.ToString(@"hh\:mm"),
                    display = $"{s.StartTime.ToString(@"hh\:mm")} - {s.EndTime.ToString(@"hh\:mm")}"
                })
                .ToList();

            return Json(slots);
        }
        // GET: /Appointments/Book?doctorId=X
        [HttpGet]
        public async Task<IActionResult> Book(string doctorId)
        {
            var doctor = await _userService.GetUserByIdAsync(doctorId);

            if (doctor == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // 👇 نجيب التواريخ فقط بدون تكرار
            var dates = doctor.AvailableSlots
                .Where(s => s.IsActive && s.Date.Date >= DateTime.Today && s.CurrentBookings < s.MaxPatients)
                .Select(s => s.Date.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            ViewBag.Dates = dates;
            ViewBag.Slots = doctor.AvailableSlots
                .Where(s => s.IsActive && s.Date.Date >= DateTime.Today && s.CurrentBookings < s.MaxPatients)
                .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                .ToList();

            ViewData["Doctor"] = doctor;

            var model = new AppointmentVM
            {
                DoctorID = doctor.Id,
                DoctorName = doctor.FullName,
                PatientID = user!.Id,
                PatientName = user.FullName,
                AppointmentDate = DateTime.Today
            };

            return View(model);
        }

        //POST: /Appointments/Book
        [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentVM model)
        {
            var doctor = await _userService.GetUserByIdAsync(model.DoctorID);
            ViewData["Doctor"] = doctor;

            // Check if the slot is still available
            var selectedSlot = await _context.DoctorAvailableSlots
                .FirstOrDefaultAsync(s => s.DoctorID == model.DoctorID &&
                                          s.Date.Date == model.AppointmentDate.Date &&
                                          s.StartTime == model.AppointmentTime &&
                                          s.IsActive);

            if (selectedSlot == null || selectedSlot.CurrentBookings >= selectedSlot.MaxPatients)
            {
                ModelState.AddModelError("", "عذراً، هذا الموعد لم يعد متاحاً (اكتمل العدد).");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Dates = doctor?.AvailableSlots?
                    .Where(s => s.IsActive && s.Date.Date >= DateTime.Today && s.CurrentBookings < s.MaxPatients)
                    .Select(s => s.Date.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList() ?? new List<DateTime>();
                ViewBag.Slots = doctor?.AvailableSlots?
                    .Where(s => s.IsActive && s.Date.Date >= DateTime.Today && s.CurrentBookings < s.MaxPatients)
                    .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
                    .ToList() ?? new List<DoctorAvailableSlot>();
                return View(model);
            }

            var appointment = new Appointment
            {
                PatientID = model.PatientID,
                DoctorID = model.DoctorID,
                AppointmentDate = model.AppointmentDate,
                AppointmentTime = model.AppointmentTime,
                Status = AppointmentStatus.Booked
            };

            var created = await _appointmentService.CreateAppointmentAsync(appointment);

            // Update the slot's current bookings
            var slot = await _context.DoctorAvailableSlots
                .FirstOrDefaultAsync(s => s.DoctorID == model.DoctorID &&
                                          s.Date.Date == model.AppointmentDate.Date &&
                                          s.StartTime == model.AppointmentTime &&
                                          s.IsActive);

            if (slot != null)
            {
                await _availableSlotService.BookSlotAsync(slot.SlotID);
            }

            var queue = new Queue
            {
                AppointmentID = created.AppointmentID,
                DoctorID = model.DoctorID,
                Position = await _queueService.GetNextPositionAsync(model.DoctorID),
                EstimatedTime = model.AppointmentDate.Add(model.AppointmentTime),
                IsActive = true
            };

            await _queueService.CreateQueueAsync(queue);

            return RedirectToAction(nameof(Track), new { id = created.AppointmentID });
        }


        // GET: /Appointments/Track/5
        public async Task<IActionResult> Track(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null) return NotFound();

            // Ensure patient can only track their own appointment
            var user = await _userManager.GetUserAsync(User);
            if (appointment.PatientID != user!.Id && !User.IsInRole("Admin"))
                return Forbid();

            return View(appointment);
        }

        // API Endpoint for frontend polling
        [HttpGet]
        public async Task<IActionResult> GetQueueStatus(int id)
        {
            var queue = await _queueService.GetQueueByAppointmentAsync(id);
            if (queue == null) return NotFound();

            var peopleAhead = queue.Position - 1;

            return Json(new
            {
                position = queue.Position,
                peopleAhead = peopleAhead < 0 ? 0 : peopleAhead,
                estimatedTime = queue.EstimatedTime.ToString("hh:mm tt"),
                isActive = queue.IsActive
            });
        }

        // POST: /Appointments/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            await _appointmentService.CancelAppointmentAsync(id);
            return RedirectToAction(nameof(MyAppointments));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var allAppointments = await _appointmentService.GetAllAppointmentsAsync();
                var orderedAppointments = allAppointments.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime).ToList();
                return View(orderedAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all appointments for admin");
                TempData["ErrorMessage"] = "حدث خطأ أثناء جلب قائمة الحجوزات.";
                return RedirectToAction("Index", "Admin");
            }
        }
    }
        
    public class BookAppointmentRequest
    {
        public string DoctorId { get; set; }
        public int SlotId { get; set; }
        public string AppointmentDate { get; set; }
    }
}
