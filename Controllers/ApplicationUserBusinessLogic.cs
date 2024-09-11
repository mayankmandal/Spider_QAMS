using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Tokens;
using Spider_QAMS.Models;
using Spider_QAMS.Repositories.Domain;
using Spider_QAMS.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Spider_QAMS.Controllers
{
    public class ApplicationUserBusinessLogic : UserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ApplicationUserBusinessLogic(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public HttpContext UserContext => _httpContextAccessor.HttpContext;
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await GetUserByEmailAsyncRepo(email);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(int userId)
        {
            return await GetUserByIdAsyncRepo(userId);
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId)
        {
            if(int.TryParse(userId, out int id))
            {
                return await GetUserByIdAsync(id);
            }
            throw new ArgumentException("Invalid User ID");
        }

        public async Task<OperationResult> ConfirmEmailAsync(ApplicationUser user, string token)
        {
            // Validate the token (this will depend on token generation logic)
            if(string.IsNullOrEmpty(token) || user == null)
            {
                return OperationResult.Failure("Invalid token or user");
            }
            if(!Guid.TryParse(token, out _))
            {
                return OperationResult.Failure("Invalid confirmation token");
            }

            try
            {
                bool isConfirmed = await ConfirmEmailAsyncRepo(user.UserId);
                if(!isConfirmed)
                {
                    return OperationResult.Failure("Failed to confirm email.");
                }
                return OperationResult.Success();
            }
            catch(Exception ex)
            {
                return OperationResult.Failure("Failed to confirm email.", ex.Message);
            }
        }


        public async Task<int> GetCurrentUserIdAsync()
        {
            var token = GetJWTCookie(Constants.JwtCookieName);
            if (!string.IsNullOrEmpty(token))
            {
                var principal = GetPrincipalFromToken(token);
                var userIdClaim = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }
            throw new InvalidOperationException("User is not authenticated");
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            return await GetUserRolesAsyncRepo(userId);
        }

        public async Task<ApplicationUser> AuthenticateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user != null && user.PasswordHash == ComputeHash(password))
            {
                return user;
            }
            return null;
        }

        public async Task<bool> CheckUserExistsAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            return user != null;
        }

        public string GenerateJSONWebToken(IEnumerable<Claim> claims)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings_SecretKey"])),
                    SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public void SetJWTCookie(string token, string name)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(3)
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(name, token, cookieOptions);
        }

        public string GetJWTCookie(string name)
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies[name];
        }

        public async Task<ApplicationUser> RegisterUserAsync(ApplicationUser user, string password)
        {
            if (user != null || password != null)
            {
                string salt = PasswordHelper.GenerateSalt();
                string hashedPassword = PasswordHelper.HashPassword(password, salt);
                user.PasswordSalt = salt;
                user.PasswordHash = hashedPassword;
                var createdUser = await RegisterUserAsyncRepo(user);
                if (createdUser != null)
                {
                    return createdUser;
                }
            }
            return null;
        }

        public string GenerateEmailConfirmationToken(ApplicationUser user)
        {
            // User-specific data for token generation
            var userId = user.UserId.ToString();
            var timestamp = DateTime.UtcNow.AddHours(24).ToString("o"); // Token valid for 24 hours

            // Combine data into a single string
            var tokenData = $"{userId}|{timestamp}";

            // Generate a hash for the token data 
            var tokenHash = ComputeHash(tokenData);

            // Combine token data with hash and encode
            var token = $"{tokenData}|{tokenHash}";
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return encodedToken;
        }

        public async Task<OperationResult> SignOutAsync()
        {
            try
            {
                // Remove the JWT cookie to log out the user
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(Constants.JwtCookieName);

                // Retrieve the JWT token from the cookie
                var jwtToken = GetJWTCookie(Constants.JwtCookieName);
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return OperationResult.Success(); // Token not present, just proceed
                }
                return OperationResult.Failure("Failed to clear out the Cookies.");
            }
            catch(Exception ex)
            {
                return OperationResult.Failure("Failed to sign out user.", ex.Message);
            }
        }

        private string ComputeHash(string input)
        {
            // Implement a password hashing mechanism here
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JwtSettings_SecretKey"])),
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                throw new SecurityTokenException("Invalid token");
            }
        }
    }
}
