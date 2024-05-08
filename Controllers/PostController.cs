using BlogHub.Models;
using BlogHub.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Add(Post post)
        {
            if (ModelState.IsValid)
            {
                _postRepository.AddPostAsync(post);
                return RedirectToAction("Index");
            }
            return View(post);

        }
        public IActionResult Edit()
        {
            return View();
        }
        public IActionResult Delete()
        {
            return View();
        }

    }
}
