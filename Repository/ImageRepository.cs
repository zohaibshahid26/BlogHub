using BlogHub.Data;
using BlogHub.Models;
namespace BlogHub.Repository
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
            if (!imageUrl.Contains("default_image.png"))
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
