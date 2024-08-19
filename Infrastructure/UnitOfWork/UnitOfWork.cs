using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Infrastructure.Data;

namespace Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public IPostRepository PostRepository { get; private set; }
        public IGenericRepository<Comment> CommentRepository { get; private set; }
        public IGenericRepository<Category> CategoryRepository { get; private set; }
        public IGenericRepository<Tag> TagRepository { get; private set; }
        public IImageRepository ImageRepository { get; private set; }
        public IGenericRepository<MyUser> UserRepository { get; private set; }
        private bool _disposed = false;

        public UnitOfWork(ApplicationDbContext context, IWebHostEnvironment env,
                      IPostRepository postRepository,
                      IGenericRepository<Category> categoryRepository,
                      IGenericRepository<Comment> commentRepository,
                      IGenericRepository<Tag> tagRepository,
                      IImageRepository imageRepository,
                      IGenericRepository<MyUser> userRepository)
        {
            _context = context;
            _env = env;
            PostRepository = postRepository;
            CategoryRepository = categoryRepository;
            CommentRepository = commentRepository;
            TagRepository = tagRepository;
            ImageRepository = imageRepository;
            UserRepository = userRepository;
        }
    

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
