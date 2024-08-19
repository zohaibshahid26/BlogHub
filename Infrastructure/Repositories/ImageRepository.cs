using Microsoft.AspNetCore.Http;
using Domain.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Infrastructure.Data;
namespace Infrastructure.Repositories
{
    public class ImageRepository: GenericRepository<Image>, IImageRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        public ImageRepository(ApplicationDbContext context,IWebHostEnvironment env) : base(context)
        {
            _context = context;
            _env = env;
        }
        public async Task<string> SaveImageAsync(IFormFile? image,string folderName)
        {
            string imageFolder = Path.Combine(_env.WebRootPath, folderName);
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }
            if (image == null)
            {
                return Path.Combine(folderName, "default_image.png");
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "-" + image.FileName;
            string filePath = Path.Combine(imageFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return Path.Combine(folderName, uniqueFileName);
        }

        public void RemoveImage(string imageUrl)
        {
            ArgumentNullException.ThrowIfNull(imageUrl);
            if (!imageUrl.Contains("default_image.png") && !imageUrl.Contains("default_profile.jpg"))
            {
                var filePath = Path.Combine(_env.WebRootPath, imageUrl);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
    }
}