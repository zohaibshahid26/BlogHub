using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task<bool> ToggleLikeAsync(string postId, string userId);
    }
}