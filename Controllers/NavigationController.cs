using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Domain;
using Spider_QAMS.Repositories.Skeleton;
using Spider_QAMS.Utilities;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Spider_QAMS.Utilities.Constants;

namespace Spider_QAMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NavigationController : ControllerBase
    {
        #region Fields
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INavigationRepository _navigationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$";
        #endregion

        #region ,
        public NavigationController(IHttpContextAccessor httpContextAccessor, INavigationRepository navigationRepository, IUserRepository userRepository, ApplicationUserBusinessLogic applicationUserBusinessLogic, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _navigationRepository = navigationRepository;
            _userRepository = userRepository;
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
        #endregion

        #region Actions

        [HttpGet("GetCurrentUser")]
        [ProducesResponseType(typeof(CurrentUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserData = await _applicationUserBusinessLogic.GetCurrentUserAsync(jwtToken);

                if (currentUserData == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                CurrentUser currentUser = new CurrentUser
                {
                    UserId = currentUserData.UserId,
                    ProfilePicName = Path.Combine(_configuration["BaseUrl"], _configuration["UserProfileImgPath"], currentUserData.ProfilePicName == null ? string.Empty : currentUserData.ProfilePicName),
                    UserName = currentUserData.UserName,
                    Designation = currentUserData.Designation,
                    FullName = currentUserData.FullName,
                };

                return Ok(currentUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetCurrentUserDetails")]
        [ProducesResponseType(typeof(ProfileUserAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserDetails()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                var currentUserDetails = await _navigationRepository.GetCurrentUserDetailsAsync(userId);
                currentUserDetails.ProfilePicName = Path.Combine(_configuration["UserProfileImgPath"], currentUserDetails.ProfilePicName);
                return Ok(currentUserDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetCurrentUserProfile")]
        [ProducesResponseType(typeof(ProfileSite), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            try
            {
                ProfileSite currentUserProfile = null;
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                if (!_httpContextAccessor.HttpContext.Session.TryGetValue(SessionKeys.CurrentUserProfileKey, out byte[] currentUserProfileData))
                {
                    currentUserProfile = await _navigationRepository.GetCurrentUserProfileAsync(userId);
                    _httpContextAccessor.HttpContext.Session.Set(SessionKeys.CurrentUserProfileKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(currentUserProfile)));
                }
                else
                {
                    currentUserProfile = JsonSerializer.Deserialize<ProfileSite>(Encoding.UTF8.GetString(currentUserProfileData), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return Ok(currentUserProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetCurrentUserPages")]
        [ProducesResponseType(typeof(IEnumerable<PageSiteVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserPages()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                List<PageSiteVM> pageSites = null;

                if (!_httpContextAccessor.HttpContext.Session.TryGetValue(SessionKeys.CurrentUserPagesKey, out byte[] pageSitesData))
                {
                    pageSites = await _navigationRepository.GetCurrentUserPagesAsync(userId);

                    _httpContextAccessor.HttpContext.Session.Set(SessionKeys.CurrentUserPagesKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pageSites)));
                }
                else
                {
                    pageSites = JsonSerializer.Deserialize<List<PageSiteVM>>(Encoding.UTF8.GetString(pageSitesData), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return Ok(pageSites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetCurrentUserCategories")]
        [ProducesResponseType(typeof(IEnumerable<CategoriesSetDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserCategories()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                List<CategoryDisplayViewModel> StructureData;
                if (!_httpContextAccessor.HttpContext.Session.TryGetValue(SessionKeys.CurrentUserCategoriesKey, out byte[] structureDataBytes))
                {
                    List<CategoriesSetDTO> categoriesSet = await _navigationRepository.GetCurrentUserCategoriesAsync(userId);
                    var groupedCategories = categoriesSet.GroupBy(cat => string.IsNullOrEmpty(cat.CategoryName) ? Constants.CategoryType_UncategorizedPages : cat.CategoryName);
                    StructureData = new List<CategoryDisplayViewModel>();

                    foreach (var categoryGroup in groupedCategories)
                    {
                        var category = new CategoryDisplayViewModel
                        {
                            CategoryName = categoryGroup.Key,
                            Pages = categoryGroup.Select(page => new PageDisplayViewModel
                            {
                                PageDesc = page.PageDesc,
                                PageUrl = page.PageUrl,
                            }).ToList()
                        };
                        StructureData.Add(category);
                    }

                    // Sort the pages within each category
                    foreach (var category in StructureData)
                    {
                        category.Pages = category.Pages.OrderBy(page => page.PageDesc).ToList();
                    }

                    // Sort the categories
                    StructureData = StructureData.OrderBy(cat => cat.CategoryName).ToList();

                    _httpContextAccessor.HttpContext.Session.Set(SessionKeys.CurrentUserCategoriesKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(StructureData)));
                }
                else
                {
                    StructureData = JsonSerializer.Deserialize<List<CategoryDisplayViewModel>>(Encoding.UTF8.GetString(structureDataBytes), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return Ok(StructureData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    // Ensure any operations on the file are completed before deletion
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose))
                    {
                        // This will ensure the file is closed properly before deletion
                        stream.Close();
                    }

                    // Delete the file after ensuring it is not being used by another process
                    System.IO.File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while deleting the file.", ex);
            }
        }

        [HttpGet("GetAllUsers")]
        [ProducesResponseType(typeof(IList<ProfileUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [AllowAnonymous]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                var allUsersData = await _navigationRepository.GetAllUsersDataAsync();
                foreach (var profileUserAPIVM in allUsersData)
                {
                    profileUserAPIVM.ProfilePicName = Path.Combine(_configuration["UserProfileImgPath"], profileUserAPIVM.ProfilePicName);
                }
                return Ok(allUsersData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetAllProfiles")]
        [ProducesResponseType(typeof(IEnumerable<ProfileSite>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProfiles()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                List<ProfileSiteVM> profileSiteVMs = new List<ProfileSiteVM>();
                var allProfilesData = await _navigationRepository.GetAllProfilesAsync();
                foreach (var profileSiteData in allProfilesData)
                {
                    ProfileSiteVM profileSite = new ProfileSiteVM
                    {
                        ProfileId = profileSiteData.ProfileId,
                        ProfileName = profileSiteData.ProfileName,
                    };
                    profileSiteVMs.Add(profileSite);
                }
                return Ok(profileSiteVMs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetAllPages")]
        [ProducesResponseType(typeof(IEnumerable<PageSiteVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPages()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                List<PageSiteVM> pageSiteVMs = new List<PageSiteVM>();
                var allPagesData = await _navigationRepository.GetAllPagesAsync();
                foreach (var page in allPagesData)
                {
                    PageSiteVM pageSiteVM = new PageSiteVM
                    {
                        PageId = page.PageId,
                        isSelected = page.isSelected,
                        PageDesc = page.PageDesc,
                        PageUrl = page.PageUrl
                    };
                    pageSiteVMs.Add(pageSiteVM);
                }
                return Ok(pageSiteVMs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("GetAllCategories")]
        [ProducesResponseType(typeof(IEnumerable<PageCategory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                List<PageCategoryVM> pageCategoryVMs = new List<PageCategoryVM>();
                var allPageCategoryData = await _navigationRepository.GetAllCategoriesAsync();
                foreach (var pageCategoryData in allPageCategoryData)
                {
                    PageCategoryVM pageCategoryVM = new PageCategoryVM
                    {
                        CategoryName = pageCategoryData.CategoryName,
                        PageCatId = pageCategoryData.PageCatId,
                        PageId = pageCategoryData.PageId,
                    };
                    pageCategoryVMs.Add(pageCategoryVM);
                }
                return Ok(pageCategoryVMs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("CheckUniqueness")]
        [AllowAnonymous] // Allow access without authentication
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckUniqueness([FromBody] UniquenessCheckRequest uniqueRequest)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                bool isUniqueValue = false;
                isUniqueValue = await _navigationRepository.CheckUniquenessAsync(uniqueRequest.Field, uniqueRequest.Value);
                return Ok(new { IsUnique = isUniqueValue });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("FetchUserRecord")]
        [ProducesResponseType(typeof(ProfileUserAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FetchUserRecord([FromBody] UserIdRequest request)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                ProfileUserAPIVM profileUserAPIVM = await _navigationRepository.GetUserRecordAsync(request.UserId);
                return Ok(profileUserAPIVM);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("CreateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUserProfile([FromBody] ProfileUserAPIVM profileUsersData)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                if (profileUsersData == null)
                {
                    return BadRequest();
                }

                // Validate the password
                if (!string.IsNullOrEmpty(profileUsersData.Password) && !Regex.IsMatch(profileUsersData.Password, passwordPattern))
                {
                    return BadRequest("Password must be between 8 and 16 characters, and must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
                }

                string salt = PasswordHelper.GenerateSalt();
                string hashedPassword = PasswordHelper.HashPassword(profileUsersData.Password, salt);

                ProfileUser profileUser = new ProfileUser
                {
                    Designation = profileUsersData.Designation.ToString(),
                    FullName = profileUsersData.FullName,
                    EmailID = profileUsersData.EmailID,
                    UserName = profileUsersData.UserName,
                    ProfilePicName = profileUsersData.ProfilePicName,
                    PhoneNumber = profileUsersData.PhoneNumber.ToString(),
                    UserId = 0,
                    PasswordHash = hashedPassword,
                    PasswordSalt = salt,
                    ProfileSiteData = new ProfileSite
                    {
                        ProfileId = profileUsersData.ProfileSiteData.ProfileId,
                    },
                    IsActive = profileUsersData.IsActive,
                    IsADUser = profileUsersData.IsADUser,
                };

                await _navigationRepository.CreateUserProfileAsync(profileUser, userId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] ProfileUserAPIVM profileUserAPIVM)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var user = await _applicationUserBusinessLogic.GetCurrentUserAsync(jwtToken);
                if (user == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                if (profileUserAPIVM == null || string.IsNullOrEmpty(profileUserAPIVM.EmailID))
                {
                    return BadRequest("Invalid user profile data");
                }

                // Fetch target user by email from profileUserAPIVM
                var targetUser = await _applicationUserBusinessLogic.GetUserByEmailAsync(profileUserAPIVM.EmailID);
                if (targetUser == null)
                {
                    return NotFound($"User with email {profileUserAPIVM.EmailID} not found.");
                }

                // Fetch the role of the target user
                var targetUserRole = await _userRepository.GetUserRolesAsyncRepo(profileUserAPIVM.UserId);
                if (targetUserRole == null)
                {
                    return NotFound("Target user does not have an associated role.");
                }
                string PreviousProfilePhotoPath = await _navigationRepository.UpdateUserProfileAsync(profileUserAPIVM, user.UserId);

                // Remove the cached item to force a refresh next time
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserProfileKey);
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserPagesKey);
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserCategoriesKey);

                if (!string.IsNullOrEmpty(profileUserAPIVM.ProfilePicName) && !string.IsNullOrEmpty(PreviousProfilePhotoPath))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"], PreviousProfilePhotoPath);
                    bool isSucess = await DeleteFileAsync(oldFilePath);
                    return Ok(isSucess);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteEntity")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEntity(int deleteId, string deleteType)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var userId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (userId == null)
                {
                    return Unauthorized("User is not authenticated.");
                }
                if (deleteId <= 0 || string.IsNullOrEmpty(deleteType))
                {
                    return BadRequest($"{deleteType} {deleteId} is invalid.");
                }
                bool isSuccess = await _navigationRepository.DeleteEntityAsync(deleteId, deleteType);
                return Ok(isSuccess);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        #endregion

    }
}
