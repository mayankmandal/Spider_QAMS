using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Spider_QAMS.Pages
{
    public class SiteDetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        public SiteDetailsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        // Dropdown Data Lists
        public IList<SponsorGroup> SponsorsList { get; set; }
        public IList<VisitStatusModel> VisitStatuses { get; set; }
        public IList<string> ATMClass {  get; set; }
        [BindProperty]
        public SiteDetailVM SiteDetailVM { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDropdownDataAsync();
            return Page();
        }
        // API call to load dropdown data
        private async Task LoadDropdownDataAsync()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));

            var responseATMClasses = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllATMClasses");
            ATMClass = string.IsNullOrEmpty(responseATMClasses) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(responseATMClasses, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var responseVisitStatuses = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllVisitStatuses");
            VisitStatuses = string.IsNullOrEmpty(responseVisitStatuses) ? new List<VisitStatusModel>() : JsonSerializer.Deserialize<List<VisitStatusModel>>(responseVisitStatuses, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch contacts and group by sponsor
            var responseContacts = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllContacts");
            var contacts = string.IsNullOrEmpty(responseContacts) ? new List<Contact>() : JsonSerializer.Deserialize<List<Contact>>(responseContacts, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch branch types and group by sponsor, sitetypes
            var responseBranchTypes = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllBranchTypes");
            var branches = string.IsNullOrEmpty(responseBranchTypes) ? new List<BranchType>() : JsonSerializer.Deserialize<List<BranchType>>(responseBranchTypes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch site locations and group by sponsors, regions, and cities
            var responseSiteTypes = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllLocations");
            var siteLocations = string.IsNullOrEmpty(responseSiteTypes) ? new List<SiteLocation>() : JsonSerializer.Deserialize<List<SiteLocation>>(responseSiteTypes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Grouping by sponsor
            SponsorsList = siteLocations
            .GroupBy(loc => loc.Sponsor.SponsorId)
            .Select(sponsorGroup => new SponsorGroup
            {
                SponsorId = sponsorGroup.Key,
                SponsorName = sponsorGroup.First().Sponsor.SponsorName, // Get the sponsor name from the first item in the group
                Regions = sponsorGroup
                    .GroupBy(loc => loc.City.RegionData.RegionId)
                    .Select(regionGroup => new RegionGroup
                    {
                        RegionId = regionGroup.Key,
                        RegionName = regionGroup.First().City.RegionData.RegionName, // Get the region name from the first item
                        Cities = regionGroup
                            .GroupBy(loc => loc.City.CityId)
                            .Select(cityGroup => new CityGroup
                            {
                                CityId = cityGroup.Key,
                                CityName = cityGroup.First().City.CityName, // Get the city name from the first item
                                Locations = cityGroup.ToList()
                            }).ToList()
                    }).ToList(),
                SiteTypes = branches
                .Where(b => b.siteType.sponsor.SponsorId == sponsorGroup.Key)
                .GroupBy(b => b.siteType.SiteTypeID)
                .Select(siteTypeGroup => new SiteTypeGroup
                {
                    SiteTypeId = siteTypeGroup.Key,
                    SiteTypeDescription = siteTypeGroup.First().siteType.Description,
                    BranchTypes = siteTypeGroup.ToList()
                }).ToList(),
                // Add Contacts List based on Sponsor
                ContactsList = contacts
                 .Where(c => c.sponsor.SponsorId == sponsorGroup.Key).ToList() // Filter contacts by sponsor
            }).ToList();
        }
        public async Task<IActionResult> OnPostCreateAsync()
        {
            return Page();
        }
    }
}
