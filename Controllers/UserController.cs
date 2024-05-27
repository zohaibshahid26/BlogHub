using BlogHub.UnitOfWork;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
namespace BlogHub.Controllers
{
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Profile(string ?id)
        {
            if (id == null)
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            }
            var user = _unitOfWork.UserRepository.Get(filter: u => u.Id == id,includeProperties:"Image").FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            var posts = _unitOfWork.PostRepository.Get(filter: p => p.UserId == id, includeProperties: "Category,Tags,Image,Comments,User");
            ViewData["PostCount"] = posts.Count();
            return View(user);
        }
    }
}
