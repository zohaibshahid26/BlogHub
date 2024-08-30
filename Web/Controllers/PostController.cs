using Domain.Entities;
using Domain.Interfaces;
using Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Web.Hubs;

namespace Web.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<PostController> _logger;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<ChatHub> _hubContext;

        public PostController(IAuthorizationService authorizationService, ILogger<PostController> logger, IPostService postService, ICategoryService categoryService, IHubContext<ChatHub> hubContext)
        {
            _authorizationService = authorizationService;
            _logger = logger;
            _postService = postService;
            _categoryService = categoryService;
            _hubContext = hubContext;
        }
       

        [Route("/Posts")]
        public IActionResult Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
                var posts = _postService.GetPostsByUser(userId);
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
                var post = _postService.GetPostDetails(id, "Category,Tags,Image,Comments.User,User,Comments.User.Image,User.Image,Likes");
                if (post == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found.", id);
                    return NotFound();
                }

                string currentUserImageUrl = string.Empty;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                    currentUserImageUrl = _postService.GetCurrentUserImageUrl(userId) ?? string.Empty;
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

                await _postService.IncrementViewCountAsync(post);

                return View( new PostDetailsViewModel { Post = post });
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
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(new PostViewModel { Categories = categories });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the add post view.");
                return StatusCode(500, "Internal server error");
            }
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Add(PostViewModel post)
        {
            if(post.Category?.CategoryName == "null")
            {
                post.Category = null;
                ModelState.AddModelError("Category", "Category is required");
            }

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
                            var tag = _postService.GetPostTag(tagName);
                            tags.Add(tag);
                        }
                    }
                    var imageUrl = await _postService.SavePostImageAsync(post.Image, "featureImages");
                    var Post = new Post
                    {
                        Title = post.Title,
                        Content = post.Content,
                        Tags = tags.ToList(),
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous",
                        CategoryId = _categoryService.GetCategoryByName(post.Category.CategoryName)!.CategoryId,
                        Image = new Image { ImageURL = await _postService.SavePostImageAsync(post.Image, "featureImages") }
                    };

                    await _postService.AddPostAsync(Post);
                    await _hubContext.Clients.All.SendAsync("ReceiveMessage", Post.Title, post.Category?.CategoryName, Post.Image.ImageURL, "/Post/Details/" + Post.PostId);
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while adding a new post.");
                    return Json(new { success = false, message = "An error occurred." });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, errors });
            
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
                var post = _postService.GetPostDetails(id, "Category,Tags,Image,Comments,User,Likes");
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
                    Categories = await _categoryService.GetAllCategoriesAsync()
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

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Edit(PostViewModel? post)
        {
            try
            {
                if (post == null)
                {
                    _logger.LogWarning("Edit post called with null post.");
                    return NotFound();
                }


                var postToUpdate = _postService.GetPostDetails(post?.PostId ?? string.Empty , "Tags,User,Image");
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
                    postToUpdate.CategoryId = _categoryService.GetCategoryByName(post.Category.CategoryName)!.CategoryId;

                    var tagNames = post.Tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToList();
                    if (tagNames != null)
                    {
                        var existingTags = postToUpdate.Tags ?? new List<Tag>();
                        var newTags = new List<Tag>();
                        foreach (var tagName in tagNames)
                        {
                            var tag = _postService.GetPostTag(tagName);
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
                            _postService.RemovePostImage(postToUpdate.Image.ImageURL);
                            await _postService.DeletePostImageAsync(imageId);
                        }
                        postToUpdate.Image = new Image { ImageURL = await _postService.SavePostImageAsync(post.Image, "featureImages") };
                    }

                  await _postService.UpdatePostAsync(postToUpdate);
                    return RedirectToAction("Index", "Post");
                }

                return View(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating post with ID {PostId}.", post?.PostId);
                return StatusCode(500, "Internal server error");
            }
        }


        [ValidateAntiForgeryToken]
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
                var post = _postService.GetPostDetails(id, "Category,Tags,Image,Comments,User,Likes");
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

                await _postService.DeletePostAsync(id);
                if (post.Image != null)
                {
                    _postService.RemovePostImage(post.Image.ImageURL);
                    await _postService.DeletePostImageAsync(post.Image.ImageId);
                }

                await _postService.SaveChangesAsync();
                return RedirectToAction("Index", "Post");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting post with ID {PostId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }


        [ValidateAntiForgeryToken]
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
                await _postService.ToggleLikeAsync(postId, userId);
                var postLikes = _postService.GetPostDetails(postId, "Likes")?.Likes;
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
            if(Request.Headers["X-Requested-With"] != "XMLHttpRequest")
            {
                return NotFound();
            }
            try
            {
                TempData["SearchQuery"] = query;
                if (query == null)
                {
                    return PartialView("_NoPostFound");
                }
                var filteredPosts = _postService.SearchPosts(query);
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