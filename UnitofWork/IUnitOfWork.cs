using BlogHub.Models;
using BlogHub.Repository;

namespace BlogHub.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IPostRepository PostRepository { get; }
        IGenericRepository<Comment> CommentRepository { get; }
        IGenericRepository<Category> CategoryRepository { get; }
        IGenericRepository<Tag> TagRepository { get; }
        IImageRepository ImageRepository { get; }
        IGenericRepository<MyUser> UserRepository { get; }
        Task SaveChangesAsync();
    }
}
