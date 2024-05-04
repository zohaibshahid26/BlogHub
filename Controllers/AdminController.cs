using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Categories()
        {
            var categories = new List<string> { "Tech", "Science", "Health", "Programming" };
            return View(categories);
        }
        public IActionResult AddCategory()
        {
            return RedirectToAction("Categories");
        }
        public IActionResult DeleteCategory()
        {
            return RedirectToAction("Categories");
        }

    }
}
