using BlogHub.Models;
using BlogHub.ViewModels;

namespace BlogHub.Repository
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task ToggleLikeAsync(string postId, string userId);
        void RemovePostImage(string imageUrl);
        Task<string> SaveImageAsync(IFormFile? image);
    }
}