using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Spider_QAMS.Pages
{
    [Authorize(Policy = "PageAccess")]
    public class ManageCitiesModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private City _city;
        public ManageCitiesModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public IList<CityVM> Cities { get; set; }
        public IList<Region> Regions { get; set; }
        [BindProperty]
        public CityVM CityViewModel { get; set; }
        private async Task LoadAllCitiesData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllCities");
            Cities = string.IsNullOrEmpty(response) ? new List<CityVM>() : JsonSerializer.Deserialize<List<CityVM>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        private async Task LoadAllRegionData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllRegion");
            Regions = string.IsNullOrEmpty(response) ? new List<Region>() : JsonSerializer.Deserialize<List<Region>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAllRegionData();
            await LoadAllCitiesData();
            return Page();
        }
        public async Task<IActionResult> OnPostCreateAsync()
        {
            ModelState.Remove("CityViewModel.RegionData.RegionName");
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["error"] = "Model State Validation Failed: " + string.Join("; ", errorMessages);
                await LoadAllRegionData();
                await LoadAllCitiesData();
                return Page();
            }
            try
            {
                _city = new City
                {
                    CityId = 0,
                    CityName = CityViewModel.CityName,
                    RegionData = new Region
                    {
                        RegionId = CityViewModel.RegionData.RegionId,
                        RegionName = ""
                    }
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/CreateCity";
                var jsonContent = JsonSerializer.Serialize(_city);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{CityViewModel.CityName} - Created Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllRegionData();
                    await LoadAllCitiesData();
                    TempData["error"] = $"{CityViewModel.CityName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (CityViewModel.CityId != null)
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
                var requestBody = new Record { RecordId = CityViewModel.CityId, RecordType = (int)FetchRecordByIdOrTextEnum.GetCityData };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _city = JsonSerializer.Deserialize<City>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    await LoadAllRegionData();
                    await LoadAllCitiesData();
                    ModelState.AddModelError("ProfileUsersData.UserId", $"Error fetching user record for {CityViewModel.CityName}. Please ensure the UserId is correct.");
                    TempData["error"] = $"Model State Validation Failed. Response status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }

                if (_city.CityName == CityViewModel.CityName)
                {
                    ModelState.Remove("CityViewModel.CityName");
                }
                if (CityViewModel.RegionData.RegionName == null|| _city.RegionData.RegionName == CityViewModel.RegionData.RegionName)
                {
                    ModelState.Remove("CityViewModel.RegionData.RegionName");
                }
            }
            if (!ModelState.IsValid)
            {
                await LoadAllRegionData(); // Reload ProfilesData if there's a validation error
                await LoadAllCitiesData();
                TempData["error"] = "Model State Validation Failed.";
                return Page();
            }
            try
            {
                _city = new City
                {
                    CityId = CityViewModel.CityId,
                    CityName = CityViewModel.CityName,
                    RegionData = new Region
                    {
                        RegionId = CityViewModel.RegionData.RegionId,
                        RegionName = ""
                    }
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateCity";
                var jsonContent = JsonSerializer.Serialize(_city);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{CityViewModel.CityName} - Profile Updated Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllRegionData();
                    await LoadAllCitiesData();
                    TempData["error"] = $"{CityViewModel.CityName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
        public async Task<IActionResult> OnPostDeleteAsync(int CityId)
        {
            if (CityId <= 0)
            {
                TempData["error"] = "User ID is required.";
                return RedirectToPage("/Error");
            }
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/DeleteEntity?deleteId={CityId}&deleteType={DeleteEntityType.City}";
                HttpResponseMessage response;
                response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"City - {CityId} deleted Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllRegionData();
                    await LoadAllCitiesData();
                    TempData["error"] = $"City - {CityId} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
