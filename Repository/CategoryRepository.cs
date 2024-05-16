using BlogHub.Data;
using BlogHub.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogHub.Repository
{
    public class CategoryRepository : ICategoryRepository, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category?>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            ArgumentNullException.ThrowIfNull(id);
            return await _context.Categories.FindAsync(id);
        }

        public async Task AddCategoryAsync(Category category)
        {
            ArgumentNullException.ThrowIfNull(category);
             await  _context.Categories.AddAsync(category);
        }

        public void UpdateCategoryAsync(Category ?category)
        {
            ArgumentNullException.ThrowIfNull(category);
            _context.Categories.Update(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            ArgumentNullException.ThrowIfNull(id);
            var category = await _context.Categories.FindAsync(id);
            ArgumentNullException.ThrowIfNull(category);
            _context.Categories.Remove(category);
        }
        public async Task SaveChangesAsync()
        {
             await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

    }
}
