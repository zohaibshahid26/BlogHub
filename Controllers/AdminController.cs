using BlogHub.Models;
using BlogHub.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        public AdminController(ICategoryRepository categoryRepositry)
        {
            _categoryRepository = categoryRepositry;
        }
        public async Task<IActionResult> Categories()
        {
            return View(await _categoryRepository.GetCategoriesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            await _categoryRepository.AddCategoryAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return RedirectToAction("Categories", "Admin");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryRepository.DeleteCategoryAsync(id);
            await _categoryRepository.SaveChangesAsync();
            return RedirectToAction("Categories", "Admin");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            _categoryRepository.UpdateCategoryAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return RedirectToAction("Categories", "Admin");
        }


    }
}
