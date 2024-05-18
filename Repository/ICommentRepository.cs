using BlogHub.Helper;
using BlogHub.Models;

namespace BlogHub.Repository
{
    public interface ICommentRepository : IDisposable
    {
        Task AddCommentAsync(Comment comment);
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(string postId);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        void UpdateComment(Comment comment);
        Task DeleteCommentAsync(int commentId);
        Task SaveChangesAsync();

    }

}
