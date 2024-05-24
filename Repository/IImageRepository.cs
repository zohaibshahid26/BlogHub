using BlogHub.Models;
namespace BlogHub.Repository
{
    public interface IImageRepository : IGenericRepository<Image>
    {
        void RemoveImage(string imageUrl);
        Task<string> SaveImageAsync(IFormFile? image, string folderName);
    }
}
