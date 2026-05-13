using MediQueue.BL;
using MediQueue.Models;
using MediQueue.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    public class ClinicsController : Controller
    {
        private readonly IClinicService _clinicService;
        private readonly ILogger<ClinicsController> _logger;

        public ClinicsController(IClinicService clinicService, ILogger<ClinicsController> logger)
        {
            _clinicService = clinicService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var clinics = await _clinicService.GetAllClinicsAsync();
                return View(clinics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all clinics");
                TempData["ErrorMessage"] = "Error fetching clinics";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var clinic = await _clinicService.GetClinicByIdAsync(id);
                if (clinic == null)
                {
                    TempData["ErrorMessage"] = "Clinic not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(clinic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching clinic with ID {id}");
                TempData["ErrorMessage"] = "Error fetching clinic";
                return RedirectToAction(nameof(Index));
            }
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new ClinicVM());
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ClinicVM clinicVM)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(clinicVM);
                }

                var clinic = new Clinic
                {
                    Name = clinicVM.Name,
                    Address = clinicVM.Address,
                    PhoneNumber = clinicVM.PhoneNumber
                };

                await _clinicService.CreateClinicAsync(clinic);
                TempData["SuccessMessage"] = "Clinic created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating clinic");
                TempData["ErrorMessage"] = "Error creating clinic";
                return View(clinicVM);
            }
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var clinic = await _clinicService.GetClinicByIdAsync(id);
                if (clinic == null)
                {
                    TempData["ErrorMessage"] = "Clinic not found";
                    return RedirectToAction(nameof(Index));
                }

                var clinicVM = new ClinicVM
                {
                    ClinicID = clinic.ClinicID,
                    Name = clinic.Name,
                    Address = clinic.Address,
                    PhoneNumber = clinic.PhoneNumber
                };

                return View(clinicVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading edit form for clinic {id}");
                TempData["ErrorMessage"] = "Error loading edit form";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] ClinicVM clinicVM)
        {
            try
            {
                if (id != clinicVM.ClinicID)
                {
                    TempData["ErrorMessage"] = "Invalid clinic ID";
                    return RedirectToAction(nameof(Index));
                }

                if (!ModelState.IsValid)
                {
                    return View(clinicVM);
                }

                var clinic = await _clinicService.GetClinicByIdAsync(id);
                if (clinic == null)
                {
                    TempData["ErrorMessage"] = "Clinic not found";
                    return RedirectToAction(nameof(Index));
                }

                clinic.Name = clinicVM.Name;
                clinic.Address = clinicVM.Address;
                clinic.PhoneNumber = clinicVM.PhoneNumber;

                await _clinicService.UpdateClinicAsync(clinic);
                TempData["SuccessMessage"] = "Clinic updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating clinic with ID {id}");
                TempData["ErrorMessage"] = "Error updating clinic";
                return View(clinicVM);
            }
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var clinic = await _clinicService.GetClinicByIdAsync(id);
                if (clinic == null)
                {
                    TempData["ErrorMessage"] = "Clinic not found";
                    return RedirectToAction(nameof(Index));
                }
                return View(clinic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for clinic {id}");
                TempData["ErrorMessage"] = "Error loading delete confirmation";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _clinicService.DeleteClinicAsync(id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Clinic not found";
                    return RedirectToAction(nameof(Index));
                }
                TempData["SuccessMessage"] = "Clinic deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting clinic with ID {id}");
                TempData["ErrorMessage"] = "Error deleting clinic";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
