using Library.Data;
using Library.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Services
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(LibraryContext libContext)
        : base(libContext) { }


        public async Task<Book> FindBookByName(string name) =>
            await GetAll().Where(p => p.Name.ToLower().Contains(name.ToLower())).FirstOrDefaultAsync();


        public async Task<List<Book>> GetBooksByAuthor(string authorName) =>
            await GetAll().Where(p => p.Author.Name == authorName).ToListAsync();


        public async Task<List<Book>> GetBooksByYear(int year) =>
            await GetAll().Where(p => p.YearOfPublication == year).ToListAsync();


    }
}
