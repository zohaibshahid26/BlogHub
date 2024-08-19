using Domain.Entities;

namespace Domain.Interfaces
{
    public interface ICommentService
    {
        Task AddCommentAsync(Comment comment);
        Task DeleteCommentAsync(int id);
        Task UpdateCommentAsync(Comment comment);
        Task<Comment?> GetCommentByIdAsync(int id);
        Comment? GetCommentWithDetails(int id, string includeProperties);
    }
}
