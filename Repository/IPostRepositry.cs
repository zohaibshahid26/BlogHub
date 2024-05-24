using BlogHub.Models;

namespace BlogHub.Repository
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task ToggleLikeAsync(string postId, string userId);
    }
}