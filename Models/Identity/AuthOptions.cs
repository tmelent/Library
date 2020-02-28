using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Library.Models.Identity
{
    public class AuthOptions
    {
        public const string ISSUER = "Library0101"; // издатель токена
        public const string AUDIENCE = "LibraryClient"; // потребитель токена
        const string KEY = "libToken!_26928594_qu0!!sqfdaxed195182";   // ключ для шифрации
        public const int LIFETIME = 1; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
