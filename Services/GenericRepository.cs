using Library.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Services
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class, IEntity
    {
        private readonly LibraryContext _libContext;
        
        public GenericRepository(LibraryContext libContext)
        {
            _libContext = libContext;            
        }
        
        public IQueryable<TEntity> GetAll() =>
            _libContext.Set<TEntity>().AsNoTracking();

        public async Task<TEntity> GetByIdAsync(int id) =>
            await _libContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        public async Task CreateAsync(TEntity entity)
        {
            await _libContext.Set<TEntity>().AddAsync(entity);
            await _libContext.SaveChangesAsync();            
        }                  

        public async Task DeleteAsync(int id)
        {            
            _libContext.Set<TEntity>().Remove(await GetByIdAsync(id));
            await _libContext.SaveChangesAsync();
        }             

        public async Task UpdateAsync(TEntity entity)
        {
            _libContext.Set<TEntity>().Update(entity);
            await _libContext.SaveChangesAsync();
        }
    }
}
