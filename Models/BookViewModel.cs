using Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models
{
    public class BookViewModel : IEntity
    {
        public int Id { get; set; }
        public Book Book { get; set; }
        public List<Author> Author { get; set; }
    }
}
