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
    public class ViewSiteDetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private SiteDetail _siteDetail;
        public ViewSiteDetailsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        // Dropdown Data Lists
        public IList<SponsorGroup> SponsorsList { get; set; }
        public IList<VisitStatusModel> VisitStatuses { get; set; }
        public IList<SitePicCategory> SitePicCategories { get; set; }
        public IList<string> ATMClass { get; set; }
        [BindProperty]
        public SiteDetailVM SiteDetailVM { get; set; }
        [BindProperty]
        public List<SitePicCategoryVMAssociation> SitePicCategoryList { get; set; }

        public async Task<IActionResult> OnGetAsync(string siteId)
        {
            if (string.IsNullOrEmpty(siteId))
            {
                return RedirectToPage("/Error");
            }
            await LoadSiteDetailsDataAsync(siteId);
            await LoadDropdownDataAsync();
            return Page();
        }
        private async Task LoadSiteDetailsDataAsync(string SiteId)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
            var requestBody = new Record { RecordId = Convert.ToInt32(SiteId), RecordType = (int)FetchRecordByIdOrTextEnum.GetSiteDetail };
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _siteDetail = JsonSerializer.Deserialize<SiteDetail>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SiteDetailVM = new SiteDetailVM()
                {
                    SiteID = _siteDetail.SiteID,
                    SiteCode = _siteDetail.SiteCode ?? string.Empty,
                    SiteName = _siteDetail.SiteName ?? string.Empty,
                    SiteCategory = _siteDetail.SiteCategory ?? string.Empty,
                    SponsorID = _siteDetail.SponsorID,
                    RegionID = _siteDetail.RegionID,
                    CityID = _siteDetail.CityID,
                    LocationID = _siteDetail.LocationID,
                    ContactID = _siteDetail.ContactID,
                    SiteTypeID = _siteDetail.SiteTypeID,
                    GPSLong = _siteDetail.GPSLong ?? string.Empty,
                    GPSLatt = _siteDetail.GPSLatt ?? string.Empty,
                    VisitUserID = _siteDetail.VisitUserID,
                    VisitedDate = _siteDetail.VisitedDate,
                    ApprovedUserID = _siteDetail.ApprovedUserID,
                    ApprovalDate = _siteDetail.ApprovalDate,
                    VisitStatusID = _siteDetail.VisitStatusID,
                    IsActive = _siteDetail.IsActive,
                    BranchNo = _siteDetail.BranchNo ?? string.Empty,
                    BranchTypeId = _siteDetail.BranchTypeId,
                    AtmClass = _siteDetail.AtmClass ?? string.Empty,

                    // Populate Contact Information
                    SiteContactInformation = new SiteContactInformationVM
                    {
                        BranchTelephoneNumber = _siteDetail.ContactInformation?.BranchTelephoneNumber ?? string.Empty,
                        BranchFaxNumber = _siteDetail.ContactInformation?.BranchFaxNumber ?? string.Empty,
                        EmailAddress = _siteDetail.ContactInformation?.EmailAddress ?? string.Empty
                    },

                    // Populate Geographical Details
                    GeographicalDetails = new GeographicalDetailsVM
                    {
                        NearestLandmark = _siteDetail.GeographicalDetails?.NearestLandmark ?? string.Empty,
                        NumberOfKmNearestCity = _siteDetail.GeographicalDetails?.NumberOfKmNearestCity ?? string.Empty,
                        BranchConstructionType = _siteDetail.GeographicalDetails?.BranchConstructionType ?? string.Empty,
                        BranchIsLocatedAt = _siteDetail.GeographicalDetails?.BranchIsLocatedAt ?? string.Empty,
                        HowToReachThere = _siteDetail.GeographicalDetails?.HowToReachThere ?? string.Empty,
                        SiteIsOnServiceRoad = _siteDetail.GeographicalDetails?.SiteIsOnServiceRoad ?? false,
                        HowToGetThere = _siteDetail.GeographicalDetails?.HowToGetThere ?? string.Empty
                    },

                    // Populate Branch Facilities
                    SiteBranchFacilities = new SiteBranchFacilitiesVM
                    {
                        Parking = _siteDetail.BranchFacilities?.Parking ?? false,
                        Landscape = _siteDetail.BranchFacilities?.Landscape ?? false,
                        Elevator = _siteDetail.BranchFacilities?.Elevator ?? false,
                        VIPSection = _siteDetail.BranchFacilities?.VIPSection ?? false,
                        SafeBox = _siteDetail.BranchFacilities?.SafeBox ?? false,
                        ICAP = _siteDetail.BranchFacilities?.ICAP ?? false
                    },

                    // Populate Data Center Information
                    SiteDataCenter = new SiteDataCenterVM
                    {
                        UPSBrand = _siteDetail.DataCenter?.UPSBrand ?? string.Empty,
                        UPSCapacity = _siteDetail.DataCenter?.UPSCapacity ?? string.Empty,
                        PABXBrand = _siteDetail.DataCenter?.PABXBrand ?? string.Empty,
                        StabilizerBrand = _siteDetail.DataCenter?.StabilizerBrand ?? string.Empty,
                        StabilizerCapacity = _siteDetail.DataCenter?.StabilizerCapacity ?? string.Empty,
                        SecurityAccessSystemBrand = _siteDetail.DataCenter?.SecurityAccessSystemBrand ?? string.Empty
                    },

                    // Populate Sign Board Type
                    SignBoardType = new SignBoardTypeVM
                    {
                        Cylinder = _siteDetail.SignBoard?.Cylinder ?? false,
                        StraightOrTotem = _siteDetail.SignBoard?.StraightOrTotem ?? false
                    },

                    // Populate Miscellaneous Information
                    SiteMiscInformation = new SiteMiscInformationVM
                    {
                        TypeOfATMLocation = _siteDetail.MiscSiteInfo?.TypeOfATMLocation ?? string.Empty,
                        NoOfExternalCameras = _siteDetail.MiscSiteInfo?.NoOfExternalCameras,
                        NoOfInternalCameras = _siteDetail.MiscSiteInfo?.NoOfInternalCameras,
                        TrackingSystem = _siteDetail.MiscSiteInfo?.TrackingSystem ?? string.Empty
                    },
                    // Populate Branch Miscellaneous Information
                    BranchMiscInformation = new BranchMiscInformationVM
                    {
                        NoOfCleaners = _siteDetail.MiscBranchInfo?.NoOfCleaners,
                        FrequencyOfDailyMailingService = _siteDetail.MiscBranchInfo?.FrequencyOfDailyMailingService,
                        ElectricSupply = _siteDetail.MiscBranchInfo?.ElectricSupply ?? string.Empty,
                        WaterSupply = _siteDetail.MiscBranchInfo?.WaterSupply ?? string.Empty,
                        BranchOpenDate = _siteDetail.MiscBranchInfo?.BranchOpenDate,
                        TellersCounter = _siteDetail.MiscBranchInfo?.TellersCounter,
                        NoOfSalesManagerOffices = _siteDetail.MiscBranchInfo?.NoOfSalesManagerOffices,
                        ExistVIPSection = _siteDetail.MiscBranchInfo?.ExistVIPSection ?? false,
                        ContractStartDate = _siteDetail.MiscBranchInfo?.ContractStartDate,
                        NoOfRenovationRetouchTime = _siteDetail.MiscBranchInfo?.NoOfRenovationRetouchTime,
                        LeasedOwBuilding = _siteDetail.MiscBranchInfo?.LeasedOwBuilding ?? false,
                        NoOfTeaBoys = _siteDetail.MiscBranchInfo?.NoOfTeaBoys,
                        FrequencyOfMonthlyCleaningService = _siteDetail.MiscBranchInfo?.FrequencyOfMonthlyCleaningService,
                        DrainSewerage = _siteDetail.MiscBranchInfo?.DrainSewerage ?? string.Empty,
                        CentralAC = _siteDetail.MiscBranchInfo?.CentralAC ?? false,
                        SplitAC = _siteDetail.MiscBranchInfo?.SplitAC ?? false,
                        WindowAC = _siteDetail.MiscBranchInfo?.WindowAC ?? false,
                        CashCounterType = _siteDetail.MiscBranchInfo?.CashCounterType,
                        NoOfTellerCounters = _siteDetail.MiscBranchInfo?.NoOfTellerCounters,
                        NoOfAffluentRelationshipManagerOffices = _siteDetail.MiscBranchInfo?.NoOfAffluentRelationshipManagerOffices,
                        SeparateVIPSection = _siteDetail.MiscBranchInfo?.SeperateVIPSection ?? false,
                        ContractEndDate = _siteDetail.MiscBranchInfo?.ContractEndDate,
                        RenovationRetouchDate = _siteDetail.MiscBranchInfo?.RenovationRetouchDate,
                        NoOfTCRMachines = _siteDetail.MiscBranchInfo?.NoOfTCRMachines,
                        NoOfTotem = _siteDetail.MiscBranchInfo?.NoOfTotem
                    }
                };
            }
            else
            {
                throw new Exception($"Error fetching user record: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        // API call to load dropdown data
        private async Task LoadDropdownDataAsync()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));

            var responseATMClasses = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllATMClasses");
            ATMClass = string.IsNullOrEmpty(responseATMClasses) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(responseATMClasses, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var responsePicCategories = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllPicCategories");
            SitePicCategories = string.IsNullOrEmpty(responsePicCategories) ? new List<SitePicCategory>() : JsonSerializer.Deserialize<List<SitePicCategory>>(responsePicCategories, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            SitePicCategoryList = SitePicCategories.Select(category => new SitePicCategoryVMAssociation
            {
                PicCatID = category.PicCatID,
                Description = category.Description,
                Images = new List<IFormFile>(),
                ImagePaths = _siteDetail.SitePicturesLst?
                .Where(p => p.SitePicCategoryData?.PicCatID == category.PicCatID)
                .Select(p => Path.Combine(
                    _configuration["BaseUrl"],
                    _configuration["SiteDetailImgPath"],
                    category.Description,
                    p.PicPath ?? string.Empty
                )).ToList() ?? new List<string>(),
                ImageComments = _siteDetail.SitePicturesLst?
                    .Where(p => p.SitePicCategoryData?.PicCatID == category.PicCatID)
                    .Select(p => p.Description ?? string.Empty)
                    .ToList() ?? new List<string>()
            }).ToList();

            var responseVisitStatuses = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllVisitStatuses");
            VisitStatuses = string.IsNullOrEmpty(responseVisitStatuses) ? new List<VisitStatusModel>() : JsonSerializer.Deserialize<List<VisitStatusModel>>(responseVisitStatuses, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch contacts and group by sponsor
            var responseContacts = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllContacts");
            var contacts = string.IsNullOrEmpty(responseContacts) ? new List<Contact>() : JsonSerializer.Deserialize<List<Contact>>(responseContacts, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch branch types and group by sponsor, sitetypes
            var responseSiteTypes = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllSiteTypes");
            var siteTypes = string.IsNullOrEmpty(responseSiteTypes) ? new List<SiteType>() : JsonSerializer.Deserialize<List<SiteType>>(responseSiteTypes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch branch types and group by sponsor, sitetypes
            var responseBranchTypes = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllBranchTypes");
            var branches = string.IsNullOrEmpty(responseBranchTypes) ? new List<BranchType>() : JsonSerializer.Deserialize<List<BranchType>>(responseBranchTypes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Fetch site locations and group by sponsors, regions, and cities
            var responseLocations = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllLocations");
            var siteLocations = string.IsNullOrEmpty(responseLocations) ? new List<SiteLocation>() : JsonSerializer.Deserialize<List<SiteLocation>>(responseLocations, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
                SiteTypes = siteTypes
                .Where(b => b.sponsor.SponsorId == sponsorGroup.Key)
                .GroupBy(b => b.SiteTypeID)
                .Select(siteTypeGroup => new SiteTypeGroup
                {
                    SiteTypeId = siteTypeGroup.Key,
                    SiteTypeDescription = siteTypeGroup.First().Description,
                    BranchTypes = branches.Where(cv => cv.siteType.SiteTypeID == siteTypeGroup.Key).ToList()
                }).ToList(),
                // Add Contacts List based on Sponsor
                ContactsList = contacts
                 .Where(c => c.sponsor.SponsorId == sponsorGroup.Key).ToList() // Filter contacts by sponsor
            }).ToList();
        }
    }
}
