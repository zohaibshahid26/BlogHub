using BlogHub.Models;

namespace BlogHub.Repository
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<bool> ToggleLikeAsync(string postId, string userId);
    }
}