using MediQueue.Models;
using MediQueue.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.Controllers
{
    public class AccountController: Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManger;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManger)
        {
            this.userManager = userManager;
            this.signInManger = signInManger;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel newUserVM)
        {
            if (ModelState.IsValid)
            {
                User userModel = new User();
                userModel.FullName = newUserVM.FullName;
                userModel.PhoneNumber = newUserVM.PhoneNumber;
                userModel.Email = newUserVM.Email;
                userModel.UserName = newUserVM.Email;
                userModel.PasswordHash = newUserVM.Password;
                IdentityResult result = await userManager.CreateAsync(userModel, newUserVM.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(userModel, "Patient");
                    await signInManger.SignInAsync(userModel, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var item in result.Errors
)
                    {
                        ModelState.AddModelError("Password", item.Description);

                    }
                }
            }
            return View(newUserVM);
        }

        public IActionResult Logout()
        {
            signInManger.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                User userModel = await userManager.FindByEmailAsync(userViewModel.Email);
                if (userModel != null)
                {
                    bool found = await userManager.CheckPasswordAsync(userModel, userViewModel.Password);
                    if (found)
                    {
                        await signInManger.SignInAsync(userModel, userViewModel.RememberMe);
                        
                        var roles = await userManager.GetRolesAsync(userModel);
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (roles.Contains("Doctor"))
                        {
                            return RedirectToAction("Dashboard", "Doctor");
                        }
                        
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError("", "Email Or Password Wrong");
            }
            return View(userViewModel);
        }
    }
}
