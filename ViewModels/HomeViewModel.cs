using BlogHub.Models;

namespace BlogHub.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Post> ?LatestPosts { get; set; }
        public IEnumerable<Post> ?TrendingPosts { get; set; }
        public IEnumerable<Category>? Categories { get; set; }
        public IEnumerable<Post> ?RecentlyViewedPosts { get; set; }

    }
}
