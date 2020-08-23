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

        [HttpGet("getBookById/{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var item = await _bookRepository.GetByIdAsync(id);
            var author = await _authorRepository.GetByIdAsync(item.AuthorId);
            return Ok(new BookResponse
            {
                BookId = item.Id,
                BookTitle = item.Name,
                AuthorName = author.Name,
                Description = item.Description,
                PublishYear = item.YearOfPublication
            });
        }

        [HttpPost("getAllBooks")]
        public async Task<IActionResult> GetAllBooks() =>
            Ok(await _bookRepository
                   .GetAll()
                   .Include(p => p.Author)
                   .Select(item => new BookResponse
                   {
                       BookId = item.Id,
                       BookTitle = item.Name,
                       AuthorName = item.Author.Name,
                       Description = item.Description,
                       PublishYear = item.YearOfPublication
                   })
                   .ToListAsync());

        [HttpPost("getBooksInRange")]
        public IActionResult GetBooksByRange([FromBody] ContentRange range) =>
            Ok(_bookRepository
                .GetAll()
                .Include(p => p.Author)
                .Skip(range.First - 1)
                .Take(range.Last)
                .Select(item => new BookResponse
                {
                    BookId = item.Id,
                    BookTitle = item.Name,
                    AuthorName = item.Author.Name,
                    Description = item.Description,
                    PublishYear = item.YearOfPublication
                }));

    }
}
