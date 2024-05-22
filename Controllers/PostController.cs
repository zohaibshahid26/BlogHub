using BlogHub.Models;
using BlogHub.UnitOfWork;
using BlogHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogHub.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private IAuthorizationService _authorizationService;
        private readonly IUnitOfWork _unitOfWork;
        public PostController(IAuthorizationService authorizationService, IUnitOfWork unitOfWork)
        {
            _authorizationService = authorizationService;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            var post =  _unitOfWork.PostRepository.Get(filter: p => p.UserId == userId, includeProperties: "Category,Tags,Image,Comments,User");
            return View(post);
        }

        [AllowAnonymous]
        public IActionResult Details(string id)
        {
            Post? post = _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: "Category,Tags,Image,Comments.User,User,Likes").FirstOrDefault();
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        public async Task<IActionResult> Add()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            return View(new PostViewModel { Categories = categories });
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel post)
        {
            if (ModelState.IsValid)
            {

                await _unitOfWork.PostRepository.AddPostAsync(post);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction("Index", "Post");
            }

            return View(post);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: "Category,Tags,Image,Comments,User,Likes").FirstOrDefault();
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
            if (post == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, post, "EditPostPolicy");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

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
            return View(postViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PostViewModel post)
        {
            await _unitOfWork.PostRepository.UpdatePost(post);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Index", "Post");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: "Category,Tags,Image,Comments,User,Likes").FirstOrDefault();
            if (post == null)
            {
                return NotFound();
            }
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, post, "DeletePostPolicy");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            await _unitOfWork.PostRepository.DeleteAsync(id);
            if (post.Image != null)
            {
                _unitOfWork.PostRepository.RemovePostImage(post.Image.ImageURL);
            }
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Index", "Post");
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ToggleLike(string postId, string userId)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Redirect("/Identity/Account/Login" + "?ReturnUrl=%2FPost%2FDetails%2F" + postId);
            }

            if (postId == null)
            {
                return NotFound();
            }
            await _unitOfWork.PostRepository.ToggleLikeAsync(postId, userId);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Details", "Post", new { id = postId });
        }
    }
}
