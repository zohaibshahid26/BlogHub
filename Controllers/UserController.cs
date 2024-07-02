using BlogHub.UnitOfWork;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using BlogHub.ViewModels;

namespace BlogHub.Controllers
{
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserController> _logger;

        public UserController(IUnitOfWork unitOfWork, ILogger<UserController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Profile(string? id)
        {
            try
            {
                if (id == null)
                {
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
                }

                var user = _unitOfWork.UserRepository.Get(filter: u => u.Id == id, includeProperties: "Image").FirstOrDefault();
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    return NotFound();
                }

                var posts = _unitOfWork.PostRepository.Get(filter: p => p.UserId == id, includeProperties: "Category,Tags,Image,Comments,Likes,User");
                ViewData["PostCount"] = posts.Count();
                ViewData["Engagement"] = posts.Sum(p => (p.Comments?.Count ?? 0) + (p.Likes?.Count ?? 0) + p.ViewCount);

                return View(new ProfileViewModel { User = user, Posts = posts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading profile for user with ID {UserId}.", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
