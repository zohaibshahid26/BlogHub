using BlogHub.Models;
using BlogHub.Repository;

namespace BlogHub.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IPostRepository PostRepository { get; }
        IGenericRepository<Comment> CommentRepository { get; }
        IGenericRepository<Category> CategoryRepository { get; }
        Task SaveChangesAsync();
    }
}
