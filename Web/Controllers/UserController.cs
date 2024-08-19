using Domain.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;
namespace Web.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger,IPostService postService,IUserService userService)
        {
            _logger = logger;
            _postService = postService;
            _userService = userService;
        }
        public IActionResult Profile(string? id)
        {
            try
            {
                if (id == null)
                {
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
                }

                var user = _userService.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    return NotFound();
                }

                var posts = _postService.GetPostsByUser(id);
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