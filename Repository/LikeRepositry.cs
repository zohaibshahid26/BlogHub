using BlogHub.Data;
using BlogHub.Models;
using Microsoft.EntityFrameworkCore;
namespace BlogHub.Repository
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;
        public LikeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ToggleLikeAsync(string postId, string userId)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existingLike == null)
            {
                _context.Likes.Add(new Like { PostId = postId, UserId = userId });
            }
            else
            {
                _context.Likes.Remove(existingLike);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLikesCountForPostAsync(string postId)
        {
            return await _context.Likes.CountAsync(l => l.PostId == postId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}
