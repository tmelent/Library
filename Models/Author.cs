using Library.Services;
using System.Collections.Generic;

namespace Library.Models
{
    public class Author : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int YearOfBirth { get; set; }
        public int? YearOfDeath { get; set; }
        public List<Book> Books { get; set; }
        
    }
}
