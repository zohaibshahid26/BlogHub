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
        private readonly IAuthorizationService _authorizationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostController> _logger;

        public PostController(IAuthorizationService authorizationService, IUnitOfWork unitOfWork, ILogger<PostController> logger)
        {
            _authorizationService = authorizationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [Route("/Posts")]
        public IActionResult Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
                var posts = _unitOfWork.PostRepository.Get(filter: p => p.UserId == userId, includeProperties: "Category,Tags,Image,Comments,User");
                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching posts for the user.");
                return StatusCode(500, "Internal server error");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var post = _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: "Category,Tags,Image,Comments.User,User,Comments.User.Image,User.Image,Likes").FirstOrDefault();
                if (post == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found.", id);
                    return NotFound();
                }

                string currentUserImageUrl = string.Empty;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    currentUserImageUrl = _unitOfWork.UserRepository.Get(filter: u => u.Id == userId, includeProperties: "Image").FirstOrDefault()?.Image?.ImageURL ?? string.Empty;
                    ViewData["CurrentUserImageUrl"] = currentUserImageUrl;
                }
                else
                {
                    ViewData["CurrentUserImageUrl"] = Path.Combine("profileImages", "default_profile.jpg");
                }

                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30),
                    HttpOnly = true
                };

                var existingPosts = Request.Cookies["RecentlyViewedPosts"];
                var recentlyViewedPosts = existingPosts != null ? existingPosts.Split(',').ToList() : new List<string>();

                if (!recentlyViewedPosts.Contains(id))
                {
                    recentlyViewedPosts.Add(id);
                    if (recentlyViewedPosts.Count > 5)
                    {
                        recentlyViewedPosts.RemoveAt(0);
                    }
                }

                Response.Cookies.Append("RecentlyViewedPosts", string.Join(",", recentlyViewedPosts), cookieOptions);
                post.ViewCount++;
                _unitOfWork.PostRepository.Update(post);
                await _unitOfWork.SaveChangesAsync();

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching post details for ID {PostId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> Add()
        {
            try
            {
                var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
                return View(new PostViewModel { Categories = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the add post view.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel post)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tagNames = post.Tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct();
                    var tags = new HashSet<Tag>();
                    if (tagNames != null)
                    {
                        foreach (var tagName in tagNames)
                        {
                            var tag = _unitOfWork.TagRepository.Get(filter: t => t.TagName == tagName).FirstOrDefault() ?? new Tag { TagName = tagName };
                            tags.Add(tag);
                        }
                    }

                    var Post = new Post
                    {
                        Title = post.Title,
                        Content = post.Content,
                        Tags = tags.ToList(),
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous",
                        CategoryId = _unitOfWork.CategoryRepository.Get(filter: c => c.CategoryName == post.Category.CategoryName).FirstOrDefault()!.CategoryId,
                        Image = new Image { ImageURL = await _unitOfWork.ImageRepository.SaveImageAsync(post.Image, "featureImages") }
                    };

                    await _unitOfWork.PostRepository.AddAsync(Post);
                    await _unitOfWork.SaveChangesAsync();
                    return RedirectToAction("Index", "Post");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while adding a new post.");
                    return StatusCode(500, "Internal server error");
                }
            }

            return View(post);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit post called with null ID.");
                return NotFound();
            }

            try
            {
                var post = _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: "Category,Tags,Image,Comments,User,Likes").FirstOrDefault();
                if (post == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found for editing.", id);
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
                    Categories = await _unitOfWork.CategoryRepository.GetAllAsync()
                };

                if (post.Tags != null)
                {
                    postViewModel.Tags = string.Join(",", post.Tags.Select(t => t.TagName));
                }

                return View(postViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the edit post view for ID {PostId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PostViewModel post)
        {
            try
            {
                var postToUpdate = _unitOfWork.PostRepository.Get(filter: p => p.PostId == post.PostId, includeProperties: "Tags,User,Image").FirstOrDefault();
                if (postToUpdate == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found for updating.", post.PostId);
                    return NotFound();
                }

                var authorizationResult = await _authorizationService.AuthorizeAsync(User, postToUpdate, "EditPostPolicy");
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }

                if (ModelState.IsValid)
                {
                    postToUpdate.Title = post.Title;
                    postToUpdate.Content = post.Content;
                    postToUpdate.CategoryId = _unitOfWork.CategoryRepository.Get(filter: c => c.CategoryName == post.Category.CategoryName).FirstOrDefault()!.CategoryId;

                    var tagNames = post.Tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
                    if (tagNames != null)
                    {
                        var existingTags = postToUpdate.Tags ?? new List<Tag>();
                        var newTags = new List<Tag>();
                        foreach (var tagName in tagNames)
                        {
                            var tag = _unitOfWork.TagRepository.Get(filter: t => t.TagName == tagName).FirstOrDefault() ?? new Tag { TagName = tagName };
                            newTags.Add(tag);
                        }
                        postToUpdate.Tags = existingTags.Union(newTags).ToList();
                    }

                    if (post.Image != null)
                    {
                        if (postToUpdate.Image != null)
                        {
                            var imageId = postToUpdate.Image.ImageId;
                            postToUpdate.ImageId = null;
                            _unitOfWork.ImageRepository.RemoveImage(postToUpdate.Image.ImageURL);
                            await _unitOfWork.ImageRepository.DeleteAsync(imageId);
                        }
                        postToUpdate.Image = new Image { ImageURL = await _unitOfWork.ImageRepository.SaveImageAsync(post.Image, "featureImages") };
                    }

                    _unitOfWork.PostRepository.Update(postToUpdate);
                    await _unitOfWork.SaveChangesAsync();
                    return RedirectToAction("Index", "Post");
                }

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating post with ID {PostId}.", post.PostId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete post called with null ID.");
                return NotFound();
            }

            try
            {
                var post = _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: "Category,Tags,Image,Comments,User,Likes").FirstOrDefault();
                if (post == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found for deletion.", id);
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
                    _unitOfWork.ImageRepository.RemoveImage(post.Image.ImageURL);
                    await _unitOfWork.ImageRepository.DeleteAsync(post.Image.ImageId);
                }

                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction("Index", "Post");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting post with ID {PostId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ToggleLike(string postId, string userId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Json(new { success = false, isAuthenticated = false, redirectUrl = "/Identity/Account/Login?ReturnUrl=%2FPost%2FDetails%2F" + postId });
            }

            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Toggle like called with null or empty post ID.");
                return Json(new { success = false, message = "Invalid request." });
            }

            try
            {
                await _unitOfWork.PostRepository.ToggleLikeAsync(postId, userId);
                await _unitOfWork.SaveChangesAsync();

                var postLikes = _unitOfWork.PostRepository.Get(filter: p => p.PostId == postId, includeProperties: "Likes").FirstOrDefault()?.Likes;
                bool isLiked = postLikes?.Any(l => l.UserId == userId) ?? false;
                int likeCount = postLikes?.Count ?? 0;

                return Json(new { success = true, isLiked, likeCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling like for post with ID {PostId}.", postId);
                return Json(new { success = false, message = "An error occurred." });
            }
        }


        [AllowAnonymous]
        public IActionResult Search()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult SearchAjax(string? query)
        {
            try
            {
                TempData["SearchQuery"] = query;
                if (query == null)
                {
                    return PartialView("_NoPostFound");
                }

                var allPosts = _unitOfWork.PostRepository.Get(
                    includeProperties: "Category,Tags,Image,Comments,User,User.Image,Likes");

                var filteredPosts = allPosts.Where(p => (p.Title.Contains(query)
                || p.Content.Contains(query)) ||
                (p.Tags != null && p.Tags.Any(tag => tag.TagName == query)) ||
                p.Category.CategoryName.Contains(query)).ToList();

                return filteredPosts.Any() ? PartialView("_Post", filteredPosts) : PartialView("_NoPostFound");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for posts with query {Query}.", query);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
