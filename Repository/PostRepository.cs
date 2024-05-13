using BlogHub.Data;
using BlogHub.Models;
using BlogHub.ViewModels;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Diagnostics;
using System.Security.Claims;

namespace BlogHub.Repository
{
    public class PostRepository : IPostRepository, IDisposable
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor  _httpContextAccessor;
        private bool _disposed = false;

        public PostRepository(ApplicationDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IEnumerable<Post>> GetPostsAsync()
        {

            return await _context.Posts.
                        Include(p => p.Category).
                        Include(p => p.Tags).
                        Include(p => p.Image).
                        Include(p => p.Comments).
                        Include(p => p.Tags).
                        Include(p => p.User).
                        ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(string id)
        {
            return await _context.Posts.
                            Include(p => p.Category).
                            Include(p => p.Tags).
                            Include(p => p.Image).
                            Include(p => p.Comments).
                            Include(p => p.Tags).
                            Include(p => p.User).
                            FirstOrDefaultAsync(p => p.PostId == id);
        }

        public async Task AddPostAsync(PostViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            model.ImageUrl = await SaveImageAsync(model.Image);
            var Image = new Image
            {
                ImageURL = model.ImageUrl
            };
            var Tags = model?.Tags?.Split(',').ToList() ?? null;
            var UniqueTags = new HashSet<Tag>();
            if (Tags!=null)
            { 
                foreach (var item in Tags)
                {
                    UniqueTags.Add(new Tag { TagName = item });
                }
            }
            var Category = new Category { CategoryName = model!.CategoryName };
            var post = new Post
            {
                Title = model!.Title,
                Content = model.Content,
                Tags = [.. UniqueTags],
                UserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous",
                Category = Category,
                Image = Image
            };
            await _context.Posts.AddAsync(post);
        }

        private async Task<string> SaveImageAsync(IFormFile ?image)
        {
            string imageFolder = Path.Combine(_env.WebRootPath, "featureImages");
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }

            if (image == null)
            {
                return Path.Combine(imageFolder, "default_image.png");
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "-" + image.FileName;
            string filePath = Path.Combine(imageFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return Path.Combine("featureImages", uniqueFileName);
        }
    

        public void UpdatePost(Post post)
        {
            ArgumentNullException.ThrowIfNull(post);

            _context.Posts.Update(post);
        }

        public async Task DeletePostAsync(string id)
        {
            var post = await GetPostByIdAsync(id);
            if (post?.Image != null)
            {
                // Delete image file from file system if needed
                var filePath = Path.Combine(_env.WebRootPath, post.Image.ImageURL);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                _context.Images.Remove(post.Image);
                _context.Posts.Remove(post);
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
