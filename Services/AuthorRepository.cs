using Library.Data;
using Library.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Services
{
    public class AuthorRepository : GenericRepository<Author>, IAuthorRepository
    {
        
        public AuthorRepository(LibraryContext libContext)
        : base(libContext) { }

        
    }
}
