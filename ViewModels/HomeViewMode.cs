using BlogHub.Models;

namespace BlogHub.ViewModels
{
    public class HomeViewMode
    {
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<Category> Categories { get; set; }
    }
}
