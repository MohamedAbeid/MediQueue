using MediQueue.BL;
using MediQueue.Models;
using MediQueue.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorAvailableSlotService _availableSlotService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IDoctorAvailableSlotService availableSlotService,
            UserManager<User> userManager,
            ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _availableSlotService = availableSlotService;
            _userManager = userManager;
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
                ModelState.Remove("DoctorID");
                ModelState.Remove("DoctorName");
                ModelState.Remove("AvailableSlotsCount");
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    foreach (var error in errors)
                    {
                        _logger.LogError($"Model Error: {error.ErrorMessage}");
                    }
                    return View(model);
                }

                // Validate date is not in the past
                if (model.Date.Date < DateTime.Now.Date)
                {
                    ModelState.AddModelError("Date", "Cannot add slots for past dates");
                    return View(model);
                }

                // Validate time range
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
                return RedirectToAction("ManageAvailableSlots");
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

                if (!ModelState.IsValid)
                {
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
                TempData["ErrorMessage"] = "Error updating slot";
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Doctor")]
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
                TempData["SuccessMessage"] = "Slot deleted successfully!";
                return RedirectToAction(nameof(ManageAvailableSlots));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting slot");
                TempData["ErrorMessage"] = "Error deleting slot";
                return RedirectToAction(nameof(ManageAvailableSlots));
            }
        }
    }
}
