using Microsoft.AspNetCore.Mvc;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Skeleton;
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
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,16}$";
        #endregion

        #region ,
        public NavigationController(IHttpContextAccessor httpContextAccessor, INavigationRepository navigationRepository, ApplicationUserBusinessLogic applicationUserBusinessLogic, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _navigationRepository = navigationRepository;
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

        [HttpPost("UpdateUserVerification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserVerification([FromBody] UserVerifyApiVM userVerifyApiVM)
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
                if (userVerifyApiVM == null)
                {
                    return BadRequest();
                }

                string PreviousProfilePhotoPath = await _navigationRepository.UpdateUserVerificationAsync(userVerifyApiVM, userId);

                // Remove the cached item to force a refresh next time
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserProfileKey);
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserPagesKey);
                _httpContextAccessor.HttpContext.Session.Remove(SessionKeys.CurrentUserCategoriesKey);

                if (!string.IsNullOrEmpty(userVerifyApiVM.ProfilePicName) && !string.IsNullOrEmpty(PreviousProfilePhotoPath))
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

        #endregion

    }
}
