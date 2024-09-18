using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Domain;
using Spider_QAMS.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using static Spider_QAMS.Utilities.Constants;

namespace Spider_QAMS.Controllers
{
    public class ApplicationUserBusinessLogic : UserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IDataProtector _protector;
        private readonly IHttpClientFactory _clientFactory;
        public ApplicationUserBusinessLogic(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IDataProtectionProvider dataProtectionProvider, IHttpClientFactory clientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _protector = dataProtectionProvider.CreateProtector("EmailConfirmation");
            _clientFactory = clientFactory;

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

        // Method to refresh the cache on demand
        public async Task RefreshCurrentUserAsync()
        {
            _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserKey);
        }

        public async Task<OperationResult> ConfirmEmailAsync(string userId,string token)
        {
            // Validate the token (this will depend on token generation logic)
            if(string.IsNullOrEmpty(token))
            {
                return OperationResult.Failure("Invalid token or user");
            }

            try
            {
                var tokenParts = DecodeEmailConfirmationToken(token);
                if (tokenParts == null || tokenParts.Length != 2)
                {
                    return OperationResult.Failure("Invalid confirmation token format");
                }

                var userIdFromToken = tokenParts[0];
                var timestamp = DateTime.Parse(tokenParts[1]);

                // Check if the token has expired
                if (timestamp < DateTime.UtcNow)
                {
                    return OperationResult.Failure("Token has expired");
                }

                // Verify that the user ID from the token matches the actual user's ID
                if(userIdFromToken != userId)
                {
                    return OperationResult.Failure("Invalid confirmation token");
                }

                // Proceed with confirming the email if the token is valid
                bool isConfirmed = await ConfirmEmailAsyncRepo(Convert.ToInt32(userIdFromToken));
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
        public async Task<ApplicationUser> GetCurrentUserAsync(string jwtToken)
        {
            if (!string.IsNullOrEmpty(jwtToken))
            {
                var principal = GetPrincipalFromToken(jwtToken);
                var emailClaim = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(emailClaim))
                {
                    return await GetUserByEmailAsync(emailClaim);
                }
            }
            return null; // Return null if no user found
        }

        public async Task<int> GetCurrentUserIdAsync(string? jwtToken = null)
        {
            var user = await GetCurrentUserAsync(jwtToken);
            if (user == null)
            {
                throw new InvalidOperationException("User is not authenticated");
            }
            return user.UserId;
        }

        public async Task<IList<string>> GetUserRolesAsync(int userId)
        {
            return await GetUserRolesAsyncRepo(userId);
        }

        public async Task<ApplicationUser> AuthenticateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user != null)
            {
                try
                {
                    // Verify the password against the stored hash and salt
                    bool isPasswordValid = PasswordHelper.VerifyPassword(password, user.PasswordSalt, user.PasswordHash);
                    if (isPasswordValid)
                    {
                        return user;
                    }
                }
                finally
                {
                    // Nullify sensitive data before returning the user
                    user.PasswordHash = null;
                    user.PasswordSalt = null;
                }
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

        public async Task FetchAndCacheUserPermissions(string AccessTokenValue)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessTokenValue);

            // Fetch pages
            var pagesResponse = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetCurrentUserPages");
            var pages = JsonSerializer.Deserialize<List<PageSiteVM>>(pagesResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _httpContextAccessor.HttpContext.Session.Set(SessionKeys.CurrentUserPagesKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pages)));

            // Fetch categories
            var categoriesResponse = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetCurrentUserCategories");
            var categories = JsonSerializer.Deserialize<List<CategoryDisplayViewModel>>(categoriesResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _httpContextAccessor.HttpContext.Session.Set(SessionKeys.CurrentUserCategoriesKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(categories)));
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
            var timestamp = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours

            // Combine data into a single string
            var tokenData = $"{userId}|{timestamp:o}";

            // Protect (encrypt) the token data
            var protectedToken = _protector.Protect(tokenData);

            // Encode the token to make it URL-safe
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(protectedToken));

            return encodedToken;
        }

        public string[] DecodeEmailConfirmationToken(string token)
        {
            // Decode the token from Base64
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            // Unprotect (decrypt) the token data
            var unprotectedToken = _protector.Unprotect(decodedToken);

            // Extract the token data
            var tokenParts = unprotectedToken.Split('|');
            if(tokenParts.Length != 2)
            {
                return null;
            }
            return tokenParts;
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

        public async Task<ApplicationUser> GetCurrentUserAsync(ApplicationUser? user = null)
        {
            if (!_httpContextAccessor.HttpContext.Session.TryGetValue(SessionKeys.CurrentUserKey, out byte[] currentUserData))
            {
                // Session key not found, fetch user data
                if (user == null)
                {
                    // Retrieve JWT or other identifier
                    var token = GetJWTCookie(JwtCookieName);
                    if (!string.IsNullOrEmpty(token))
                    {
                        var principal = GetPrincipalFromToken(token);
                        var emailClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                        if (!string.IsNullOrEmpty(emailClaim))
                        {
                            var currentUser = await GetUserByEmailAsync(emailClaim);
                            if (currentUser != null)
                            {
                                _httpContextAccessor.HttpContext.Session.Set(SessionKeys.CurrentUserKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(currentUser)));
                                return currentUser;
                            }
                        }
                    }
                    return null; // No user found, return null
                }
                else
                {
                    // Set the session with the provided user
                    _httpContextAccessor.HttpContext.Session.Set(SessionKeys.CurrentUserKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user)));
                }
            }
            else
            {
                // Retrieve and deserialize the user data from the session
                user = JsonSerializer.Deserialize<ApplicationUser>(Encoding.UTF8.GetString(currentUserData),new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return user;
        }

        public async Task<IList<Claim>> GetCurrentUserClaimsAsync(ApplicationUser user)
        {
            if (!_httpContextAccessor.HttpContext.Session.TryGetValue(Constants.SessionKeys.CurrentUserClaimsKey, out byte[] claimsData))
            {
                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                        new Claim(ClaimTypes.Email, user.EmailID)
                    };

                    var roles = await GetUserRolesAsync(user.UserId);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    _httpContextAccessor.HttpContext.Session.Set(Constants.SessionKeys.CurrentUserClaimsKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(claims)));
                    await GetCurrentUserAsync(user);

                    return claims;
                }
            }
            else
            {
                return JsonSerializer.Deserialize<IList<Claim>>(Encoding.UTF8.GetString(claimsData), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return null;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
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
