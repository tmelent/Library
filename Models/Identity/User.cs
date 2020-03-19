using Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Models.Identity
{
    public class User : IEntity
    {
        public User()
        {
            RefreshTokens = new HashSet<RefreshToken>();
        }
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } = "user";
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        

    }
}
