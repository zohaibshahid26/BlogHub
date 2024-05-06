using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BlogHub.Controllers
{
    public class PostController : Controller
    {
        [Route("/MyPosts")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }
        public IActionResult Edit()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
        }
        [Route("/Posts/{id}")]
        public IActionResult Index(int id)
        {
            return View("Post");
        }

    }
}
