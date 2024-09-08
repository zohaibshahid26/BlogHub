using Domain.Entities;
using Web.Models;

namespace Web.ViewModels
{
    public class ProfileViewModel
    {
        public MyUser User { get; set; }
        public IEnumerable<Post> ?Posts { get; set; }
    }
}