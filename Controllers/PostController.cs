using BlogHub.Models;
using BlogHub.ViewModels;
using BlogHub.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BlogHub.Controllers
{
    [Authorize]
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
        [AllowAnonymous]
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
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(PostViewModel post)
        {
           
                if (ModelState.IsValid)
                { 
                    await _postRepository.AddPostAsync(post);
                    await _postRepository.SaveChangesAsync();
                    return RedirectToAction("Index", "Post");
                }
            return View(post);
           
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
                _postRepository.UpdatePost(post);
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
