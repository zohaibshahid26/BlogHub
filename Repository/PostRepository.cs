using BlogHub.Data;
using BlogHub.Models;
using Microsoft.EntityFrameworkCore;
namespace BlogHub.Repository
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public PostRepository(ApplicationDbContext context, IWebHostEnvironment env) : base(context)
        {
            _context = context;
            _env = env;
        }   

        public async Task<string> SaveImageAsync(IFormFile? image)
        {
            string imageFolder = Path.Combine(_env.WebRootPath, "featureImages");
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }
            if (image == null)
            {
                return Path.Combine("featureImages", "default_image.png");
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "-" + image.FileName;
            string filePath = Path.Combine(imageFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return Path.Combine("featureImages", uniqueFileName);
        }

        public void RemovePostImage(string imageUrl)
        {
            ArgumentNullException.ThrowIfNull(imageUrl);
            if(!imageUrl.Contains("default_image.png"))
            {
                var filePath = Path.Combine(_env.WebRootPath, imageUrl);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            } 
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
