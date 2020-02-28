using Library.Data;
using Library.Models.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Services
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(LibraryContext libContext) : base(libContext)
        {

        }

        public async Task<User> GetUserByLoginAndPassword(string login, string password) => await GetAll().Where(p => p.Login == login && p.Password == password).FirstOrDefaultAsync();

        public bool DoesUserExist(string login) => GetAll().Where(p => p.Login == login).Any() ? true : false;

        public async Task<User> GetUserByLogin(string login) => await GetAll().Where(p => p.Login == login).FirstOrDefaultAsync();
    }
}
