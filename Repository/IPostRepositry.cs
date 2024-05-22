using BlogHub.Models;
using BlogHub.ViewModels;

namespace BlogHub.Repository
{
    public interface IPostRepository : IGenericRepository<Post>
    {
        Task AddPostAsync(PostViewModel post);
        Task UpdatePost(PostViewModel post);
        Task ToggleLikeAsync(string postId, string userId);
        void RemovePostImage(string imageUrl);
    }
}