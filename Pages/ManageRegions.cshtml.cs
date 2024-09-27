using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Spider_QAMS.Models.ViewModels;

namespace Spider_QAMS.Pages
{
    public class ManageRegionsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private Region _region;
        public ManageRegionsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        [BindProperty]
        public RegionVM Region { get; set; }
        public List<RegionVM> Regions { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAllRegionData();
            return Page();
        }
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["error"] = "Model State Validation Failed: " + string.Join("; ", errorMessages);
                await LoadAllRegionData();
                return Page();
            }
            try
            {
                _region = new Region
                {
                    RegionId = 0,
                    RegionName = Region.RegionName
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/CreateRegion";
                var jsonContent = JsonSerializer.Serialize(_region);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{Region.RegionName} - Created Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllRegionData();
                    TempData["error"] = $"{Region.RegionName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                return HandleError(ex, "Error occurred during HTTP request.");
            }
            catch (JsonException ex)
            {
                return HandleError(ex, "Error occurred while parsing JSON.");
            }
            catch (Exception ex)
            {
                return HandleError(ex, "An unexpected error occurred.");
            }
        }
        private async Task LoadAllRegionData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllRegion");
            Regions = JsonSerializer.Deserialize<List<RegionVM>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (Region.RegionId != null)
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
                var requestBody = new Record { RecordId = Region.RegionId, RecordType = (int)FetchRecordByIdEnum.GetRegionData };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _region = JsonSerializer.Deserialize<Region>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    await LoadAllRegionData();
                    ModelState.AddModelError("ProfileUsersData.UserId", $"Error fetching user record for {Region.RegionName}. Please ensure the UserId is correct.");
                    TempData["error"] = $"Model State Validation Failed. Response status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }

                if (_region.RegionName == Region.RegionName)
                {
                    ModelState.Remove("Region.RegionName");
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadAllRegionData(); // Reload ProfilesData if there's a validation error
                TempData["error"] = "Model State Validation Failed.";
                return Page();
            }
            try
            {
                _region = new Region
                {
                    RegionId = Region.RegionId,
                    RegionName = Region.RegionName
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateRegion";
                var jsonContent = JsonSerializer.Serialize(_region);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{Region.RegionName} - Profile Updated Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllRegionData();
                    TempData["error"] = $"{Region.RegionName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }
            }
            catch (HttpRequestException ex)
            {
                return HandleError(ex, "Error occurred during HTTP request.");
            }
            catch (JsonException ex)
            {
                return HandleError(ex, "Error occurred while parsing JSON.");
            }
            catch (Exception ex)
            {
                return HandleError(ex, "An unexpected error occurred.");
            }
        }
        public async Task<IActionResult> OnPostDeleteAsync(int RegionId)
        {
            if (RegionId <= 0)
            {
                TempData["error"] = "User ID is required.";
                return RedirectToPage("/Error");
            }
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/DeleteEntity?deleteId={RegionId}&deleteType={DeleteEntityType.Region}";
                HttpResponseMessage response;
                response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"Region - {RegionId} deleted Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllRegionData();
                    TempData["error"] = $"Region - {RegionId} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                return HandleError(ex, "An unexpected error occurred.");
            }
        }
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            TempData["error"] = $"Error Message - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }
    }
}
