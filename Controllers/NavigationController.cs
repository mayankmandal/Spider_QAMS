using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Domain;
using Spider_QAMS.Repositories.Skeleton;
using Spider_QAMS.Utilities;
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
        // Helper method for creating files
        public async Task<string> CreateFileAsync(string folderPath, string fileName, string base64Content)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, fileName);

                // Check if base64Content has a data URI prefix and remove it
                if(base64Content.Contains(","))
                {
                    base64Content = base64Content.Substring(base64Content.IndexOf(",") + 1);
                }

                // Convert base64 to byte array and save the file
                var imageBytes = Convert.FromBase64String(base64Content);
                await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await stream.WriteAsync(imageBytes, 0, imageBytes.Length);

                return filePath; // Return full file path after successful creation
            }
            catch (Exception ex)
            {
                throw new Exception("Error while creating the file.", ex);
            }
        }
        [HttpPost("FetchRecord")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FetchRecordData([FromBody] Record record)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("CheckUniqueness")]
        [AllowAnonymous] // Allow access without authentication
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckUniqueness([FromBody] UniquenessCheckRequest uniqueRequest)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(new { IsUnique = isUniqueValue });
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpDelete("DeleteEntity")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEntity(int deleteId, string deleteType)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpPost("CreateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUserProfile([FromBody] ProfileUserAPIVM profileUsersData)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("UpdateUser")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserProfile([FromBody] ProfileUserAPIVM profileUserAPIVM)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository, _userRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpGet("GetSettingsData")]
        [ProducesResponseType(typeof(ProfileUserAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetSettingsData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(profileUserAPIVM);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("UpdateSettingsData")]
        [ProducesResponseType(typeof(SettingsAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSettingsData(SettingsAPIVM userSettings)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpPost("UpdateRegion")]
        [ProducesResponseType(typeof(Region), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRegionData(Region region)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("CreateRegion")]
        [ProducesResponseType(typeof(Region), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateRegionData(Region region)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpPost("UpdateCity")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCityData(City city)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("CreateCity")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCityData(City city)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpPost("UpdateLocation")]
        [ProducesResponseType(typeof(SiteLocation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLocationData(SiteLocation siteLocation)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("CreateLocation")]
        [ProducesResponseType(typeof(SiteLocation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateLocationData(SiteLocation siteLocation)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpPost("UpdateContact")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateContactData(Contact contact)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("CreateContact")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateContactData(Contact contact)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpPost("CreateSiteDetails")]
        [ProducesResponseType(typeof(SiteDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSiteDetailsData(SiteDetail siteDetail)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(site);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpPost("UpdateSiteDetails")]
        [ProducesResponseType(typeof(SiteDetail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSiteDetailsData(SiteDetail siteDetail)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok(isSuccess);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpPost("UpdateSiteImages")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSiteImagesData(SiteImageUploaderVM siteImageUploaderVM)
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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

                    var SitePicturesData = new List<SitePictures>();

                    // Process each category and its images
                    foreach (var category in siteImageUploaderVM.SitePicCategoryList)
                    {
                        foreach (var image in category.Images)
                        {
                            if (image.IsDeleted.GetValueOrDefault() && image.SitePicID > 0 && image.FilePath != null)
                            {
                                // Mark for Delete
                                // Delete the physical file
                                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["SiteDetailImgPath"], category.PicCatId.ToString(), image.FileName);
                                bool fileDeletionSuccess = await DeleteFileAsync(oldFilePath);

                                if (!fileDeletionSuccess)
                                {
                                    return BadRequest("Failed to delete image file.");
                                }
                                // Add new image data to sitePicturesData list
                                SitePicturesData.Add(new SitePictures
                                {
                                    SitePicID = (int)image.SitePicID,
                                    SiteID = siteImageUploaderVM.SiteId,
                                    Description = image.FileDescription ?? string.Empty,
                                    PicPath = image.FileName,
                                    IsDeleted = image.IsDeleted.GetValueOrDefault(),
                                    SitePicCategoryData = new SitePicCategory
                                    {
                                        PicCatID = category.PicCatId,
                                        Description = category.Description
                                    }
                                });
                            }
                            else if (!image.IsDeleted.GetValueOrDefault() && image.SitePicID < 0 && image.FilePath == null && image.ImageFile != null)
                            {
                                // Mark for Add
                                // Prepare to create a new image
                                var uniqueFileName = $"{siteImageUploaderVM.SiteId}_{category.PicCatId}_{image.FileName}";
                                var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["SiteDetailImgPath"], category.PicCatId.ToString());

                                var filePath = await CreateFileAsync(folderPath, uniqueFileName, image.ImageFile);

                                // Add new image data to sitePicturesData list
                                SitePicturesData.Add(new SitePictures
                                {
                                    SitePicID = -1,
                                    SiteID = siteImageUploaderVM.SiteId,
                                    Description = image.FileDescription ?? string.Empty,
                                    PicPath = uniqueFileName,
                                    IsDeleted = image.IsDeleted.GetValueOrDefault(),
                                    SitePicCategoryData = new SitePicCategory
                                    {
                                        PicCatID = category.PicCatId,
                                        Description = category.Description
                                    }
                                });
                            }
                            else if (!image.IsDeleted.GetValueOrDefault() && image.SitePicID > 0 && image.FilePath != null && image.ImageFile == null)
                            {
                                // Mark for Update
                                SitePicturesData.Add(new SitePictures
                                {
                                    SitePicID = (int)image.SitePicID,
                                    SiteID = siteImageUploaderVM.SiteId,
                                    Description = image.FileDescription ?? string.Empty,
                                    PicPath = image.FileName,
                                    IsDeleted = image.IsDeleted.GetValueOrDefault(),
                                    SitePicCategoryData = new SitePicCategory
                                    {
                                        PicCatID = category.PicCatId,
                                        Description = category.Description
                                    }
                                });
                            }
                        }
                    }

                    bool isSuccess = false;
                    isSuccess = await _navigationRepository.UpdateSiteImagesAsync(SitePicturesData);

                    if (!isSuccess)
                    {
                        return BadRequest();
                    }
                    else
                    {
                        // Commit the transaction
                        await unitOfWork.CommitAsync();
                        return Ok();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpGet("GetCurrentUser")]
        [ProducesResponseType(typeof(CurrentUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUser()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(currentUser);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetCurrentUserDetails")]
        [ProducesResponseType(typeof(ProfileUserAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserDetails()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(currentUserDetails);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetCurrentUserProfile")]
        [ProducesResponseType(typeof(ProfileSite), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(currentUserProfile);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetCurrentUserPages")]
        [ProducesResponseType(typeof(IEnumerable<PageSiteVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserPages()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(pageSites);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetCurrentUserCategories")]
        [ProducesResponseType(typeof(IEnumerable<CategoriesSetDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserCategories()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(StructureData);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        [HttpGet("GetAllUsers")]
        [ProducesResponseType(typeof(IList<ProfileUser>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(allUsersData);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllProfiles")]
        [ProducesResponseType(typeof(IEnumerable<ProfileSite>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProfiles()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(profileSiteVMs);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllPages")]
        [ProducesResponseType(typeof(IEnumerable<PageSiteVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPages()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(pageSiteVMs);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllCategories")]
        [ProducesResponseType(typeof(IEnumerable<PageCategory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(pageCategoryVMs);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllRegion")]
        [ProducesResponseType(typeof(List<Region>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRegionData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(regionsList);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllCities")]
        [ProducesResponseType(typeof(List<City>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCitiesData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(regionsList);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllLocations")]
        [ProducesResponseType(typeof(List<SiteLocation>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllLocationsData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(regionsList);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetRegionListOfCities")]
        [ProducesResponseType(typeof(List<RegionAssociatedCities>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRegionListOfCitiesData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(regions);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllSponsors")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSponsorsData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(sponsors);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllContacts")]
        [ProducesResponseType(typeof(List<Contact>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllContactsData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(contacts);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllSiteTypes")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSiteTypesData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(siteTypes);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllBranchTypes")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllBranchTypesData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(branchTypes);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllVisitStatuses")]
        [ProducesResponseType(typeof(List<Sponsor>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllVisitStatusesData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(visitStatuses);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllATMClasses")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllATMClassesData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(atmClasses);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllPicCategories")]
        [ProducesResponseType(typeof(List<SitePicCategory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPicCategoriesData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(sitePicCategories);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }
        [HttpGet("GetAllSiteDetails")]
        [ProducesResponseType(typeof(List<SiteDetail>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllSiteDetailsData()
        {
            using (var unitOfWork = new UnitOfWork(_configuration.GetConnectionString("DefaultConnection"), _navigationRepository))
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
                    // Commit the transaction
                    await unitOfWork.CommitAsync();
                    return Ok(sites);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
