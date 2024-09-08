using Web.Models;
using Web.ViewModels;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, IPostService postService, ICategoryService categoryService)
        {
            _logger = logger;
            _postService = postService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var trendingPosts = _postService.GetTrendingPosts(6);
                var latestPosts = _postService.GetLatestPosts(10);
                var categories = await _categoryService.GetAllCategoriesAsync();
                var recentlyViewedPosts = Request.Cookies["RecentlyViewedPosts"];
                var recentlyViewedPostIds = recentlyViewedPosts != null ? recentlyViewedPosts.Split(',').ToList() : new List<string>();
                var recentlyViewedPostDetails = _postService.GetPostsByIds(recentlyViewedPostIds);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the home page.");
                return StatusCode(500, "Internal server error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("An error occurred. Request ID: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = requestId });
        }

        [Route("/Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            _logger.LogError("The following status code occurred: {StatusCode}", statusCode);
            if (statusCode == 404)
            {
                return View("NotFound");
            }
            return View("Error");
        }
    }
}
