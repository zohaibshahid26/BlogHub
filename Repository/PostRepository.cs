using BlogHub.Data;
using BlogHub.Models;
using BlogHub.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogHub.Repository
{
    public class PostRepository : GenericRepository<Post>, IPostRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _disposed = false;

        public PostRepository(ApplicationDbContext context, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }   

        private async Task<string> SaveImageAsync(IFormFile? image)
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

        public async Task AddPostAsync(PostViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            model.ImageUrl = await SaveImageAsync(model.Image);
            var image = new Image
            {
                ImageURL = model.ImageUrl
            };

            var tags = model?.Tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct() ?? Enumerable.Empty<string>();

            // Retrieve existing tags or create new ones if they don't exist
            var uniqueTags = new HashSet<Tag>();
            foreach (var tagName in tags)
            {
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName)
                         ?? new Tag { TagName = tagName };
                uniqueTags.Add(tag);
            }

            var categoryId = _context.Categories.FirstOrDefault(c => c.CategoryName == model!.Category.CategoryName)!.CategoryId;

            var post = new Post
            {
                Title = model!.Title,
                Content = model.Content,
                Tags = uniqueTags.ToList(),
                UserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous",
                CategoryId = categoryId,
                Image = image
            };
            await _context.Posts.AddAsync(post);
        }

        public async Task UpdatePost(PostViewModel postViewModel)
        {
            ArgumentNullException.ThrowIfNull(postViewModel);
            var postToUpdate = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.PostId == postViewModel.PostId);
            var categoryId = _context.Categories.FirstOrDefault(c => c.CategoryName == postViewModel!.Category.CategoryName)!.CategoryId;

            if (postToUpdate != null)
            {
                postToUpdate.Title = postViewModel.Title;
                postToUpdate.Content = postViewModel.Content;
                postToUpdate.CategoryId = categoryId;

                // Handling tags
                var tagNames = postViewModel.Tags?.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct() ?? Enumerable.Empty<string>();
                postToUpdate.Tags!.Clear();
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName == tagName)
                             ?? new Tag { TagName = tagName };
                    postToUpdate.Tags.Add(tag);
                }
                if (postViewModel.Image != null)
                {
                    if (postToUpdate.Image != null)
                    {
                        var filePath = Path.Combine(_env.WebRootPath, postToUpdate.Image.ImageURL);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        _context.Images.Remove(postToUpdate.Image);
                    }
                    postToUpdate.Image = new Image { ImageURL = await SaveImageAsync(postViewModel.Image) };
                }

                _context.Posts.Update(postToUpdate);
            }
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
