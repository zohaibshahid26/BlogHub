using BlogHub.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace BlogHub.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<TEntity> Get(
           Expression<Func<TEntity, bool>>? filter = null,
           Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
           string includeProperties = "")
        {
            IQueryable<TEntity> query= _context.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public async Task AddAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await _context.Set<TEntity>().AddAsync(entity);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _context.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(object id)
        {
            ArgumentNullException.ThrowIfNull(id);
            return await _context.Set<TEntity>().FindAsync(id);

        }

        public void Update(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _context.Set<TEntity>().Update(entity);
        }

        public async Task DeleteAsync(object id)
        {
            ArgumentNullException.ThrowIfNull(id);
            TEntity ?entityToDelete = await _context.Set<TEntity>().FindAsync(id);
            if (entityToDelete != null)
            {
                _context.Set<TEntity>().Remove(entityToDelete);
            }
        }

    }
}
