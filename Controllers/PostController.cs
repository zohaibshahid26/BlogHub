﻿using BlogHub.Models;
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

        [Route("/Posts")]
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            var posts = _unitOfWork.PostRepository.Get(filter: p => p.UserId == userId, includeProperties: "Category,Tags,Image,Comments,User");
            return View(posts);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            Post? post = _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: "Category,Tags,Image,Comments.User,User,Comments.User.Image,User.Image,Likes").FirstOrDefault();
            if (post == null)
            {
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
                    Image = new Image { ImageURL = await _unitOfWork.ImageRepository.SaveImageAsync(post.Image,"featureImages") }
                };
                await _unitOfWork.PostRepository.AddAsync(Post);
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
                Categories = await _unitOfWork.CategoryRepository.GetAllAsync()
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
            var postToUpdate = _unitOfWork.PostRepository.Get(filter: p => p.PostId == post.PostId, includeProperties: "Tags,User,Image").FirstOrDefault();
            if (postToUpdate == null)
            {
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
                    postToUpdate.Image = new Image { ImageURL = await _unitOfWork.ImageRepository.SaveImageAsync(post.Image,"featureImages")};
                }
                _unitOfWork.PostRepository.Update(postToUpdate);
                await _unitOfWork.SaveChangesAsync();
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
                _unitOfWork.ImageRepository.RemoveImage(post.Image.ImageURL);
                await _unitOfWork.ImageRepository.DeleteAsync(post.Image.ImageId);
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

        [AllowAnonymous]
        public IActionResult Search(string ?query)
        {
            if (query == null)
            {
                return View(new List<Post>());
            }

            var allPosts = _unitOfWork.PostRepository.Get(
            includeProperties: "Category,Tags,Image,Comments,User,User.Image,Likes");

            var filteredPosts = allPosts
            .Where(p =>(p.Title.Contains(query) || p.Content.Contains(query)) || (p.Tags != null && p.Tags.Any(tag => tag.TagName == query)) || p.Category.CategoryName.Contains(query)).ToList();

            TempData["SearchQuery"] = query;
            return View(filteredPosts);
        }
    }
}