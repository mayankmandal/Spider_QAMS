using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;

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
        [BindProperty]
        public SiteLocationVM siteLocationVM { get; set; }
        private async Task LoadAllLocationsData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllLocations");
            var locations = JsonSerializer.Deserialize<List<SiteLocation>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
                SponsorId = loc.SponsorId
            }).ToList();
        }
        private async Task LoadAllRegionListOfCitiesData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetRegionListOfCities");
            RegionAssociatedCitiesLst = JsonSerializer.Deserialize<List<RegionAssociatedCities>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAllLocationsData();
            await LoadAllRegionListOfCitiesData();
            return Page();
        }
    }
}
