using BlogHub.Data;
using BlogHub.Models;
using BlogHub.Repository;

namespace BlogHub.UnitofWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public IPostRepository PostRepository { get; private set; }
        public IGenericRepository<Comment> CommentRepository { get; private set; }
        public IGenericRepository<Category> CategoryRepository { get; private set; }
        private bool _disposed = false;


        public UnitOfWork(ApplicationDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            PostRepository = new PostRepository(_context, _env, _httpContextAccessor);
            CategoryRepository = new GenericRepository<Category>(_context);
            CommentRepository = new GenericRepository<Comment>(_context);
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
