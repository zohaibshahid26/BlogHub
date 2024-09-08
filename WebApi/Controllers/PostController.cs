using Domain.Entities;
using Domain.Interfaces;
using WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<PostController> _logger;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public PostController(IAuthorizationService authorizationService, ILogger<PostController> logger, IPostService postService, ICategoryService categoryService)
        {
            _authorizationService = authorizationService;
            _logger = logger;
            _postService = postService;
            _categoryService = categoryService;
        }

        [AllowAnonymous]
        [HttpGet("User/{userId}")]
        public IActionResult GetPosts(string userId)
        {
            try
            {
                var posts = _postService.GetPostsByUser(userId);

                // Map to PostDTO
                var postDtos = posts.Select(post => new PostDTO
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content,
                    DatePosted = post.DatePosted,
                    CategoryId = post.CategoryId,
                    CategoryName = post.Category.CategoryName,
                    Tags = post.Tags?.Select(tag => new TagDTO { TagId = tag.TagId, TagName = tag.TagName }).ToList(),
                    Image = post.Image != null ? new ImageDTO { ImageId = post.Image.ImageId, ImageURL = post.Image.ImageURL } : null,
                    Comments = post.Comments?.Select(comment => new CommentDTO
                    {
                        CommentId = comment.CommentId,
                        Content = comment.Content,
                        UserName = comment.User?.FirstName + " " + comment.User?.LastName,
                        DatePosted = comment.DatePosted
                    }).ToList(),
                    Likes = post.Likes?.Select(like => new LikeDTO { LikeId = like.LikeId, UserId = like.UserId, UserName = like.User?.FirstName + " " + like.User?.LastName }).ToList(),
                    TimeToRead = post.TimeToRead,
                    ViewCount = post.ViewCount
                }).ToList();

                return Ok(postDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching posts for the user.");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // GET: api/Post/{id}
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostDetails(string id)
        {
            try
            {
                var post = _postService.GetPostDetails(id, "Category,Tags,Image,Comments.User,User,Comments.User.Image,User.Image,Likes");
                if (post == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found.", id);
                    return NotFound(new { Message = $"Post with ID '{id}' not found." });
                }

                var postDto = new PostDTO
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content,
                    DatePosted = post.DatePosted,
                    CategoryId = post.CategoryId,
                    CategoryName = post.Category.CategoryName,
                    Tags = post.Tags?.Select(tag => new TagDTO { TagId = tag.TagId, TagName = tag.TagName }).ToList(),
                    Image = post.Image != null ? new ImageDTO { ImageId = post.Image.ImageId, ImageURL = post.Image.ImageURL } : null,
                    Comments = post.Comments?.Select(c => new CommentDTO
                    {
                        CommentId = c.CommentId,
                        Content = c.Content,
                        UserName = c.User?.FirstName + " " + c.User?.LastName,
                        DatePosted = c.DatePosted
                    }).ToList(),
                    Likes = post.Likes?.Select(l => new LikeDTO { LikeId = l.LikeId, UserId = l.UserId, UserName = l.User?.FirstName + " " + l.User?.LastName }).ToList(),
                    TimeToRead = post.TimeToRead,
                    ViewCount = post.ViewCount
                };

                await _postService.IncrementViewCountAsync(post);
                return Ok(postDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching post details for ID {PostId}.", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddPost([FromForm] PostAddDTO postAddDto)
        {
            if (postAddDto == null || string.IsNullOrWhiteSpace(postAddDto.CategoryName))
            {
                return BadRequest(new { Message = "Category is required." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tagNames = postAddDto.Tags?.Split(',').Select(t => t.Trim()).Distinct();
                    var tags = new HashSet<Tag>();
                    if (tagNames != null)
                    {
                        foreach (var tagName in tagNames)
                        {
                            var tag = _postService.GetPostTag(tagName);
                            tags.Add(tag);
                        }
                    }
                    var post = new Post
                    {
                        Title = postAddDto.Title,
                        Content = postAddDto.Content,
                        Tags = tags.ToList(),
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous",
                        CategoryId = _categoryService.GetCategoryByName(postAddDto.CategoryName)!.CategoryId,
                        Image =  new Image { ImageURL = await _postService.SavePostImageAsync(postAddDto.Image, "featureImages") }
                    };

                    await _postService.AddPostAsync(post);
                    return Ok(new { success = true, post = postAddDto });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while adding a new post.");
                    return StatusCode(500, new { success = false, message = "An error occurred." });
                }
            }

            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditPost(string id, [FromForm] PostEditDTO postEditDto)
        {
            if (postEditDto == null || postEditDto.PostId != id)
            {
                return BadRequest(new { Message = "Invalid post data." });
            }

            try
            {
                // Fetch the post details from the service
                var postToUpdate = _postService.GetPostDetails(id, "Tags,User,Image");
                if (postToUpdate == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found for updating.", id);
                    return NotFound(new { Message = $"Post with ID '{id}' not found." });
                }

                // Authorization check
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, postToUpdate, "EditPostPolicy");
                if (!authorizationResult.Succeeded)
                {
                    return Forbid();
                }

                // Update title, content, and category
                postToUpdate.Title = postEditDto.Title;
                postToUpdate.Content = postEditDto.Content;

                // Get category by name
                var category = _categoryService.GetCategoryByName(postEditDto.CategoryName);
                if (category == null)
                {
                    return BadRequest(new { Message = $"Category '{postEditDto.CategoryName}' not found." });
                }
                postToUpdate.CategoryId = category.CategoryId;

                // Handle tag updates
                var tagNames = postEditDto.Tags?.Split(',').Select(t => t.Trim()).Distinct().ToList();
                if (tagNames != null)
                {
                    var newTags = tagNames.Select(tagName => _postService.GetPostTag(tagName)).ToList();
                    postToUpdate.Tags = newTags;
                }

                // Handle image update if a new image is uploaded
                if (postEditDto.Image != null)
                {
                    if (postToUpdate.Image != null)
                    {
                        // Remove the old image
                        _postService.RemovePostImage(postToUpdate.Image.ImageURL);
                        await _postService.DeletePostImageAsync(postToUpdate.Image.ImageId);
                    }

                    // Save new image and assign to the post
                    postToUpdate.Image = new Image
                    {
                        ImageURL = await _postService.SavePostImageAsync(postEditDto.Image, "featureImages")
                    };
                }

                // Save changes
                await _postService.UpdatePostAsync(postToUpdate);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating post with ID {PostId}.", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }


        // DELETE: api/Post/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            try
            {
                var post = _postService.GetPostDetails(id, "Category,Tags,Image,Comments,User,Likes");
                if (post == null)
                {
                    _logger.LogWarning("Post with ID {PostId} not found for deletion.", id);
                    return NotFound(new { Message = $"Post with ID '{id}' not found." });
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

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting post with ID {PostId}.", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }


        [HttpPost("ToggleLike/{postId}")]
        public async Task<IActionResult> ToggleLike(string postId)
        {
            // Get the current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Toggle like called with null or empty post ID or user ID.");
                return BadRequest(new { success = false, message = "Invalid request." });
            }

            try
            {
                await _postService.ToggleLikeAsync(postId, userId);
                var postLikes = _postService.GetPostDetails(postId, "Likes")?.Likes;
                bool isLiked = postLikes?.Any(l => l.UserId == userId) ?? false;
                int likeCount = postLikes?.Count ?? 0;

                // Return the updated like status and count
                return Ok(new { success = true, isLiked, likeCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while toggling like for post with ID {PostId}.", postId);
                return StatusCode(500, new { success = false, message = "An error occurred." });
            }
        }

        [HttpGet("Search")]
        [AllowAnonymous]
        public IActionResult Search([FromQuery] string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { success = false, message = "Query cannot be empty." });
            }

            try
            {
                // Fetch the filtered posts based on the search query
                var filteredPosts = _postService.SearchPosts(query);
                if (!filteredPosts.Any())
                {
                    return Ok(new { success = false, message = "No posts found for the search query." });
                }

                // Map each Post to PostDTO
                var postDtos = filteredPosts.Select(post => new PostDTO
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content,
                    DatePosted = post.DatePosted,
                    CategoryId = post.CategoryId,
                    CategoryName = post.Category?.CategoryName ?? string.Empty,
                    Tags = post.Tags?.Select(tag => new TagDTO
                    {
                        TagId = tag.TagId,
                        TagName = tag.TagName
                    }).ToList(),
                    Image = post.Image != null ? new ImageDTO
                    {
                        ImageId = post.Image.ImageId,
                        ImageURL = post.Image.ImageURL
                    } : null,
                    Comments = post.Comments?.Select(comment => new CommentDTO
                    {
                        CommentId = comment.CommentId,
                        Content = comment.Content,
                        UserName = comment.User?.FirstName + " " + comment.User?.LastName,
                        DatePosted = comment.DatePosted
                    }).ToList(),
                    Likes = post.Likes?.Select(like => new LikeDTO
                    {
                        LikeId = like.LikeId,
                        UserId = like.UserId,
                        UserName = like.User?.FirstName + " " + like.User?.LastName
                    }).ToList(),
                    TimeToRead = post.TimeToRead,
                    ViewCount = post.ViewCount
                }).ToList();

                // Return the list of mapped PostDTOs
                return Ok(new { success = true, posts = postDtos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching for posts with query {Query}.", query);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }



    }
}
