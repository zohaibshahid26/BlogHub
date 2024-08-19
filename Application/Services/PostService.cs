using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Post> GetPostsByUser(string userId)
        {
            return _unitOfWork.PostRepository.Get(filter: p => p.UserId == userId, includeProperties: "Category,Tags,Image,Likes,Comments,User");
        }

        public Post? GetPostDetails(string id, string includeProperties)
        {
            return _unitOfWork.PostRepository.Get(filter: p => p.PostId == id, includeProperties: includeProperties).FirstOrDefault();
        }
        public string? GetCurrentUserImageUrl(string? userId)
        {
            return _unitOfWork.UserRepository.Get(filter: u => u.Id == userId, includeProperties: "Image").FirstOrDefault()?.Image?.ImageURL;
        }

        public IEnumerable<Post> GetPostsByCategory(string categoryName, string includeProperties)
        {
            return _unitOfWork.PostRepository.Get(filter: p => p.Category.CategoryName == categoryName, includeProperties: includeProperties);
        }

        public async Task IncrementViewCountAsync(Post post)
        {
            post.ViewCount++;
            _unitOfWork.PostRepository.Update(post);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.CategoryRepository.GetAllAsync();
        }

        public async Task AddPostAsync(Post post)
        {
            await _unitOfWork.PostRepository.AddAsync(post);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdatePostAsync(Post post)
        {
            _unitOfWork.PostRepository.Update(post);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeletePostAsync(string id)
        {
            await _unitOfWork.PostRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ToggleLikeAsync(string postId, string userId)
        {
            await _unitOfWork.PostRepository.ToggleLikeAsync(postId, userId);
            await _unitOfWork.SaveChangesAsync();
        }

        public IEnumerable<Post> SearchPosts(string query)
        {
            var allPosts = _unitOfWork.PostRepository.Get(includeProperties: "Category,Tags,Image,Comments,User,User.Image,Likes");
            return allPosts.Where(p => (p.Title.Contains(query) || p.Content.Contains(query)) ||
                                       (p.Tags != null && p.Tags.Any(tag => tag.TagName == query)) ||
                                       p.Category.CategoryName.Contains(query)).ToList();
        }

        public IEnumerable<Post> GetTrendingPosts(int count)
        {
            return _unitOfWork.PostRepository.Get(
                orderBy: q => q.OrderByDescending(p => p.Comments!.Count + p.Likes!.Count + p.ViewCount),
                includeProperties: "Category,Tags,Image,Comments,User,Likes,User.Image"
            ).Take(count);
        }

        public IEnumerable<Post> GetLatestPosts(int count)
        {
            return _unitOfWork.PostRepository.Get(
                orderBy: q => q.OrderByDescending(p => p.DatePosted),
                includeProperties: "Category,Tags,Image,Comments,User,Likes,User.Image"
            ).Take(count);
        }

        public IEnumerable<Post> GetPostsByIds(IEnumerable<string> postIds)
        {
            return _unitOfWork.PostRepository.Get(
                filter: p => postIds.Contains(p.PostId),
                includeProperties: "Category,Tags,Image,Comments.User,User,Likes,User.Image"
            ).ToList();
        }
        public void RemovePostImage(string url)
        {
            _unitOfWork.ImageRepository.RemoveImage(url);
        }
        public async Task DeletePostImageAsync(int imageId)
        {
            await _unitOfWork.ImageRepository.DeleteAsync(imageId);
        }

        public async Task SaveChangesAsync()
        {
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> SavePostImageAsync(IFormFile image, string folder)
        {
            return await _unitOfWork.ImageRepository.SaveImageAsync(image, folder);

        }
        public Tag GetPostTag(string tagName)
        {
          return  _unitOfWork.TagRepository.Get(filter: t => t.TagName == tagName).FirstOrDefault() ?? new Tag { TagName = tagName };
        }

    }
}