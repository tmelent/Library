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
    [Route("authors")]
    public class AuthorController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        public AuthorController(IAuthorRepository authorRepository)
        {
            if (_authorRepository == null) 
                _authorRepository = authorRepository;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _authorRepository.GetAll().ToListAsync());
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> Create(Author author)
        {
            await _authorRepository.CreateAsync(author);
            return RedirectToAction("Index");
        }
    }
}