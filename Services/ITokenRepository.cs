using Library.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Services
{
    public interface ITokenRepository : IGenericRepository<RefreshToken>
    {
        
    }
}
