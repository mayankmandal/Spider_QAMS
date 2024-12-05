using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Spider_QAMS.Utilities.ValidationAttributes;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace Spider_QAMS.Pages
{
    [Authorize(Policy = "PageAccess")]
    public class SiteImageUploaderModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private SiteImageUploaderVM _siteImageUploaderVM;
        public SiteImageUploaderModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        [BindProperty]
        public SiteImageUploaderVM SiteImageUploaderVM { get; set; }

        // Dropdown Data Lists
        public IList<SitePicCategory> SitePicCategories { get; set; }

        public async Task<IActionResult> OnGetAsync(string siteId)
        {
            if (string.IsNullOrEmpty(siteId) || !long.TryParse(siteId, out long siteIdLong) || siteIdLong <= 0)
            {
                TempData["error"] = "Invalid Site ID.";
                return RedirectToPage("/ManageSiteDetails");
            }

            SiteImageUploaderVM = new SiteImageUploaderVM
            {
                SiteId = siteIdLong
            };

            await LoadDropdownDataAsync();
            await LoadExistingImagesAsync(siteId);

            return Page();
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            // Get the maximum count allowed from the MaxCount attribute
            int maxAllowedImages = GetMaxCountAttributeValue(typeof(SitePicCategoryViewModel), nameof(SitePicCategoryViewModel.Images), 10);

            // Iterate over each category and filter out deleted images
            for (int i = 0; i < SiteImageUploaderVM.SitePicCategoryList.Count; i++)
            {
                var category = SiteImageUploaderVM.SitePicCategoryList[i];
                int activeImageCount = category.Images.Count(image => image.IsDeleted != true);

                // If category is SiteProfilePicture, enforce the condition of only one non-deleted image
                if(category.Description == SitePicCategory_SiteProfilePicture)
                {
                    // Check if more than one non-deleted image exists
                    if (activeImageCount > 1)
                    {
                        // Add validation error if more than one non-deleted image is found
                        ModelState.AddModelError($"SiteImageUploaderVM.SitePicCategoryList[{i}].Images", "Only one Site Profile picture should be present.");
                    }
                }
                // If active image count <= max allowed images, remove the validation errors for Images
                else if (activeImageCount <= maxAllowedImages)
                {
                    ModelState.Remove($"SiteImageUploaderVM.SitePicCategoryList[{i}].Images");
                }
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                return new JsonResult(new { success = false, message = "Model Validation Failed : " + string.Join("; ", errorMessages)});
            }

            // Save files to server and collect the data for API call
            _siteImageUploaderVM = SiteImageUploaderVM;

            // Send API request to save image data
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateSiteImages";
                var jsonContent = JsonSerializer.Serialize(_siteImageUploaderVM);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    return new JsonResult(new {success= true, message = "Images uploaded and saved successfully." });
                }
                else
                {
                    return new JsonResult(new {success= false, message = $"Error saving images: {response.StatusCode} - {response.ReasonPhrase}"});
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "An unexpected error occurred while saving images." });
            }
        }
        // Helper method to retrieve MaxCount attribute value
        private int GetMaxCountAttributeValue(Type modelType, string propertyName, int userDefinedMaxCount = 0)
        {
            var property = modelType.GetProperty(propertyName);
            var maxCountAttr = property?.GetCustomAttribute<MaxCountAttribute>();

            return maxCountAttr?.MaxCount ?? userDefinedMaxCount;
        }
        private async Task LoadDropdownDataAsync()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));

            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllPicCategories");
            SitePicCategories = string.IsNullOrEmpty(response)
                ? new List<SitePicCategory>()
                : JsonSerializer.Deserialize<List<SitePicCategory>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        private async Task LoadExistingImagesAsync(string siteId)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
            var requestBody = new { RecordId = Convert.ToInt32(siteId), RecordType = (int)FetchRecordByIdOrTextEnum.GetSitePictureBySiteId };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var existingImages = string.IsNullOrEmpty(responseContent)
                    ? new List<SitePictures>()
                    : JsonSerializer.Deserialize<List<SitePictures>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var groupedImages = existingImages.GroupBy(img => img.SitePicCategoryData.PicCatID)
                    .Select(group => new SitePicCategoryViewModel
                    {
                        PicCatId = group.Key,
                        Description = group.First().SitePicCategoryData.Description,
                        Images = group.Select(img => new ImageViewModel
                        {
                            SitePicID = img.SitePicID,
                            FilePath = Path.Combine(_configuration["BaseUrl"],_configuration["SiteDetailImgPath"], img.SitePicCategoryData.PicCatID.ToString(), img.PicPath)?.Replace("\\","/") ?? string.Empty,
                            FileName = img.PicPath ?? string.Empty,
                            ImageFile = null,
                            IsDeleted = false,
                            FileDescription = img.Description ?? string.Empty
                        }).ToList()
                    }).ToList();

                SiteImageUploaderVM.SitePicCategoryList = groupedImages;
            }
            else
            {
                throw new Exception($"Error fetching images: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            return new JsonResult(new { success = false, message = $"Error Message - " + errorMessage + ". Error details: " + ex.Message });
        }
    }
}
