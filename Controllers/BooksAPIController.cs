using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Library.Controllers
{
    [Route("api/book")]
    public class BooksAPIController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        public BooksAPIController(IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }
        // GET: api/<controller>
        [HttpGet("getbyid/{id}")]
        public async Task<IEnumerable<string>> GetById(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            return new string[] { book.Name, book.Description, book.YearOfPublication.ToString() };
        }

        [HttpPost("getBooksInRange")]
        public async Task<IActionResult> GetBooksByRange([FromBody] ContentRange range) {
            var x = await _bookRepository.GetAll().Include(p => p.Author).Skip(range.First - 1).Take(range.Last - range.First).ToListAsync();
            List<BookResponse> list = new List<BookResponse>();
            foreach (var item in x)            
                list.Add(new BookResponse { BookTitle = item.Name, AuthorName = item.Author.Name, Description = item.Description, PublishYear = item.YearOfPublication });
            
            return Ok(list);
        }
    }
}
