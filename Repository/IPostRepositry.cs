using BlogHub.ViewModels;
using BlogHub.Models;

namespace BlogHub.Repository
{
    public interface IPostRepository : IDisposable
    {
        Task<IEnumerable<Post>> GetPostsAsync();
        Task<Post?> GetPostByIdAsync(string id);
        Task AddPostAsync(PostViewModel post);
        Task UpdatePostAsync(Post post);
        Task DeletePostAsync(string id);
        Task SaveChangesAsync();
    }
}

