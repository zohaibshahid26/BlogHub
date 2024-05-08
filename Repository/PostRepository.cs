using BlogHub.Data;
using BlogHub.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogHub.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public PostRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Post>> GetPostsAsync()
        {
            return await _context.Posts.AsNoTracking().ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(string id)
        {
            return await _context.Posts.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddPostAsync(Post post)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));
            await _context.Posts.AddAsync(post);
        }

        public async Task UpdatePostAsync(Post post)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));
            _context.Posts.Update(post);
            await Task.CompletedTask;
        }

        public async Task DeletePostAsync(string id)
        {
            var post = await GetPostByIdAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await Task.CompletedTask;
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this._disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
