using BlogHub.Models;

namespace BlogHub.ViewModels
{
    public class ProfileViewModel
    {
        public MyUser User { get; set; }
        public IEnumerable<Post> ?Posts { get; set; }
    }
}
