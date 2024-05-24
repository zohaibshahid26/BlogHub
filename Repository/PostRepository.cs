using BlogHub.Data;
using BlogHub.Models;
using Microsoft.EntityFrameworkCore;
namespace BlogHub.Repository
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public PostRepository(ApplicationDbContext context) : base(context)
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
        }

    }
}
