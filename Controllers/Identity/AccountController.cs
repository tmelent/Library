using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Library.Data;
using Library.Models.Identity;
using Library.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Library.Controllers.Identity
{
    [Route("/api/account")]
    public class AccountController : Controller
    {
        // тестовые данные вместо использования базы данных

        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly LibraryContext _libraryContext;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IUserRepository userRepository, ITokenRepository tokenRepository, ILogger<AccountController> logger, LibraryContext libContext)
        {
            _userRepository = userRepository;
            _logger = logger;
            _tokenRepository = tokenRepository;
            _libraryContext = libContext;
        }

        #region HashSalting
        [HttpGet]
        public string HashPassword(string password)
        {
            byte[] salt; // Generating Salt
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            _logger.LogInformation("Password was hashed.");
            return Convert.ToBase64String(hashBytes);
        }
        [HttpGet]
        public async Task<bool> ComparePassword(string login, string password)
        {
            try
            {
                string savedPasswordHash = (await _userRepository.GetUserByLogin(login)).Password;
                /* Extract the bytes */
                byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
                /* Get the salt */
                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                /* Compute the hash on the password the user entered */
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);
                /* Compare the results */
                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 16] != hash[i])
                    {
                        _logger.LogInformation($"Password is incorrect for {login}.") ;
                        return false;
                    }
                _logger.LogInformation($"Password is correct for {login}");
                return true;
            }
            catch (NullReferenceException)
            {
                _logger.LogError($"User {login} was not found.");
                throw new Exception($"User {login} was not found.");
            }
        }
        #endregion
        [HttpPost("makeToken")]
        public async Task<string> MakeToken(string username)
        {
            var identity = await GetIdentity(username);
            if (identity == null)
            {
                _logger.LogError($"Incorrect password or login for {username}");
                throw new Exception("Incorrect password or login");
            }

            var now = DateTime.UtcNow;
            
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token(string username)
        {
            _logger.LogInformation($"Attempt to create Token for account {username} at {DateTime.UtcNow}");
            string encodedJwt;
            try
            {
                encodedJwt = await MakeToken(username);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            var user = await _userRepository.GetUserByLogin(username);

            _logger.LogInformation($"Token was created at {DateTime.UtcNow} for user {user.Login}.");

            return Ok(new TokenModel
            {
                AccessToken = encodedJwt,
                AccessExpirationDate = DateTime.UtcNow.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                RefreshToken = await GenerateRefreshToken(user),
                HttpStatusCode = System.Net.HttpStatusCode.OK
            });
        }

        [HttpGet]
        private async Task<ClaimsIdentity> GetIdentity(string username)
        {
            var person = await _userRepository.GetUserByLogin(username);
            if (person != null)
            {
                _logger.LogInformation($"User {username} was found.");
                var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, person.Login),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
                };
                ClaimsIdentity claimsIdentity =
                new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                    ClaimsIdentity.DefaultRoleClaimType);                      
                return claimsIdentity;
            }
            _logger.LogInformation($"User {username} was not found.");
            return null;
        }
        /// <summary>
        /// Механизм создания refresh-token
        /// </summary>
        /// <returns>Random refresh-token</returns>
        [HttpGet]
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                _logger.LogInformation($"Token was generated at {DateTime.Now}");
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Генерация нового токена с добавлением в БД 
        /// </summary>
        /// <param name="user"></param>
        /// <returns>RefreshToken {string Token, DateTimeOffset ExpirationDate}</returns>
        [HttpGet]
        public async Task<RefreshToken> GenerateRefreshToken(User user)
        {
            _logger.LogInformation($"Creating refresh_token for user {user.Login} at {DateTime.Now}");
            // Добавление токена в БД с привязкой к пользователю
            RefreshToken refreshToken = new RefreshToken()
            {
                UserId = user.Id,
                Token = GenerateRefreshToken(),
                Expiration = DateTime.UtcNow.AddHours(24)
            };
            await _tokenRepository.CreateAsync(refreshToken);

            return refreshToken;
        }

        /// <summary>
        /// Замена старого токена новым
        /// </summary>
        /// <param name="token">Старый access_token</param>
        /// <param name="refreshToken">refresh_token</param>
        /// <returns>ObjectResult {access_Token, refresh_Token}</returns>
        ///        


        [HttpPost("refreshtoken")]
        public async Task<IActionResult> Refresh([FromBody]RefreshTokenRequestModel requestModel)
        {
            
            var username = GetPrincipalFromExpiredToken(requestModel.OldAccessToken).Identity.Name;

            var user = await _userRepository.GetUserByLogin(username);
            var savedRefreshToken = _libraryContext.RefreshTokens
                .Where(x => x.User.Login == username && x.Token == requestModel.OldRefreshToken)
                .Select(x => x.Token)
                .FirstOrDefault();                

            if (savedRefreshToken != requestModel.OldRefreshToken)
                throw new SecurityTokenException("Invalid refresh token");
            
            _logger.LogInformation($"Attempt to refresh token for user {username} at {DateTime.Now}");            
            await _tokenRepository.DeleteAsync(_tokenRepository.GetAll().
                Where(p => p.Token == requestModel.OldRefreshToken && p.UserId == user.Id)
                .Select(p => p.Id)
                .FirstOrDefault());                      
            
            _logger.LogInformation($"{username} was updated in database at {DateTime.Now} with new refresh-token");
            return await Token(username);
        }
        
        /// <summary>
        /// Получение пользователя по истекшему токену
        /// </summary>
        /// <param name="token">Старый access_token</param>
        /// <returns></returns>
        [HttpGet]
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        [HttpGet]
        private ClaimsPrincipal GetPrincipalFromAccessToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, 
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                ValidateLifetime = true // Токен должен быть не просроченным
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, 
                StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        #region Authentication
        /// <summary>
        /// Создание нового аккаунта
        /// </summary>
        /// <param name="model">Модель аккаунта для регистрации - RegisterModel</param>
        /// <returns></returns>
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                bool userExists = _userRepository.DoesUserExist(model.Login);
                if (!userExists)
                {
                    // добавляем пользователя в бд

                    User user = new User
                    {
                        Login = model.Login,
                        Password = HashPassword(model.Password),
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber
                    };

                    await _userRepository.CreateAsync(user);

                    //await Authenticate(user, false); // аутентификация

                    return Ok($"User {model.Login} was created successfully");
                }
                return BadRequest("Invalid username or password.");
            }
            return BadRequest("Model is not valid.");
        }

        /// <summary>
        /// Вход в аккаунт без аутентификации
        /// </summary>
        /// <param name="model">Модель для входа в аккаунт - LoginModel</param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody]LoginModel model)
        {
            _logger.LogInformation($"Attempt to log in account {model.Login} at {DateTime.UtcNow}");
            User user = await _userRepository.GetUserByLogin(model.Login);
            if (user != null)
            {
                if (await ComparePassword(model.Login, model.Password))
                {
                    _logger.LogInformation("Creating token...");

                    return await Token(model.Login);
                }
                _logger.LogInformation("Incorrect password.");
                return BadRequest("Incorrect login or password.");
            }
            return BadRequest("Incorrect login or password.");
        }

        [HttpPost("logout1")]
        public async Task<IActionResult> Logout(string refreshToken, string accessToken)
        {
            
            var username = GetPrincipalFromExpiredToken(accessToken).Identity.Name;
            var user = await _userRepository.GetUserByLogin(username);
            _logger.LogInformation($"Attempt to log out from account {username} at {DateTime.Now}. Removing token...");
            var token = user.RefreshTokens.ToList().Find(p => p.Token == refreshToken);
            _logger.LogInformation($"Token for user {username} found. Removing...");
            if (token != null)
            {
                user.RefreshTokens.Remove(token);
                await _userRepository.UpdateAsync(user);
                return Ok();
            }
            _logger.LogError($"Invalid token for user {username}");
            throw new SecurityTokenException("Invalid token");
            
            
        }
        #endregion

        /// <summary>
        /// Получение данных о текущем профиле
        /// </summary>
        /// <returns>Json(User user)</returns>
        [Authorize]
        [HttpPost("getProfileData")]
        public async Task<IActionResult> GetProfileData()
        {
            var username = GetPrincipalFromAccessToken(Request.Headers.Where(p => p.Key == "Authorization").Select(p => p.Value).FirstOrDefault().ToString().Split(" ")[1]);
            var user = await _userRepository.GetUserByLogin(username.Identity.Name);
            return Ok(user);
        }

        [Authorize]
        [HttpPost("updateProfileData")]
        public async Task<IActionResult> UpdateProfileData([FromBody]UpdateProfileModel upm)
        {
            var user = await _userRepository.GetUserByLogin(upm.Username);
            if (upm.Phone != null)
                user.PhoneNumber = upm.Phone;
            if (upm.Email != null)
                user.Login = upm.Email;
            await _userRepository.UpdateAsync(user);
            return Ok();
        }
        
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody]RefreshTokenRequestModel tokens)
        {
            try
            {
                await _tokenRepository.DeleteAsync(await _tokenRepository.GetAll().Where(p => p.Token == tokens.OldRefreshToken).Select(p => p.Id).FirstOrDefaultAsync());
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }            
            return Ok();
        }
    }
}

