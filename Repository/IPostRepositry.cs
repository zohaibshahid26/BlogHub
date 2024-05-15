using BlogHub.ViewModels;
using BlogHub.Models;

namespace BlogHub.Repository
{
    public interface IPostRepository : IDisposable
    {
        Task<IEnumerable<Post?>> GetPostsAsync();
        Task<Post?> GetPostByIdAsync(string id);
        Task<Post?> GetPostByUserIdAsync(string id);
        Task AddPostAsync(PostViewModel post);
        Task UpdatePost(PostViewModel post);
        Task DeletePostAsync(string id);
        Task<IEnumerable<Category>> GetCategories();
        Task SaveChangesAsync();
    }
}

