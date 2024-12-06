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
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$";
        private readonly IUnitOfWork _unitOfWork;
        #endregion

        #region Constructor,
        public NavigationController(ApplicationUserBusinessLogic applicationUserBusinessLogic, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Validate and extract user ID from JWT token
        /// </summary>
        private async Task<int?> GetAuthenticatedUserIdAsync()
        {
            var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(jwtToken)) return null;

            return await _applicationUserBusinessLogic.GetCurrentUserIdAsync(jwtToken);
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
                if (base64Content.Contains(","))
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
        public async Task<string> ReadFileAsync(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    // Read the file content as a byte array
                    byte[] fileBytes;
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        fileBytes = new byte[stream.Length];
                        await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                    }

                    // Convert the byte array to a base64 string
                    string base64Content = Convert.ToBase64String(fileBytes);

                    // Return the base64 content
                    return base64Content;
                }
                else
                {
                    throw new FileNotFoundException("File does not exist at the specified path.");
                }
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                if (record.RecordId <= 0 && string.IsNullOrEmpty(record.RecordText))
                {
                    return BadRequest("Invalid record parameters.");
                }

                // Fetch the appropriate type from the repository using the RecordType
                object result = await _unitOfWork.NavigationRepository.FetchRecordByTypeAsync(record);

                // Get the expected type for the response
                Type expectedType = FetchRecordTypeMapper.GetTypeByEnum((FetchRecordByIdOrTextEnum)record.RecordType);

                // If the result is not of the expected type, throw an exception
                if (result != null && result.GetType() != expectedType)
                {
                    throw new InvalidOperationException("Returned data does not match expected type.");
                }
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                bool isUniqueValue = false;
                isUniqueValue = await _unitOfWork.NavigationRepository.CheckUniquenessAsync(uniqueRequest);
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(new { IsUnique = isUniqueValue });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpDelete("DeleteEntity")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEntity(int deleteId, string deleteType)
        {
            // Deleted files
            var deletedFiles = new List<(string FilePath, string OriginalContent)>();
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                if (deleteId <= 0 || string.IsNullOrEmpty(deleteType))
                {
                    return BadRequest($"{deleteType} {deleteId} is invalid.");
                }

                (bool, List<string>) isSuccessPaths = await _unitOfWork.NavigationRepository.DeleteEntityAsync(deleteId, deleteType);

                if (isSuccessPaths.Item1 && isSuccessPaths.Item2.Count > 0)
                {
                    if (deleteType == DeleteEntityType.User)
                    {
                        foreach (var path in isSuccessPaths.Item2)
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"], path);

                            // Get original file content in base64 format for rollback
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                var originalContent = await ReadFileAsync(oldFilePath);
                                deletedFiles.Add((oldFilePath, originalContent));
                            }
                            isSuccessPaths.Item1 &= await DeleteFileAsync(oldFilePath);
                        }
                    }
                    else if (deleteType == DeleteEntityType.SiteDetail)
                    {
                        foreach (var path in isSuccessPaths.Item2)
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["SiteDetailImgPath"], path);

                            // Get original file content in base64 format for rollback
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                var originalContent = await ReadFileAsync(oldFilePath);
                                deletedFiles.Add((oldFilePath, originalContent));
                            }
                            isSuccessPaths.Item1 &= await DeleteFileAsync(oldFilePath);
                        }
                    }
                }

                if (!isSuccessPaths.Item1)
                {
                    return BadRequest("Failed to delete in database.");
                }
                else
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccessPaths.Item1);
                }
            }
            catch (Exception ex)
            {
                // Rollback the transaction
                _unitOfWork.Rollback();

                // Rollback file operations
                foreach (var (filePath, originalContent) in deletedFiles)
                {
                    // Restore deleted files using the original content
                    var folderPath = Path.GetDirectoryName(filePath);
                    var fileName = Path.GetFileName(filePath);
                    try
                    {
                        await CreateFileAsync(folderPath, fileName, originalContent);
                    }
                    catch (Exception e)
                    {
                        throw new FileLoadException($"Failed to restore file at {filePath}: {ex.Message}");
                    }
                }
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

            string uniqueFileName = null;
            string filePath = null;
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                if (profileUsersData == null)
                {
                    return BadRequest();
                }

                // Validate the password
                if (!string.IsNullOrEmpty(profileUsersData.Password) && !Regex.IsMatch(profileUsersData.Password, passwordPattern))
                {
                    return BadRequest("Password must be between 8 and 16 characters, and must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.");
                }

                if (!string.IsNullOrEmpty(profileUsersData.ProfilePictureFile))
                {
                    uniqueFileName = $"{Guid.NewGuid()}_{profileUsersData.ProfilePictureName}.jpg";
                    var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"]);
                    filePath = await CreateFileAsync(folderPath, uniqueFileName, profileUsersData.ProfilePictureFile);
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
                    UserId = 0,
                    Designation = profileUsersData.Designation.ToString(),
                    FullName = profileUsersData.FullName,
                    EmailID = profileUsersData.EmailID,
                    UserName = profileUsersData.UserName,
                    ProfilePicName = uniqueFileName, // Save unique file name
                    PhoneNumber = profileUsersData.PhoneNumber.ToString(),
                    PasswordHash = hashedPassword,
                    PasswordSalt = salt,
                    ProfileSiteData = new ProfileSite
                    {
                        ProfileId = profileUsersData.ProfileSiteData.ProfileId,
                    },
                    IsActive = profileUsersData.IsActive,
                    IsADUser = profileUsersData.IsADUser,
                };

                await _unitOfWork.NavigationRepository.CreateUserProfileAsync(profileUser, userId.Value);
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // Delete the file if it was created
                if (!string.IsNullOrEmpty(filePath))
                {
                    await DeleteFileAsync(filePath);
                }
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
            string uniqueFileName = null;
            string filePath = null;
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                if (profileUserAPIVM == null || string.IsNullOrEmpty(profileUserAPIVM.EmailID))
                {
                    return BadRequest("Invalid user profile data");
                }

                // Fetch the role of the target user
                var targetUserRole = await _unitOfWork.UserRepository.GetUserRolesAsyncRepo(userId.Value);
                if (targetUserRole == null)
                {
                    return NotFound("Target user does not have an associated role.");
                }

                if (!string.IsNullOrEmpty(profileUserAPIVM.ProfilePictureFile))
                {
                    uniqueFileName = $"{Guid.NewGuid()}_{profileUserAPIVM.ProfilePictureName}.jpg";
                    var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"]);
                    filePath = await CreateFileAsync(folderPath, uniqueFileName, profileUserAPIVM.ProfilePictureFile);
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
                    ProfilePicName = uniqueFileName,
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

                // Update profile in the database and get previous profile photo path
                string PreviousProfilePhotoPath = await _unitOfWork.NavigationRepository.UpdateUserProfileAsync(profileUser, userId.Value);

                // Commit the transaction
                await _unitOfWork.CommitAsync();

                // Remove the cached item to force a refresh next time
                _applicationUserBusinessLogic.UserContext.Session.Remove(SessionKeys.CurrentUserProfileKey);
                _applicationUserBusinessLogic.UserContext.Session.Remove(SessionKeys.CurrentUserPagesKey);
                _applicationUserBusinessLogic.UserContext.Session.Remove(SessionKeys.CurrentUserCategoriesKey);

                if (!string.IsNullOrEmpty(profileUserAPIVM.ProfilePictureFile) && !string.IsNullOrEmpty(PreviousProfilePhotoPath))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"], PreviousProfilePhotoPath);
                    await DeleteFileAsync(oldFilePath);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate user
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null) return Unauthorized("User is not authenticated.");

                ProfileUserAPIVM profileUserAPIVM = await _unitOfWork.NavigationRepository.GetSettingsDataAsync(userId.Value);
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(profileUserAPIVM);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpPost("UpdateSettingsData")]
        [ProducesResponseType(typeof(SettingsAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSettingsData(SettingsAPIVM userSettings)
        {

            string uniqueFileName = null;
            string filePath = null;
            try
            {
                // Authenticate User
                var currentUserId = await GetAuthenticatedUserIdAsync();
                if (currentUserId == null)
                    return Unauthorized("User is not authenticated.");
                // Validate the password
                if (!string.IsNullOrEmpty(userSettings.SettingsPassword) && (!Regex.IsMatch(userSettings.SettingsPassword, passwordPattern) || userSettings.SettingsPassword != userSettings.SettingsReTypePassword))
                {
                    return BadRequest("Password must be between 8 and 16 characters, and must contain at least one uppercase letter, one lowercase letter, one digit, and one special character. Also, both passwords must match.");
                }

                if (!string.IsNullOrEmpty(userSettings.SettingsProfilePictureFile))
                {
                    uniqueFileName = $"{Guid.NewGuid()}_{userSettings.SettingsProfilePictureName}.jpg";
                    var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"]);
                    filePath = await CreateFileAsync(folderPath, uniqueFileName, userSettings.SettingsProfilePictureFile);
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
                    ProfilePicName = uniqueFileName,
                    UserId = userSettings.SettingsUserId,
                    PasswordHash = hashedPassword != null ? hashedPassword : string.Empty,
                    PasswordSalt = salt != null ? salt : string.Empty,
                };

                string PreviousProfilePhotoPath = await _unitOfWork.NavigationRepository.UpdateSettingsDataAsync(profileUser, currentUserId.Value);

                // Remove the cached item to force a refresh next time
                _applicationUserBusinessLogic.UserContext.Session.Remove(SessionKeys.CurrentUserProfileKey);

                // Commit the transaction
                await _unitOfWork.CommitAsync();

                if (!string.IsNullOrEmpty(userSettings.SettingsProfilePictureFile) && !string.IsNullOrEmpty(PreviousProfilePhotoPath))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["UserProfileImgPath"], PreviousProfilePhotoPath);
                    await DeleteFileAsync(oldFilePath);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.UpdateRegionAsync(region);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.CreateRegionAsync(region);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.UpdateCityAsync(city);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.CreateCityAsync(city);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.UpdateLocationAsync(siteLocation);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.CreateLocationAsync(siteLocation);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.UpdateContactAsync(contact);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.CreateContactAsync(contact);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }

        [HttpPost("UpdateProfile")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfileData(ProfilePagesAccess profilePagesAccess)
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.UpdateProfileAsync(profilePagesAccess, userId.Value);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();

                    // Remove the cached item to force a refresh next time
                    _applicationUserBusinessLogic.UserContext.Session.Remove(SessionKeys.CurrentUserProfileKey);
                    _applicationUserBusinessLogic.UserContext.Session.Remove(SessionKeys.CurrentUserPagesKey);
                    _applicationUserBusinessLogic.UserContext.Session.Remove(SessionKeys.CurrentUserCategoriesKey);

                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpPost("CreateProfile")]
        [ProducesResponseType(typeof(City), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProfileData(ProfilePagesAccess profilePagesAccess)
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.CreateProfileAsync(profilePagesAccess, userId.Value);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                SiteDetail site = await _unitOfWork.NavigationRepository.CreateSiteDetailsAsync(siteDetail);

                if (site != null)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(site);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                bool isSuccess = false;
                isSuccess = await _unitOfWork.NavigationRepository.UpdateSiteDetailsAsync(siteDetail);

                if (isSuccess)
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok(isSuccess);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }

        [HttpPost("UpdateSiteImages")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSiteImagesData(SiteImageUploaderVM siteImageUploaderVM)
        {

            // Track created and deleted files
            var createdFiles = new List<string>();
            var deletedFiles = new List<(string FilePath, string OriginalContent)>();

            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

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

                            // Get original file content in base64 format for rollback
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                var originalContent = await ReadFileAsync(oldFilePath);
                                deletedFiles.Add((oldFilePath, originalContent));
                            }

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
                            var uniqueFileName = $"{Guid.NewGuid()}_{siteImageUploaderVM.SiteId}_{category.PicCatId}_{image.FileName}";
                            var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["SiteDetailImgPath"], category.PicCatId.ToString());

                            var filePath = await CreateFileAsync(folderPath, uniqueFileName, image.ImageFile);

                            // Track the newly created file for rollback purposes
                            createdFiles.Add(filePath);

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
                isSuccess = await _unitOfWork.NavigationRepository.UpdateSiteImagesAsync(SitePicturesData);

                if (!isSuccess)
                {
                    return BadRequest("Failed to update site images in database.");
                }
                else
                {
                    // Commit the transaction
                    await _unitOfWork.CommitAsync();
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                // Rollback the transaction
                _unitOfWork.Rollback();

                // Rollback file operations
                foreach (var createdFile in createdFiles)
                {
                    await DeleteFileAsync(createdFile); // Delete any files created during the operation
                }

                foreach (var (filePath, originalContent) in deletedFiles)
                {
                    // Restore deleted files using the original content
                    var folderPath = Path.GetDirectoryName(filePath);
                    var fileName = Path.GetFileName(filePath);

                    try
                    {
                        await CreateFileAsync(folderPath, fileName, originalContent);
                    }
                    catch (Exception e)
                    {
                        throw new FileLoadException($"Failed to restore file at {filePath}: {ex.Message}");
                    }

                }
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }

        [HttpGet("GetCurrentUser")]
        [ProducesResponseType(typeof(CurrentUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserData()
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
                await _unitOfWork.CommitAsync();
                return Ok(currentUser);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetCurrentUserDetails")]
        [ProducesResponseType(typeof(ProfileUserAPIVM), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserDetailsData()
        {

            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                var currentUserDetails = await _unitOfWork.NavigationRepository.GetUserRecordAsync(userId.Value);
                currentUserDetails.ProfilePictureFile = Path.Combine(_configuration["UserProfileImgPath"], currentUserDetails.ProfilePictureFile);
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(currentUserDetails);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetCurrentUserProfile")]
        [ProducesResponseType(typeof(ProfileSite), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserProfileData()
        {
            try
            {
                ProfileSite currentUserProfile = null;
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                if (!_applicationUserBusinessLogic.UserContext.Session.TryGetValue(SessionKeys.CurrentUserProfileKey, out byte[] currentUserProfileData))
                {
                    currentUserProfile = await _unitOfWork.NavigationRepository.GetCurrentUserProfileAsync(userId.Value);
                    _applicationUserBusinessLogic.UserContext.Session.Set(SessionKeys.CurrentUserProfileKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(currentUserProfile)));
                }
                else
                {
                    currentUserProfile = JsonSerializer.Deserialize<ProfileSite>(Encoding.UTF8.GetString(currentUserProfileData), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(currentUserProfile);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetCurrentUserPages")]
        [ProducesResponseType(typeof(List<PageSiteVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserPagesData()
        {

            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<PageSiteVM> pageSites = null;

                if (!_applicationUserBusinessLogic.UserContext.Session.TryGetValue(SessionKeys.CurrentUserPagesKey, out byte[] pageSitesData))
                {
                    pageSites = await _unitOfWork.NavigationRepository.GetCurrentUserPagesAsync(userId.Value);

                    _applicationUserBusinessLogic.UserContext.Session.Set(SessionKeys.CurrentUserPagesKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pageSites)));
                }
                else
                {
                    pageSites = JsonSerializer.Deserialize<List<PageSiteVM>>(Encoding.UTF8.GetString(pageSitesData), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(pageSites);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetCurrentUserCategories")]
        [ProducesResponseType(typeof(List<CategoryDisplayViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUserCategoriesData()
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                List<CategoryDisplayViewModel> StructureData;
                if (!_applicationUserBusinessLogic.UserContext.Session.TryGetValue(SessionKeys.CurrentUserCategoriesKey, out byte[] structureDataBytes))
                {
                    List<CategoriesSetDTO> categoriesSet = await _unitOfWork.NavigationRepository.GetCurrentUserCategoriesAsync(userId.Value);
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

                    _applicationUserBusinessLogic.UserContext.Session.Set(SessionKeys.CurrentUserCategoriesKey, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(StructureData)));
                }
                else
                {
                    StructureData = JsonSerializer.Deserialize<List<CategoryDisplayViewModel>>(Encoding.UTF8.GetString(structureDataBytes), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(StructureData);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }

        [HttpGet("GetAllUsers")]
        [ProducesResponseType(typeof(List<ProfileUserAPIVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsersData()
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                var allUsersData = await _unitOfWork.NavigationRepository.GetAllUsersDataAsync();
                foreach (var profileUserAPIVM in allUsersData)
                {
                    profileUserAPIVM.ProfilePictureFile = Path.Combine(_configuration["UserProfileImgPath"], profileUserAPIVM.ProfilePictureFile);
                }
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(allUsersData);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetAllProfiles")]
        [ProducesResponseType(typeof(List<ProfileSite>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProfilesData()
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                var allProfilesData = await _unitOfWork.NavigationRepository.GetAllProfilesAsync();
                
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(allProfilesData);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetAllPages")]
        [ProducesResponseType(typeof(List<PageSite>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPagesData()
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                var allPagesData = await _unitOfWork.NavigationRepository.GetAllPagesAsync();
                
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(allPagesData);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetAllProfilePages")]
        [ProducesResponseType(typeof(List<ProfilePagesAccess>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProfilePagesData()
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");

                var allProfilesPagesData = await _unitOfWork.NavigationRepository.GetAllProfilePagesAsync();

                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(allProfilesPagesData);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
        [HttpGet("GetAllCategories")]
        [ProducesResponseType(typeof(List<PageCategory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategoriesData()
        {
            try
            {
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                var allPageCategoryData = await _unitOfWork.NavigationRepository.GetAllCategoriesAsync();

                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(allPageCategoryData);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<Region> regionsList = await _unitOfWork.NavigationRepository.GetAllRegionsAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(regionsList);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<City> regionsList = await _unitOfWork.NavigationRepository.GetAllCitiesAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(regionsList);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<SiteLocation> regionsList = await _unitOfWork.NavigationRepository.GetAllLocationsAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(regionsList);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<CityRegionViewModel> cityRegionList = await _unitOfWork.NavigationRepository.GetRegionListOfCitiesAsync();
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
                await _unitOfWork.CommitAsync();
                return Ok(regions);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<Sponsor> sponsors = await _unitOfWork.NavigationRepository.GetAllSponsorsAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(sponsors);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<Contact> contacts = await _unitOfWork.NavigationRepository.GetAllContactsAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(contacts);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<SiteType> siteTypes = await _unitOfWork.NavigationRepository.GetAllSiteTypesAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(siteTypes);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<BranchType> branchTypes = await _unitOfWork.NavigationRepository.GetAllBranchTypesAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(branchTypes);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<VisitStatusModel> visitStatuses = await _unitOfWork.NavigationRepository.GetAllVisitStatusesAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(visitStatuses);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<string> atmClasses = await _unitOfWork.NavigationRepository.GetAllATMClassesAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(atmClasses);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<SitePicCategory> sitePicCategories = await _unitOfWork.NavigationRepository.GetAllPicCategoriesAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(sitePicCategories);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
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
                // Authenticate User
                var userId = await GetAuthenticatedUserIdAsync();
                if (userId == null)
                    return Unauthorized("User is not authenticated.");
                List<SiteDetail> sites = await _unitOfWork.NavigationRepository.GetAllSiteDetailsAsync();
                // Commit the transaction
                await _unitOfWork.CommitAsync();
                return Ok(sites);
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        #endregion
    }
}
