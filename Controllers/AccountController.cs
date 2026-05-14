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
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await signInManger.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح.";
                
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) return RedirectToAction("Index", "Admin");
                if (roles.Contains("Doctor")) return RedirectToAction("Dashboard", "Doctor");
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}
