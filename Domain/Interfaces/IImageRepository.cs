using Domain.Entities;
using Microsoft.AspNetCore.Http;
namespace Domain.Interfaces
{
    public interface IImageRepository : IGenericRepository<Image>
    {
        void RemoveImage(string imageUrl);
        Task<string> SaveImageAsync(IFormFile? image, string folderName);
    }
}