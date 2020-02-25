using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Controllers
{
    [Route("books")]
    public class BookController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        public BookController(IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }
            
        public async Task<IActionResult> Index() =>            
            View(await _bookRepository.GetAll().Include(book => book.Author).ToListAsync());        

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var vm = new BookViewModel { Book = new Book(), Author = await _authorRepository.GetAll().ToListAsync()};
            return View(vm);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(BookViewModel bvm)
        {
            await _bookRepository.CreateAsync(new Book { AuthorId = bvm.Book.AuthorId, Description = bvm.Book.Description, Name = bvm.Book.Name, YearOfPublication = bvm.Book.YearOfPublication});
            return RedirectToAction("Index");
        }
    }
}