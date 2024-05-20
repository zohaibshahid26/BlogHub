using BlogHub.Data;
using BlogHub.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogHub.Repository
{
    public class CommentRepository : ICommentRepository, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public CommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddCommentAsync(Comment comment)
        {
            ArgumentNullException.ThrowIfNull(comment);
            comment.Post = await _context.Posts.FindAsync(comment.PostId);
            comment.User = await _context.Users.FindAsync(comment.UserId);
            await _context.Comments.AddAsync(comment);
        }

        public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(string postId)
        {
            ArgumentNullException.ThrowIfNull(postId);
            return await _context.Comments.Where(c => c.PostId == postId).ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            ArgumentNullException.ThrowIfNull(commentId);
            return await _context.Comments.
                Include(c => c.Post).
                ThenInclude(c => c!.User).
                FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public void UpdateComment(Comment comment)
        {
            ArgumentNullException.ThrowIfNull(comment);
            _context.Comments.Update(comment);
        }

        public async  Task DeleteCommentAsync(int commentId)
        {
            ArgumentNullException.ThrowIfNull(commentId);
            var comment = await _context.Comments.FindAsync(commentId);
            ArgumentNullException.ThrowIfNull(comment);
            _context.Comments.Remove(comment);
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
