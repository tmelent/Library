using Library.Services;

namespace Library.Models
{
    public class Book : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }       
        public int YearOfPublication { get; set; }
        public string Description { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
    }
}
