using BlogHub.Data;
using BlogHub.Models;
using BlogHub.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogHub.Repository
{
    public class PostRepository : IPostRepository, IDisposable
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public PostRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
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
                            FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddPostAsync(PostViewModel post)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));

            if (post.Tags == null) throw new ArgumentNullException(nameof(post.Tags));
            var tagList = post.Tags.Split(',').ToList();


            var uniqueTags = new HashSet<Tag>();

            foreach (var tagName in tagList)
            {
                var tagExists = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName.Trim());
                if (tagExists == null)
                {
                    var newTag = new Tag { TagName = tagName.Trim() };
                    uniqueTags.Add(newTag);
                    await _context.Tags.AddAsync(newTag);
                }
                else
                {
                    uniqueTags.Add(tagExists);
                }
            }

            await _context.SaveChangesAsync();


            if (post.Image == null) throw new ArgumentNullException(nameof(post.Image));
            Image image = new Image
            {
                ImageData = await FileToByteArrayAsync(post.Image)
            };

            async Task<byte[]> FileToByteArrayAsync(IFormFile file)
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    return stream.ToArray();
                }
            }
            var imageExists = await _context.Images.FirstOrDefaultAsync(i => i.ImageData == image.ImageData);
            if (imageExists == null)
            {
                await _context.Images.AddAsync(image);
                await _context.SaveChangesAsync();
            }

            Category ?category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == post.CategoryName);
            if (category == null)
            {
                category = new Category
                {
                    CategoryName = post.CategoryName
                };
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
            }

            Post newPost = new Post
            {
                Title = post.Title,
                Content = post.Content,
                Category = category,
                Tags = uniqueTags.ToList(),
                Image = image,
                UserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous"
            };
            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(PostViewModel post)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));
            

        }

        public async Task DeletePostAsync(string id)
        {
            var post = await GetPostByIdAsync(id);
            if (post != null)
            {
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
