using BlogHub.Models;
using BlogHub.ViewModels;
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

        public async Task<IActionResult> Index()
        {
            var trendingPosts = _unitOfWork.PostRepository.Get(orderBy: q => q.OrderByDescending(p => p.Comments!.Count + p.Likes!.Count + p.ViewCount), includeProperties: "Category,Tags,Image,Comments,User,Likes,User.Image").Take(6);
            var latestPosts = _unitOfWork.PostRepository.Get(orderBy: q => q.OrderByDescending(p => p.DatePosted), includeProperties: "Category,Tags,Image,Comments,User,Likes,User.Image").Take(5);
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync();


            var recentlyViewedPosts = Request.Cookies["RecentlyViewedPosts"];
            var recentlyViewedPostIds = recentlyViewedPosts != null ? recentlyViewedPosts.Split(',').ToList() : new List<string>();

            var recentlyViewedPostDetails = _unitOfWork.PostRepository.Get(filter: p => recentlyViewedPostIds.Contains(p.PostId), includeProperties: "Category,Tags,Image,Comments.User,User,Likes,User.Image").ToList();

            var homeViewModel = new HomeViewModel
            {
                TrendingPosts = trendingPosts,
                LatestPosts = latestPosts,
                Categories = categories,
                RecentlyViewedPosts = recentlyViewedPostDetails
            };
            _logger.LogInformation("Home page visited");
            return View(homeViewModel);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            { 
                return View("NotFound");
            }
            return View("Error");
        }
    }
}