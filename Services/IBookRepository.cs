using Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Services
{
    /// <summary>
    /// Работа с таблицей книг
    /// </summary>
    public interface IBookRepository : IGenericRepository<Book>
    {        
        Task<List<Book>> GetBooksByAuthor(string authorName);
        Task<List<Book>> GetBooksByYear(int year);
        Task<Book> FindBookByName(string name);
    }
}
