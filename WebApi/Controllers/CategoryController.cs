using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")] // Require Admin policy
    public class CategoryController : ControllerBase
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;

        public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService, IPostService postService)
        {
            _logger = logger;
            _categoryService = categoryService;
            _postService = postService;
        }

        // GET: api/category/{id}/posts
        [HttpGet("{id}/posts")]
        [AllowAnonymous]
        public IActionResult GetPostsByCategory(string id)
        {
            try
            {
                // Check if the category exists
                var category = _categoryService.GetCategoryByName(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with name {CategoryName} not found.", id);
                    return NotFound(new { Message = $"Category '{id}' not found." });
                }

                // Fetch the posts for this category using the PostService
                var posts = _postService.GetPostsByCategory(id, "Category,User,Likes,User.Image,Image,Comments,Tags");
                if (posts == null || !posts.Any())
                {
                    _logger.LogWarning("No posts found for category with ID {CategoryId}.", category.CategoryId);
                    return NotFound(new { Message = $"No posts found for category '{category.CategoryName}'." });
                }

                // Map the posts to PostDTO
                var postDtos = posts.Select(post => new PostDTO
                {
                    PostId = post.PostId,
                    Title = post.Title,
                    Content = post.Content,
                    DatePosted = post.DatePosted,
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
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
                        DatePosted = comment.DatePosted,
                        UserName = comment.User?.UserName ?? "Unknown",
                    }).ToList(),
                    Likes = post.Likes?.Select(like => new LikeDTO
                    {
                        LikeId = like.LikeId,
                        UserId = like.UserId,
                        UserName = like.User?.UserName ?? "Unknown"
                    }).ToList(),
                    TimeToRead = post.TimeToRead,
                    ViewCount = post.ViewCount
                }).ToList();

                return Ok(postDtos); // Return posts in JSON format
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching posts for category {CategoryName}.", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                // Fetch all categories
                var categories = await _categoryService.GetAllCategoriesAsync();

                // Map the categories to CategoryDTO
                var categoryDtos = categories.Select(category => new CategoryDTO
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName
                }).ToList();

                // Return the list of CategoryDTOs in JSON format
                return Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching categories.");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDTO categoryDto)
        {
            try
            {
                // Check if the CategoryDTO is null
                if (categoryDto == null)
                {
                    _logger.LogWarning("Attempted to add a null category.");
                    return BadRequest(new { Message = "Category data cannot be null" });
                }

                // Check if the CategoryName is null or empty
                if (string.IsNullOrWhiteSpace(categoryDto.CategoryName))
                {
                    _logger.LogWarning("Attempted to add a category with an empty name.");
                    return BadRequest(new { Message = "Category name cannot be null or empty" });
                }

                // Check if the category already exists
                var existingCategory = _categoryService.GetCategoryByName(categoryDto.CategoryName);
                if (existingCategory != null)
                {
                    _logger.LogInformation("Category {CategoryName} already exists.", categoryDto.CategoryName);
                    return Conflict(new { Message = $"Category '{categoryDto.CategoryName}' already exists." });
                }

                // Map CategoryDTO to Category entity
                var category = new Category
                {
                    CategoryName = categoryDto.CategoryName
                };

                // Add the new category
                await _categoryService.AddCategoryAsync(category);
                _logger.LogInformation("Category {CategoryName} added successfully.", category.CategoryName);

                return Ok(new { Message = $"Category '{category.CategoryName}' added successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding category {CategoryName}.", categoryDto.CategoryName);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }


        // DELETE: api/Category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            try
            {
                // Check if category exists before deletion
                var category = _categoryService.GetCategoryByName(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                    return NotFound(new { Message = $"Category with ID {id} not found." });
                }

                // Delete the category
                await _categoryService.DeleteCategoryAsync(category.CategoryId);
                _logger.LogInformation("Category with ID {CategoryId} deleted successfully.", id);

                return Ok(new { Message = $"Category with ID {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting category with ID {CategoryId}.", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // PUT: api/Category/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDTO categoryDto)
        {
            try
            {
                // Check if the CategoryDTO is null
                if (categoryDto == null)
                {
                    _logger.LogWarning("Attempted to update a category with null data.");
                    return BadRequest(new { Message = "Category data cannot be null" });
                }

                // Check if the CategoryName is null or empty
                if (string.IsNullOrWhiteSpace(categoryDto.CategoryName))
                {
                    _logger.LogWarning("Attempted to update a category with an empty name.");
                    return BadRequest(new { Message = "Category name cannot be null or empty" });
                }

                // Check if the category exists
                var existingCategory = await _categoryService.GetCategoryByIdAsync(id);
                if (existingCategory == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                    return NotFound(new { Message = $"Category with ID {id} not found." });
                }

                // Update the category with the new data from the DTO
                existingCategory.CategoryName = categoryDto.CategoryName;
                await _categoryService.UpdateCategoryAsync(existingCategory);

                _logger.LogInformation("Category with ID {CategoryId} updated successfully.", id);
                return Ok(new { Message = $"Category with ID {id} updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating category with ID {CategoryId}.", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

    }
}
