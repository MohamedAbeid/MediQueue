using MediQueue.BL;
using MediQueue.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MediQueue.Controllers
{
    public class HomeController : Controller
    {
        private readonly IClinicService _clinicService;
        private readonly IUserService _userService;

        public HomeController(IClinicService clinicService, IUserService userService)
        {
            _clinicService = clinicService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var clinics = await _clinicService.GetAllClinicsAsync();
            var doctors = await _userService.GetAllDoctorsAsync();

            var data = new Dictionary<string, object>
            {
                { "Clinics", clinics },
                { "Doctors", doctors }
            };

            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
