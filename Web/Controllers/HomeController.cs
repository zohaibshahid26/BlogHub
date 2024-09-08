using Web.Models;
using Web.ViewModels;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, IPostService postService, ICategoryService categoryService, IMemoryCache cache)
        {
            _logger = logger;
            _postService = postService;
            _categoryService = categoryService;
            _cache = cache;
        }

        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Client, VaryByQueryKeys = new[] { "pageNumber", "pageSize" })]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                var trendingPosts = _cache.GetOrCreate("TrendingPosts", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return _postService.GetTrendingPosts(6);
                });

                var latestPosts = _cache.GetOrCreate($"LatestPosts_{pageNumber}_{pageSize}", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return _postService.GetLatestPosts(pageNumber, pageSize);
                });

                var categories = await _cache.GetOrCreateAsync("Categories", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return await _categoryService.GetAllCategoriesAsync();
                });

                var recentlyViewedPosts = Request.Cookies["RecentlyViewedPosts"];
                var recentlyViewedPostIds = recentlyViewedPosts != null ? recentlyViewedPosts.Split(',').ToList() : new List<string>();
                var recentlyViewedPostDetails = _postService.GetPostsByIds(recentlyViewedPostIds);

                var totalPostsCount = _cache.GetOrCreate("TotalPostsCount", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                    return _postService.GetTotalPostsCount();
                });

                var totalPageCount = (int)Math.Ceiling((double)totalPostsCount / pageSize);

                var homeViewModel = new HomeViewModel
                {
                    TrendingPosts = trendingPosts,
                    LatestPosts = latestPosts,
                    Categories = categories,
                    RecentlyViewedPosts = recentlyViewedPostDetails,
                    PageNumber = pageNumber,
                    TotalPages = totalPageCount,
                    PageSize = pageSize
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