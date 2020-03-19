using Library.Models.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Library.Services
{
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Поиск пользователя по логину и паролю
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns>Task<User></returns>
        Task<User> GetUserByLoginAndPassword(string login, string password);
        Task<User> GetUserByLogin(string login);
        bool DoesUserExist(string login);
        
    }
}