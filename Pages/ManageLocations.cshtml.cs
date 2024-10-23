using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Spider_QAMS.Pages
{
    public class ManageLocationsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private SiteLocation _siteLocation;
        public ManageLocationsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public IList<SiteLocationVM> LocationVMs { get; set; }
        public IList<RegionAssociatedCities> RegionAssociatedCitiesLst { get; set; }
        public IList<Sponsor> Sponsors { get; set; }
        [BindProperty]
        public SiteLocationVM siteLocationVM { get; set; }
        private async Task LoadAllLocationsData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllLocations");
            var locations = string.IsNullOrEmpty(response) ? new List<SiteLocation>() : JsonSerializer.Deserialize<List<SiteLocation>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            LocationVMs = locations.Select(loc => new SiteLocationVM
            {
                LocationId = loc.LocationId,
                Location = loc.Location,
                StreetName = loc.StreetName,
                CityId = loc.City.CityId,
                CityName = loc.City.CityName,
                RegionId = loc.City.RegionData.RegionId,
                RegionName = loc.City.RegionData.RegionName,
                DistrictName = loc.DistrictName,
                BranchName = loc.BranchName,
                SponsorId = loc.Sponsor.SponsorId,
                SponsorName = loc.Sponsor.SponsorName
            }).ToList();
        }
        private async Task LoadAllRegionListOfCitiesData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetRegionListOfCities");
            RegionAssociatedCitiesLst = string.IsNullOrEmpty(response) ? new List<RegionAssociatedCities>() : JsonSerializer.Deserialize<List<RegionAssociatedCities>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        private async Task LoadAllSponsorsData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllSponsors");
            Sponsors = string.IsNullOrEmpty(response) ? new List<Sponsor>() : JsonSerializer.Deserialize<List<Sponsor>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAllLocationsData();
            await LoadAllRegionListOfCitiesData();
            await LoadAllSponsorsData();
            return Page();
        }
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["error"] = "Model State Validation Failed: " + string.Join("; ", errorMessages);
                await LoadAllLocationsData();
                await LoadAllRegionListOfCitiesData();
                await LoadAllSponsorsData();
                return Page();
            }
            try
            {
                _siteLocation = new SiteLocation
                {
                    LocationId = 0,
                    Sponsor = new Sponsor
                    {
                        SponsorId = siteLocationVM.SponsorId,
                        SponsorName = ""
                    },
                    Location = siteLocationVM.Location,
                    BranchName = siteLocationVM.BranchName,
                    City = new City
                    {
                        CityId = siteLocationVM.CityId,
                        CityName = "",
                        RegionData = new Region
                        {
                            RegionId = siteLocationVM.RegionId,
                            RegionName = ""
                        }
                    },
                    DistrictName = siteLocationVM.DistrictName,
                    StreetName = siteLocationVM.StreetName
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/CreateLocation";
                var jsonContent = JsonSerializer.Serialize(_siteLocation);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{siteLocationVM.Location} - Created Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllLocationsData();
                    await LoadAllRegionListOfCitiesData();
                    await LoadAllSponsorsData();
                    TempData["error"] = $"{siteLocationVM.Location} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
            if (siteLocationVM.LocationId != null)
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
                var requestBody = new Record { RecordId = siteLocationVM.LocationId, RecordType = (int)FetchRecordByIdOrTextEnum.GetLocationData };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _siteLocation = JsonSerializer.Deserialize<SiteLocation>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    await LoadAllLocationsData();
                    await LoadAllRegionListOfCitiesData();
                    await LoadAllSponsorsData();
                    ModelState.AddModelError("ProfileUsersData.UserId", $"Error fetching user record for {siteLocationVM.Location}. Please ensure the UserId is correct.");
                    TempData["error"] = $"Model State Validation Failed. Response status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadAllLocationsData();
                await LoadAllRegionListOfCitiesData();
                await LoadAllSponsorsData();
                TempData["error"] = "Model State Validation Failed.";
                return Page();
            }
            try
            {
                _siteLocation = new SiteLocation
                {
                    LocationId = siteLocationVM.LocationId,
                    Sponsor = new Sponsor
                    {
                        SponsorId = siteLocationVM.SponsorId,
                        SponsorName = ""
                    },
                    Location = siteLocationVM.Location,
                    BranchName = siteLocationVM.BranchName,
                    City = new City
                    {
                        CityId = siteLocationVM.CityId,
                        CityName = "",
                        RegionData = new Region
                        {
                            RegionId = siteLocationVM.RegionId,
                            RegionName = ""
                        }
                    },
                    DistrictName = siteLocationVM.DistrictName,
                    StreetName = siteLocationVM.StreetName
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateLocation";
                var jsonContent = JsonSerializer.Serialize(_siteLocation);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{siteLocationVM.Location} - Profile Updated Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllLocationsData();
                    await LoadAllRegionListOfCitiesData();
                    await LoadAllSponsorsData();
                    TempData["error"] = $"{siteLocationVM.Location} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
        public async Task<IActionResult> OnPostDeleteAsync(int LocationId)
        {
            if (LocationId <= 0)
            {
                TempData["error"] = "User ID is required.";
                await LoadAllLocationsData();
                await LoadAllRegionListOfCitiesData();
                await LoadAllSponsorsData();
                return RedirectToPage("");
            }
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/DeleteEntity?deleteId={LocationId}&deleteType={DeleteEntityType.Location}";
                HttpResponseMessage response;
                response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"Location - {LocationId} deleted Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllLocationsData();
                    await LoadAllRegionListOfCitiesData();
                    await LoadAllSponsorsData();
                    TempData["error"] = $"Location - {LocationId} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
