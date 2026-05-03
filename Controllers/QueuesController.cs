using MediQueue.BL;
using MediQueue.Models;
using MediQueue.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    public class QueuesController : Controller
    {
        private readonly IQueueService _queueService;
        private readonly IUserService _userService;
        private readonly ILogger<QueuesController> _logger;

        public QueuesController(IQueueService queueService, IUserService userService, ILogger<QueuesController> logger)
        {
            _queueService = queueService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var queues = await _queueService.GetAllQueuesAsync();
                return View(queues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all queues");
                TempData["ErrorMessage"] = "Error fetching queues";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var queue = await _queueService.GetQueueByIdAsync(id);
                if (queue == null)
                {
                    TempData["ErrorMessage"] = "Queue not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(queue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching queue with ID {id}");
                TempData["ErrorMessage"] = "Error fetching queue";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> DoctorQueue(string doctorId)
        {
            try
            {
                var doctor = await _userService.GetUserByIdAsync(doctorId);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }

                var queues = await _queueService.GetQueuesByDoctorAsync(doctorId);
                ViewData["DoctorName"] = doctor.FullName;
                return View(queues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching queue for doctor {doctorId}");
                TempData["ErrorMessage"] = "Error fetching queue";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ActiveQueue(string doctorId)
        {
            try
            {
                var doctor = await _userService.GetUserByIdAsync(doctorId);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }

                var activeQueues = await _queueService.GetActiveQueuesAsync(doctorId);
                ViewData["DoctorName"] = doctor.FullName;
                return View(activeQueues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching active queue for doctor {doctorId}");
                TempData["ErrorMessage"] = "Error fetching active queue";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var queue = await _queueService.GetQueueByIdAsync(id);
                if (queue == null)
                {
                    TempData["ErrorMessage"] = "Queue not found";
                    return RedirectToAction(nameof(Index));
                }

                var queueVM = new QueueVM
                {
                    QueueID = queue.QueueID,
                    AppointmentID = queue.AppointmentID,
                    DoctorID = queue.DoctorID,
                    Position = queue.Position,
                    EstimatedTime = queue.EstimatedTime,
                    IsActive = queue.IsActive
                };

                return View(queueVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for queue {id}");
                TempData["ErrorMessage"] = "Error loading edit form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] QueueVM queueVM)
        {
            try
            {
                if (id != queueVM.QueueID)
                {
                    TempData["ErrorMessage"] = "Invalid queue ID";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    return View(queueVM);
                }

                var queue = await _queueService.GetQueueByIdAsync(id);
                if (queue == null)
                {
                    TempData["ErrorMessage"] = "Queue not found";
                    return RedirectToAction(nameof(Index));
                }

                queue.Position = queueVM.Position;
                queue.EstimatedTime = queueVM.EstimatedTime;
                queue.IsActive = queueVM.IsActive;

                await _queueService.UpdateQueueAsync(queue);
                TempData["SuccessMessage"] = "Queue updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating queue with ID {id}");
                TempData["ErrorMessage"] = "Error updating queue";
                return View(queueVM);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var queue = await _queueService.GetQueueByIdAsync(id);
                if (queue == null)
                {
                    TempData["ErrorMessage"] = "Queue not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(queue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for queue {id}");
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
                var result = await _queueService.DeleteQueueAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Queue not found";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "Queue deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting queue with ID {id}");
                TempData["ErrorMessage"] = "Error deleting queue";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Reorder(string doctorId)
        {
            try
            {
                var doctor = await _userService.GetUserByIdAsync(doctorId);
                if (doctor == null)
                {
                    TempData["ErrorMessage"] = "Doctor not found";
                    return RedirectToAction(nameof(Index));
                }

                await _queueService.ReorderQueueAsync(doctorId);
                TempData["SuccessMessage"] = "Queue reordered successfully";
                return RedirectToAction(nameof(DoctorQueue), new { doctorId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reordering queue for doctor {doctorId}");
                TempData["ErrorMessage"] = "Error reordering queue";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var queue = await _queueService.GetQueueByIdAsync(id);
                if (queue == null)
                {
                    TempData["ErrorMessage"] = "Queue not found";
                    return RedirectToAction(nameof(Index));
                }

                await _queueService.DeactivateQueueAsync(id);
                TempData["SuccessMessage"] = "Queue deactivated successfully";
                return RedirectToAction(nameof(DoctorQueue), new { doctorId = queue.DoctorID });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating queue with ID {id}");
                TempData["ErrorMessage"] = "Error deactivating queue";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
