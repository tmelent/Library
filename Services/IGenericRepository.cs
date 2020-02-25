using System.Linq;
using System.Threading.Tasks;

namespace Library.Services
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        Task<TEntity> GetByIdAsync(int id);

        Task CreateAsync(TEntity entity);

        Task Update(int id, TEntity entity);

        Task Delete(int id);
    }
}
