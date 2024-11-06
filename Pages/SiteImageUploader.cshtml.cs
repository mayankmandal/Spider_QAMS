using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Spider_QAMS.Pages
{
    public class SiteImageUploaderModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
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
            var requestBody = new { RecordId = Convert.ToInt32(siteId), RecordType = (int)FetchRecordByIdOrTextEnum.GetSitePicture };
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
                            FilePath = Path.Combine(_configuration["BaseUrl"],_configuration["SiteDetailImgPath"], img.SitePicCategoryData.Description, img.PicPath) ?? string.Empty,
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
    }
}
