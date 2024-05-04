using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    public class CommentController : Controller
    {
        public IActionResult Add()
        {
            return RedirectToAction("Index", "Post", new { id = 1 });
        }
        public IActionResult Delete()
        {
            return RedirectToAction("Index", "Post", new { id = 1 });
        }
    }
}
