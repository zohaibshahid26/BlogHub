using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize(Policy = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;

        public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService,IPostService postService)
        {
            _logger = logger;
            _categoryService = categoryService;
            _postService = postService;
        }

        [AllowAnonymous]
        public IActionResult Posts(string id)
        {
            try
            {
                var category = _categoryService.GetCategoryByName(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with name {CategoryName} not found.", id);
                    return NotFound();
                }

                var posts = _postService.GetPostsByCategory(id, "Category,User,Likes,User.Image,Image,Comments");
                ViewBag.CategoryName = id;
                return View(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching posts for category {CategoryName}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> Manage()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching categories.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            try
            {
                if (category == null)
                {
                    _logger.LogWarning("Attempted to add a null category.");
                    return BadRequest();
                }

                var existingCategory = _categoryService.GetCategoryByName(category.CategoryName);
                if (existingCategory == null)
                {
                    await _categoryService.AddCategoryAsync(category);
                    _logger.LogInformation("Category {CategoryName} added successfully.", category.CategoryName);
                }
                else
                {
                    _logger.LogInformation("Category {CategoryName} already exists.", category.CategoryName);
                }

                return RedirectToAction("Manage", "Category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding category {CategoryName}.", category.CategoryName);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                _logger.LogInformation("Category with ID {CategoryId} deleted successfully.", id);
                return RedirectToAction("Manage", "Category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting category with ID {CategoryId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                    return NotFound();
                }

                await _categoryService.UpdateCategoryAsync(category);
                _logger.LogInformation("Category with ID {CategoryId} updated successfully.", id);
                return RedirectToAction("Manage", "Category");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating category with ID {CategoryId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
