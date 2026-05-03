using MediQueue.BL;
using MediQueue.Models;
using MediQueue.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IClinicService _clinicService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IClinicService clinicService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _clinicService = clinicService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                TempData["ErrorMessage"] = "Error fetching users";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching user with ID {id}");
                TempData["ErrorMessage"] = "Error fetching user";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Doctors()
        {
            try
            {
                var doctors = await _userService.GetAllDoctorsAsync();
                return View(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all doctors");
                TempData["ErrorMessage"] = "Error fetching doctors";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> DoctorsByClinic(int clinicId)
        {
            try
            {
                var clinic = await _clinicService.GetClinicByIdAsync(clinicId);
                if (clinic == null)
                {
                    TempData["ErrorMessage"] = "Clinic not found";
                    return RedirectToAction(nameof(Index));
                }

                var doctors = await _userService.GetDoctorsByClinicAsync(clinicId);
                ViewData["ClinicName"] = clinic.Name;
                return View(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching doctors for clinic {clinicId}");
                TempData["ErrorMessage"] = "Error fetching doctors";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> SearchDoctors(string specialty)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(specialty))
                {
                    TempData["ErrorMessage"] = "Please enter a specialty";
                    return RedirectToAction(nameof(Doctors));
                }

                var doctors = await _userService.SearchDoctorsBySpecialtyAsync(specialty);
                ViewData["Specialty"] = specialty;
                return View(doctors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching doctors by specialty");
                TempData["ErrorMessage"] = "Error searching doctors";
                return RedirectToAction(nameof(Doctors));
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                var clinics = await _clinicService.GetAllClinicsAsync();
                ViewData["Clinics"] = clinics;

                var userVM = new UserVM
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Specialty = user.Specialty,
                    ClinicID = user.ClinicID,
                    UserName = user.UserName
                };

                return View(userVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for user {id}");
                TempData["ErrorMessage"] = "Error loading edit form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [FromForm] UserVM userVM)
        {
            try
            {
                if (id != userVM.Id)
                {
                    TempData["ErrorMessage"] = "Invalid user ID";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    var clinics = await _clinicService.GetAllClinicsAsync();
                    ViewData["Clinics"] = clinics;
                    return View(userVM);
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }

                user.FullName = userVM.FullName;
                user.Email = userVM.Email;
                user.PhoneNumber = userVM.PhoneNumber;
                user.Specialty = userVM.Specialty;
                user.ClinicID = userVM.ClinicID;

                await _userService.UpdateUserAsync(user);
                TempData["SuccessMessage"] = "User updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {id}");
                TempData["ErrorMessage"] = "Error updating user";
                var clinics = await _clinicService.GetAllClinicsAsync();
                ViewData["Clinics"] = clinics;
                return View(userVM);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for user {id}");
                TempData["ErrorMessage"] = "Error loading delete confirmation";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "User not found";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "User deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user with ID {id}");
                TempData["ErrorMessage"] = "Error deleting user";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
