using BlogHub.Models;
using BlogHub.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogHub.Controllers
{
    [Authorize(Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IActionResult> Categories()
        {
            return View(await _unitOfWork.CategoryRepository.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (category == null)
            {
                return NotFound();
            }

            await _unitOfWork.CategoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Categories", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _unitOfWork.CategoryRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Categories", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Categories", "Admin");
        }

    }
}
