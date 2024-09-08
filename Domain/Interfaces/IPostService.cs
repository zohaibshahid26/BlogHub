using Microsoft.AspNetCore.Http;
using Domain.Entities;
namespace Domain.Interfaces
{
    public interface IPostService
    {
        IEnumerable<Post> GetPostsByUser(string userId);
        Post? GetPostDetails(string id, string includeProperties);

        IEnumerable<Post> GetPostsByCategory(string categoryName, string includeProperties);
        string? GetCurrentUserImageUrl(string userId);
        Task IncrementViewCountAsync(Post post);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task AddPostAsync(Post post);
        Task UpdatePostAsync(Post post);
        Task DeletePostAsync(string id);
        Task ToggleLikeAsync(string postId, string userId);
        IEnumerable<Post> SearchPosts(string query);
        IEnumerable<Post> GetTrendingPosts(int count);
        IEnumerable<Post> GetLatestPosts(int pageNumber, int pageSize);

        int GetTotalPostsCount();

        IEnumerable<Post> GetPostsByIds(IEnumerable<string> postIds);

        void RemovePostImage(string url);
        Task DeletePostImageAsync(int id);

        Task<string> SavePostImageAsync(IFormFile image,string folder);

        Tag GetPostTag(string TagName);

        Task SaveChangesAsync();
    }
}