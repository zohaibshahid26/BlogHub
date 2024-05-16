using BlogHub.Models;
using BlogHub.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BlogHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostRepository _postRepositry;

        public HomeController(ILogger<HomeController> logger, IPostRepository postRepository)
        {
            _logger = logger;
            _postRepositry = postRepository;
        }

        public async Task<IActionResult> Index()
        {
            var latestPosts = await _postRepositry.GetLatestPostAsync();
            //var categories = await _postRepositry.GetCategories();
            //var trendingPosts = await _postRepositry.GetTrendingPostAsync();
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
