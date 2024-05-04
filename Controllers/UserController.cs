using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    public class UserController : Controller
    {
        public IActionResult SignUp()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Logout()
        {
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Profile()
        {
            return View();
        }
        [HttpPost]
        public IActionResult EditProfile()
        {
            return RedirectToAction("Profile");
        }
    }
}
