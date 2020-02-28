using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Library.Models.Identity;
using Library.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Library.Controllers.Identity
{
    [Route("/api/account")]
    public class AccountController : Controller
    {
        // тестовые данные вместо использования базы данных

        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IUserRepository userRepository, ILogger<AccountController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        #region HashSalting
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
                        _logger.LogInformation("Password is incorrect.");
                        return false;
                    }
                _logger.LogInformation("Password is correct");
                return true;
            }
            catch (NullReferenceException)
            {
                _logger.LogError("User was not found.");
                throw new Exception("User was not found");
            }
        }
        #endregion

        [HttpPost("/token")]
        public async Task<IActionResult> Token(string username)
        {
            var identity = await GetIdentity(username);
            if (identity == null)
            {
                _logger.LogInformation("Incorrect password or login.");
                return BadRequest(new { errorText = "Invalid username or password." });
            }
            _logger.LogInformation("Identity was found successfully");
            var now = DateTime.UtcNow;
            // создаем JWT-токен
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            _logger.LogInformation("Token was created.");
            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };
            _logger.LogInformation("Returning token as JSON...");
            return Json(response);
        }

        private async Task<ClaimsIdentity> GetIdentity(string username)
        {
            var person = await _userRepository.GetUserByLogin(username);
            if (person != null)
            {
                _logger.LogInformation("User was found.");
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
            _logger.LogInformation("User was not found.");
            return null;
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

                    return Ok(new { errorText = "Account has been created successfully" });
                }
                return BadRequest(new { errorText = "Invalid username or password." });
            }
            return BadRequest(new { errorText = "Model is not valid." });
        }

        /// <summary>
        /// Вход в аккаунт без аутентификации
        /// </summary>
        /// <param name="model">Модель для входа в аккаунт - LoginModel</param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody]LoginModel model)
        {

            User user = await _userRepository.GetUserByLogin(model.Login);
            if (user != null)
            {
                if (await ComparePassword(model.Login, model.Password))
                {
                    _logger.LogInformation("Correct password. Sending token.");
                    return await Token(model.Login);
                }
                _logger.LogInformation("Incorrect password.");
                return BadRequest(new { errorText = "Incorrect login or password." });
            }
            return BadRequest(new { errorText = "Incorrect login or password." });
        }
        #endregion
    }
}

