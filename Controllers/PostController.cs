using BlogHub.Models;
using BlogHub.ViewModels;
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

        public async Task<IActionResult> Index()
        { 
            var posts = await _postRepository.GetPostsAsync();
            return View(posts);
        }

        public async Task<IActionResult> Details(string id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        public IActionResult Add()
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel post)
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            else
            {
                if (ModelState.IsValid)
                { 
                    await _postRepository.AddPostAsync(post);
                    await _postRepository.SaveChangesAsync();
                    return RedirectToAction("Index", "Post");
                }
                return View(post);
            }
           
        }
        public IActionResult Edit(string id)
        {
            var post = _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Post post)
        {
            if (ModelState.IsValid)
            {
                await _postRepository.UpdatePostAsync(post);
                await _postRepository.SaveChangesAsync();
                return RedirectToAction("Index", "Post");
            }
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            await _postRepository.DeletePostAsync(id);
            await _postRepository.SaveChangesAsync();
            return RedirectToAction("Index", "Post");
        }


    }
}
