using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    [Authorize]
    public class UserController : Controller
    {

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
