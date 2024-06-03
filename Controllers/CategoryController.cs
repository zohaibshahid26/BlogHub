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
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public  IActionResult Posts(string id)
        {
            var category = _unitOfWork.CategoryRepository.Get(filter: c => c.CategoryName == id).FirstOrDefault();
            if (category == null)
            {
                return NotFound();
            }
            var posts = _unitOfWork.PostRepository.Get(filter: p => p.Category.CategoryName == id, includeProperties: "Category,User,Likes,User.Image,Image,Comments");
            ViewBag.CategoryName = id;
            return View(posts);

        }
        
        public async Task<IActionResult> Manage()
        {
            return View(await _unitOfWork.CategoryRepository.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Add(Category category)
        {
            if (category == null)
            {
                return NotFound();
            }
            var existingCategory = _unitOfWork.CategoryRepository.Get(filter: c => c.CategoryName == category.CategoryName).FirstOrDefault();
            if (existingCategory == null)
            {
                await _unitOfWork.CategoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();
            }
            return RedirectToAction("Manage", "Category");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitOfWork.CategoryRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Manage", "Category");
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            _unitOfWork.CategoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Manage", "Category");
        }
    }
}
