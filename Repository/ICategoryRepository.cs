using BlogHub.Models;

namespace BlogHub.Repository
{
    public interface ICategoryRepository : IDisposable
    {
        Task<IEnumerable<Category?>> GetCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int id);
        Task AddCategoryAsync(Category category);
        void UpdateCategoryAsync(Category ?category);
        Task DeleteCategoryAsync(int id);
        Task SaveChangesAsync();
    }
}
