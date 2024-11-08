using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Domain;
using Spider_QAMS.Repositories.Skeleton;
using Spider_QAMS.Utilities;
using System.Security.Authentication;
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
        [HttpPost("FetchRecord")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FetchRecordData([FromBody] Record record)
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

                /*if (string.IsNullOrEmpty(record) || string.IsNullOrEmpty(input))
                {
                    return BadRequest();
                }*/

                // Fetch the appropriate type from the repository using the RecordType
                object result = await _navigationRepository.FetchRecordByTypeAsync(record);

                // Get the expected type for the response
                Type expectedType = FetchRecordTypeMapper.GetTypeByEnum((FetchRecordByIdOrTextEnum)record.RecordType);

                // If the result is not of the expected type, throw an exception
                if (result != null && result.GetType() != expectedType)
                {
                    throw new InvalidOperationException("Returned data does not match expected type.");
                }
                return Ok(result);
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
                isUniqueValue = await _navigationRepository.CheckUniquenessAsync(uniqueRequest);
                return Ok(new { IsUnique = isUniqueValue });
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

                string salt = string.Empty;
                string hashedPassword = string.Empty;

                if (!string.IsNullOrEmpty(profileUsersData.Password))
                {
                    salt = PasswordHelper.GenerateSalt();
                    hashedPassword = PasswordHelper.HashPassword(profileUsersData.Password, salt);
                }

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
                if (user == null || user.UserId <= 0)
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

                string salt = string.Empty;
                string hashedPassword = string.Empty;

                if (!string.IsNullOrEmpty(profileUserAPIVM.Password))
                {
                    salt = PasswordHelper.GenerateSalt();
                    hashedPassword = PasswordHelper.HashPassword(profileUserAPIVM.Password, salt);
                }

                ProfileUser profileUser = new ProfileUser
                {
                    Designation = profileUserAPIVM.Designation.ToString(),
                    FullName = profileUserAPIVM.FullName,
                    EmailID = profileUserAPIVM.EmailID,
                    UserName = profileUserAPIVM.UserName,
                    ProfilePicName = profileUserAPIVM.ProfilePicName,
                    PhoneNumber = profileUserAPIVM.PhoneNumber.ToString(),
                    UserId = profileUserAPIVM.UserId,
                    PasswordHash = hashedPassword,
                    PasswordSalt = salt,
                    ProfileSiteData = new ProfileSite
                    {
                        ProfileId = profileUserAPIVM.ProfileSiteData.ProfileId,
                    },
                    IsActive = profileUserAPIVM.IsActive,
                    IsADUser = profileUserAPIVM.IsADUser,
                };

                string PreviousProfilePhotoPath = await _navigationRepository.UpdateUserProfileAsync(profileUser, user.UserId);

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

        [HttpGet("GetSettingsData")]
        [ProducesResponseType(typeof(ProfileUserAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSettingsData()
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
                ProfileUserAPIVM profileUserAPIVM = await _navigationRepository.GetSettingsDataAsync(userId);
                return Ok(profileUserAPIVM);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("UpdateSettingsData")]
        [ProducesResponseType(typeof(SettingsAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSettingsData(SettingsAPIVM userSettings)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }
                // Validate the password
                if (!string.IsNullOrEmpty(userSettings.SettingsPassword) && (!Regex.IsMatch(userSettings.SettingsPassword, passwordPattern) || userSettings.SettingsPassword != userSettings.SettingsReTypePassword))
                {
                    return BadRequest("Password must be between 8 and 16 characters, and must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Also, both passwords must match.");
                }

                string salt = string.Empty;
                string hashedPassword = string.Empty;

                if (!string.IsNullOrEmpty(userSettings.SettingsPassword) || !string.IsNullOrEmpty(userSettings.SettingsReTypePassword))
                {
                    salt = PasswordHelper.GenerateSalt();
                    hashedPassword = PasswordHelper.HashPassword(userSettings.SettingsPassword, salt);
                }

                ProfileUser profileUser = new ProfileUser
                {
                    FullName = userSettings.SettingsFullName,
                    EmailID = userSettings.SettingsEmailID,
                    UserName = userSettings.SettingsUserName,
                    ProfilePicName = userSettings.SettingsProfilePicName,
                    UserId = userSettings.SettingsUserId,
                    PasswordHash = hashedPassword != null ? hashedPassword : string.Empty,
                    PasswordSalt = salt != null ? salt : string.Empty,
                };

                string PreviousProfilePhotoPath = await _navigationRepository.UpdateSettingsDataAsync(profileUser, currentUserId);

                // Remove the cached item to force a refresh next time
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserProfileKey);

                if (!string.IsNullOrEmpty(userSettings.SettingsProfilePicName) && !string.IsNullOrEmpty(PreviousProfilePhotoPath))
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

        [HttpPost("UpdateRegion")]
        [ProducesResponseType(typeof(Region), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRegionData(Region region)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.UpdateRegionAsync(region);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("CreateRegion")]
        [ProducesResponseType(typeof(Region), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRegionData(Region region)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.CreateRegionAsync(region);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("UpdateCity")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCityData(City city)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.UpdateCityAsync(city);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("CreateCity")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCityData(City city)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.CreateCityAsync(city);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("UpdateLocation")]
        [ProducesResponseType(typeof(SiteLocation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLocationData(SiteLocation siteLocation)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.UpdateLocationAsync(siteLocation);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("CreateLocation")]
        [ProducesResponseType(typeof(SiteLocation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateLocationData(SiteLocation siteLocation)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.CreateLocationAsync(siteLocation);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("UpdateContact")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateContactData(Contact contact)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.UpdateContactAsync(contact);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("CreateContact")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateContactData(Contact contact)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.CreateContactAsync(contact);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("CreateSiteDetails")]
        [ProducesResponseType(typeof(SiteDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSiteDetailsData(SiteDetail siteDetail)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                SiteDetail site = await _navigationRepository.CreateSiteDetailsAsync(siteDetail);

                if (site != null)
                {
                    return Ok(site);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("UpdateSiteDetails")]
        [ProducesResponseType(typeof(SiteDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSiteDetailsData(SiteDetail siteDetail)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                bool isSuccess = false;
                isSuccess = await _navigationRepository.UpdateSiteDetailsAsync(siteDetail);

                if (isSuccess)
                {
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost("UploadSiteImage")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadSiteImageData(IFormFile imageFile,[FromForm] int categoryId, [FromForm] long siteId)
        {
            try
            {
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT Token is missing");
                }
                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                if(imageFile == null || imageFile.Length == 0)
                {
                    return BadRequest("Invalid image file.");
                }

                string uniqueFileName = $"{siteId}_{Path.GetFileName(imageFile.FileName)}";
                string categoryFolder = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["SiteDetailImgPath"], categoryId.ToString());

                if (!Directory.Exists(categoryFolder))
                {
                    Directory.CreateDirectory(categoryFolder);
                }

                string filePath = Path.Combine(categoryFolder, uniqueFileName);
                using var fileStream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(fileStream);

                var imageRecord = new SitePictures
                {
                    SiteID = Convert.ToInt64(siteId),
                    Description = string.Empty,
                    PicPath = uniqueFileName,
                    SitePicCategoryData = new SitePicCategory { PicCatID = Convert.ToInt32(categoryId) },
                };

                int SitePicId = -1;
                SitePicId = await _navigationRepository.UploadSiteImageAsync(imageRecord);

                if (SitePicId <= 0)
                {
                    return BadRequest();
                }
                else
                {
                    return Ok(new { SitePicId, filePath = uniqueFileName });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete("DeleteSiteImage")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSiteImageData(int imageId)
        {
            try
            {
                // Validate JWT token
                var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(jwtToken))
                {
                    return Unauthorized("JWT token is missing.");
                }

                var currentUserId = await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
                if (currentUserId == null || currentUserId <= 0)
                {
                    return Unauthorized("User is not authenticated.");
                }

                // Retrieve the image record from the repository
                if (imageId <= 0)
                {
                    return BadRequest("Invalid image ID.");
                }

                Record record = new Record
                {
                    RecordId = imageId,
                    RecordText = string.Empty,
                    RecordType = (int)FetchRecordByIdOrTextEnum.GetSitePictureBySitePicID
                };

                object result = await _navigationRepository.FetchRecordByTypeAsync(record);
                if (result == null)
                {
                    return NotFound("Image not found.");
                }

                // Get expected type and validate result type
                Type expectedType = FetchRecordTypeMapper.GetTypeByEnum((FetchRecordByIdOrTextEnum)record.RecordType);
                if (result.GetType() != expectedType)
                {
                    return StatusCode(500, "Returned data does not match expected type.");
                }

                // Extract PicPath and SitePicCategoryId from the result
                var typedResult = Convert.ChangeType(result, expectedType);
                string picPath = expectedType.GetProperty("PicPath")?.GetValue(typedResult) as string;
                // Get the SitePicCategoryData object from typedResult
                var sitePicCategoryData = expectedType.GetProperty("SitePicCategoryData")?.GetValue(typedResult);

                // Check if sitePicCategoryData is not null and retrieve PicCatID
                int? sitePicCategoryId = sitePicCategoryData?.GetType().GetProperty("PicCatID")?.GetValue(sitePicCategoryData) as int?;

                if (string.IsNullOrEmpty(picPath) || sitePicCategoryId == null)
                {
                    return BadRequest("Image path or category ID is missing.");
                }

                // Delete the physical file
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["SiteDetailImgPath"], sitePicCategoryId.ToString(), picPath);
                bool fileDeletionSuccess = await DeleteFileAsync(oldFilePath);

                if (!fileDeletionSuccess)
                {
                    return BadRequest("Failed to delete image file.");
                }

                // Delete the database record
                bool recordDeletionSuccess = await _navigationRepository.DeleteEntityAsync(imageId, DeleteEntityType.SitePicture);

                if (!recordDeletionSuccess)
                {
                    return BadRequest("Failed to delete image record from the database.");
                }
                return Ok(new { message = $"{picPath} has been deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

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
                var currentUserDetails = await _navigationRepository.GetUserRecordAsync(userId);
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

        [HttpGet("GetAllUsers")]
        [ProducesResponseType(typeof(IList<ProfileUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        [HttpGet("GetAllRegion")]
        [ProducesResponseType(typeof(List<Region>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRegionData()
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
                List<Region> regionsList = await _navigationRepository.GetAllRegionsAsync();
                return Ok(regionsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllCities")]
        [ProducesResponseType(typeof(List<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCitiesData()
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
                List<City> regionsList = await _navigationRepository.GetAllCitiesAsync();
                return Ok(regionsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllLocations")]
        [ProducesResponseType(typeof(List<SiteLocation>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllLocationsData()
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
                List<SiteLocation> regionsList = await _navigationRepository.GetAllLocationsAsync();
                return Ok(regionsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetRegionListOfCities")]
        [ProducesResponseType(typeof(List<RegionAssociatedCities>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRegionListOfCitiesData()
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
                List<CityRegionViewModel> cityRegionList = await _navigationRepository.GetRegionListOfCitiesAsync();
                var regions = cityRegionList == null ? new List<RegionAssociatedCities>() : cityRegionList
                    .GroupBy(cr => new { cr.RegionId, cr.RegionName }) // Group by RegionId and RegionName
                    .Select(g => new RegionAssociatedCities
                    {
                        RegionId = g.Key.RegionId,
                        RegionName = g.Key.RegionName,
                        Cities = g.Select(city => new CityViewModel
                        {
                            CityId = city.CityId,
                            CityName = city.CityName
                        }).ToList()
                    }).ToList();
                return Ok(regions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllSponsors")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSponsorsData()
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
                List<Sponsor> sponsors = await _navigationRepository.GetAllSponsorsAsync();
                return Ok(sponsors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllContacts")]
        [ProducesResponseType(typeof(List<Contact>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllContactsData()
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
                List<Contact> contacts = await _navigationRepository.GetAllContactsAsync();
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllSiteTypes")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSiteTypesData()
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
                List<SiteType> siteTypes = await _navigationRepository.GetAllSiteTypesAsync();
                return Ok(siteTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllBranchTypes")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBranchTypesData()
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
                List<BranchType> branchTypes = await _navigationRepository.GetAllBranchTypesAsync();
                return Ok(branchTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllVisitStatuses")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllVisitStatusesData()
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
                List<VisitStatusModel> visitStatuses = await _navigationRepository.GetAllVisitStatusesAsync();
                return Ok(visitStatuses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllATMClasses")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllATMClassesData()
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
                List<string> atmClasses = await _navigationRepository.GetAllATMClassesAsync();
                return Ok(atmClasses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllPicCategories")]
        [ProducesResponseType(typeof(List<SitePicCategory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPicCategoriesData()
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
                List<SitePicCategory> sitePicCategories = await _navigationRepository.GetAllPicCategoriesAsync();
                return Ok(sitePicCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("GetAllSiteDetails")]
        [ProducesResponseType(typeof(List<SiteDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSiteDetailsData()
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
                List<SiteDetail> sites = await _navigationRepository.GetAllSiteDetailsAsync();
                return Ok(sites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        #endregion
    }
}
