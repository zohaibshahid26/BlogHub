using BlogHub.Models;
using BlogHub.UnitOfWork;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlogHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var trendingPosts = _unitOfWork.PostRepository.Get(orderBy: q => q.OrderByDescending(p => p.Likes!.Count),includeProperties: "Category,Tags,Image,Comments,User,Likes").Take(6);
            var latestPosts = _unitOfWork.PostRepository.Get(orderBy: q => q.OrderByDescending(p => p.DatePosted), includeProperties: "Category,Tags,Image,Comments,User,Likes").Take(5);
            var categories = _unitOfWork.CategoryRepository.GetAllAsync();
            return View(latestPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
