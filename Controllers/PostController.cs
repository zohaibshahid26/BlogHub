using BlogHub.ViewModels;
using BlogHub.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            var post = await _postRepository.GetPostsByUserIdAsync(userId);

            return View(post);
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

        public async Task<IActionResult> Add()
        {
            var categories = await _postRepository.GetCategories();
            return View(new PostViewModel { Categories = categories });
        }

        [HttpPost]
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


        public async Task<IActionResult> Edit(string ?id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _postRepository.GetPostByIdAsync(id);
            var categories = await _postRepository.GetCategories();
           
            var postViewModel = new PostViewModel
            {
                PostId = post!.PostId,
                Title = post.Title,
                Content = post.Content,
                Category = post.Category,
                Categories = categories
            };
            if (post.Tags != null)
            {
                postViewModel.Tags = string.Join(",", post.Tags.Select(t => t.TagName));
            }
            await _postRepository.SaveChangesAsync();
            return View(postViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PostViewModel post)
        {
            await _postRepository.UpdatePost(post);
            await _postRepository.SaveChangesAsync();
            return RedirectToAction("Index", "Post");
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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ToggleLike(string postId, string userId)
        {
            if(User.Identity?.IsAuthenticated != true)
            {
                return Redirect("/Identity/Account/Login" + "?ReturnUrl=%2FPost%2FDetails%2F" + postId);
            }

            if (postId == null)
            {
                return NotFound();
            }
            await _postRepository.ToggleLikeAsync(postId, userId);
            await _postRepository.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = postId });
        }

    }
}
