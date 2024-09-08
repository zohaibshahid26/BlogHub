using Domain.Entities;

namespace Web.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Post> ?LatestPosts { get; set; }
        public IEnumerable<Post> ?TrendingPosts { get; set; }
        public IEnumerable<Category>? Categories { get; set; }
        public IEnumerable<Post> ?RecentlyViewedPosts { get; set; }

        public int PageNumber { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; }


    }
}
