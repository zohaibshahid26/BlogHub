using BlogHub.Models;
using BlogHub.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    [Authorize(Policy = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(IUnitOfWork unitOfWork, ILogger<CategoryController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Posts(string id)
        {
            try
            {
                var category = _unitOfWork.CategoryRepository.Get(filter: c => c.CategoryName == id).FirstOrDefault();
                if (category == null)
                {
                    _logger.LogWarning("Category with name {CategoryName} not found.", id);
                    return NotFound();
                }

                var posts = _unitOfWork.PostRepository.Get(filter: p => p.Category.CategoryName == id, includeProperties: "Category,User,Likes,User.Image,Image,Comments");
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
                var categories = await _unitOfWork.CategoryRepository.GetAllAsync();
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

                var existingCategory = _unitOfWork.CategoryRepository.Get(filter: c => c.CategoryName == category.CategoryName).FirstOrDefault();
                if (existingCategory == null)
                {
                    await _unitOfWork.CategoryRepository.AddAsync(category);
                    await _unitOfWork.SaveChangesAsync();
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
                await _unitOfWork.CategoryRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
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
                var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                    return NotFound();
                }

                _unitOfWork.CategoryRepository.Update(category);
                await _unitOfWork.SaveChangesAsync();
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
