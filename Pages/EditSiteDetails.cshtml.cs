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
    public class EditSiteDetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private SiteDetail _siteDetail;
        public EditSiteDetailsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
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

                    // Populate Site Pictures List for rendering
                    SitePicturesLst = _siteDetail.SitePicturesLst?.Select(p => new SitePicturesVM
                    {
                        SitePicID = p.SitePicID,
                        Description = p.Description ?? string.Empty,
                        PicPath = Path.Combine(
                            _configuration["BaseUrl"],
                            _configuration["SiteDetailImgPath"],
                            p.SitePicCategoryData.Description,
                            p.PicPath) ?? string.Empty,
                        SitePicCategoryVMData = new SitePicCategoryVM
                        {
                            PicCatID = p.SitePicCategoryData?.PicCatID,
                            Description = p.SitePicCategoryData?.Description ?? string.Empty
                        }
                    }).ToList(),

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
                ImagesPath = _siteDetail.SitePicturesLst?
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
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("SiteDetailVM.SitePicturesLst");
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["error"] = "Model State Validation Failed: " + string.Join("; ", errorMessages);
                await LoadDropdownDataAsync();
                return Page();
            }
            try
            {
                _siteDetail = new SiteDetail
                {
                    SiteID = 0,  // Assuming SiteID is set during editing
                    SiteCode = SiteDetailVM.SiteCode == null ? string.Empty : SiteDetailVM.SiteCode,  // Assuming SiteCode is under SiteBranchFacilities
                    SiteName = SiteDetailVM.SiteName == null ? string.Empty : SiteDetailVM.SiteName,
                    SiteCategory = SiteDetailVM.SiteCategory == null ? string.Empty : SiteDetailVM.SiteCategory,
                    SponsorID = SiteDetailVM.SponsorID == null ? 0 : SiteDetailVM.SponsorID,
                    RegionID = SiteDetailVM.RegionID == null ? 0 : SiteDetailVM.RegionID,
                    CityID = SiteDetailVM.CityID == null ? 0 : SiteDetailVM.CityID,
                    LocationID = SiteDetailVM.LocationID == null ? 0 : SiteDetailVM.LocationID,
                    ContactID = SiteDetailVM.ContactID == null ? 0 : SiteDetailVM.ContactID,
                    SiteTypeID = SiteDetailVM.SiteTypeID == null ? 0 : SiteDetailVM.SiteTypeID,
                    GPSLong = SiteDetailVM.GPSLong == null ? string.Empty : SiteDetailVM.GPSLong,
                    GPSLatt = SiteDetailVM.GPSLatt == null ? string.Empty : SiteDetailVM.GPSLatt,
                    VisitUserID = SiteDetailVM.VisitUserID == null ? 0 : SiteDetailVM.VisitUserID,
                    VisitedDate = SiteDetailVM.VisitedDate == null ? DateTime.MinValue : SiteDetailVM.VisitedDate,
                    ApprovedUserID = SiteDetailVM.ApprovedUserID == null ? 0 : SiteDetailVM.ApprovedUserID,
                    ApprovalDate = SiteDetailVM.ApprovalDate == null ? DateTime.MinValue : SiteDetailVM.ApprovalDate,
                    VisitStatusID = SiteDetailVM.VisitStatusID == null ? 0 : SiteDetailVM.VisitStatusID,
                    IsActive = SiteDetailVM.IsActive == null ? false : SiteDetailVM.IsActive,
                    AtmClass = SiteDetailVM.AtmClass == null ? string.Empty : SiteDetailVM.AtmClass, // ATM Class
                    BranchNo = SiteDetailVM.BranchNo == null ? string.Empty : SiteDetailVM.BranchNo,
                    BranchTypeId = SiteDetailVM.BranchTypeId == null ? 0 : SiteDetailVM.BranchTypeId,

                    // Map child objects with null handling
                    ContactInformation = new SiteContactInformation
                    {
                        BranchTelephoneNumber = SiteDetailVM.SiteContactInformation?.BranchTelephoneNumber ?? string.Empty,
                        BranchFaxNumber = SiteDetailVM.SiteContactInformation?.BranchFaxNumber ?? string.Empty,
                        EmailAddress = SiteDetailVM.SiteContactInformation?.EmailAddress ?? string.Empty,
                    },
                    GeographicalDetails = new GeographicalDetails
                    {
                        NearestLandmark = SiteDetailVM.GeographicalDetails?.NearestLandmark ?? string.Empty,
                        NumberOfKmNearestCity = SiteDetailVM.GeographicalDetails?.NumberOfKmNearestCity ?? string.Empty,
                        BranchConstructionType = SiteDetailVM.GeographicalDetails?.BranchConstructionType ?? string.Empty,
                        BranchIsLocatedAt = SiteDetailVM.GeographicalDetails?.BranchIsLocatedAt ?? string.Empty,
                        HowToReachThere = SiteDetailVM.GeographicalDetails?.HowToReachThere ?? string.Empty,
                        SiteIsOnServiceRoad = SiteDetailVM.GeographicalDetails?.SiteIsOnServiceRoad ?? false,
                        HowToGetThere = SiteDetailVM.GeographicalDetails?.HowToGetThere ?? string.Empty,
                    },
                    BranchFacilities = new SiteBranchFacilities
                    {
                        Parking = SiteDetailVM.SiteBranchFacilities?.Parking ?? false,
                        Landscape = SiteDetailVM.SiteBranchFacilities?.Landscape ?? false,
                        Elevator = SiteDetailVM.SiteBranchFacilities?.Elevator ?? false,
                        VIPSection = SiteDetailVM.SiteBranchFacilities?.VIPSection ?? false,
                        SafeBox = SiteDetailVM.SiteBranchFacilities?.SafeBox ?? false,
                        ICAP = SiteDetailVM.SiteBranchFacilities?.ICAP ?? false,
                    },
                    DataCenter = new SiteDataCenter
                    {
                        UPSBrand = SiteDetailVM.SiteDataCenter?.UPSBrand ?? string.Empty,
                        UPSCapacity = SiteDetailVM.SiteDataCenter?.UPSCapacity ?? string.Empty,
                        PABXBrand = SiteDetailVM.SiteDataCenter?.PABXBrand ?? string.Empty,
                        StabilizerBrand = SiteDetailVM.SiteDataCenter?.StabilizerBrand ?? string.Empty,
                        StabilizerCapacity = SiteDetailVM.SiteDataCenter?.StabilizerCapacity ?? string.Empty,
                        SecurityAccessSystemBrand = SiteDetailVM.SiteDataCenter?.SecurityAccessSystemBrand ?? string.Empty,
                    },
                    SignBoard = new SignBoardType
                    {
                        Cylinder = SiteDetailVM.SignBoardType?.Cylinder ?? false,
                        StraightOrTotem = SiteDetailVM.SignBoardType?.StraightOrTotem ?? false,
                    },
                    MiscBranchInfo = new BranchMiscInformation
                    {
                        NoOfCleaners = SiteDetailVM.BranchMiscInformation?.NoOfCleaners ?? 0,
                        FrequencyOfDailyMailingService = SiteDetailVM.BranchMiscInformation?.FrequencyOfDailyMailingService ?? 0,
                        ElectricSupply = SiteDetailVM.BranchMiscInformation?.ElectricSupply ?? string.Empty,
                        WaterSupply = SiteDetailVM.BranchMiscInformation?.WaterSupply ?? string.Empty,
                        BranchOpenDate = SiteDetailVM.BranchMiscInformation?.BranchOpenDate ?? DateTime.MinValue,
                        TellersCounter = SiteDetailVM.BranchMiscInformation?.TellersCounter ?? 0,
                        NoOfSalesManagerOffices = SiteDetailVM.BranchMiscInformation?.NoOfSalesManagerOffices ?? 0,
                        ExistVIPSection = SiteDetailVM.BranchMiscInformation?.ExistVIPSection ?? false,
                        ContractStartDate = SiteDetailVM.BranchMiscInformation?.ContractStartDate ?? DateTime.MinValue,
                        NoOfRenovationRetouchTime = SiteDetailVM.BranchMiscInformation?.NoOfRenovationRetouchTime ?? 0,
                        LeasedOwBuilding = SiteDetailVM.BranchMiscInformation?.LeasedOwBuilding ?? false,
                        NoOfTeaBoys = SiteDetailVM.BranchMiscInformation?.NoOfTeaBoys ?? 0,
                        FrequencyOfMonthlyCleaningService = SiteDetailVM.BranchMiscInformation?.FrequencyOfMonthlyCleaningService ?? 0,
                        DrainSewerage = SiteDetailVM.BranchMiscInformation?.DrainSewerage ?? string.Empty,
                        CentralAC = SiteDetailVM.BranchMiscInformation?.CentralAC ?? false,
                        SplitAC = SiteDetailVM.BranchMiscInformation?.SplitAC ?? false,
                        WindowAC = SiteDetailVM.BranchMiscInformation?.WindowAC ?? false,
                        CashCounterType = SiteDetailVM.BranchMiscInformation?.CashCounterType ?? 0,
                        NoOfTellerCounters = SiteDetailVM.BranchMiscInformation?.NoOfTellerCounters ?? 0,
                        NoOfAffluentRelationshipManagerOffices = SiteDetailVM.BranchMiscInformation?.NoOfAffluentRelationshipManagerOffices ?? 0,
                        SeperateVIPSection = SiteDetailVM.BranchMiscInformation?.SeparateVIPSection ?? false,
                        ContractEndDate = SiteDetailVM.BranchMiscInformation?.ContractEndDate ?? DateTime.MinValue,
                        RenovationRetouchDate = SiteDetailVM.BranchMiscInformation?.RenovationRetouchDate ?? DateTime.MinValue,
                        NoOfTCRMachines = SiteDetailVM.BranchMiscInformation?.NoOfTCRMachines ?? 0,
                        NoOfTotem = SiteDetailVM.BranchMiscInformation?.NoOfTotem ?? 0,
                    },
                    MiscSiteInfo = new SiteMiscInformation
                    {
                        TypeOfATMLocation = SiteDetailVM.SiteMiscInformation?.TypeOfATMLocation ?? string.Empty,
                        NoOfExternalCameras = SiteDetailVM.SiteMiscInformation?.NoOfExternalCameras ?? 0,
                        NoOfInternalCameras = SiteDetailVM.SiteMiscInformation?.NoOfInternalCameras ?? 0,
                        TrackingSystem = SiteDetailVM.SiteMiscInformation?.TrackingSystem ?? string.Empty,
                    },
                    // Initialize SitePicturesLst from SitePicCategoryList
                    SitePicturesLst = SitePicCategoryList.SelectMany(category =>
                    category.Images.Select((image, index) => new SitePictures
                    {
                        Description = category.ImageComments.ElementAtOrDefault(index) ?? string.Empty,
                        PicPath = image.FileName,
                        SitePicCategoryData = new SitePicCategory
                        {
                            PicCatID = category.PicCatID ?? 0,
                            Description = category.Description ?? string.Empty
                        }
                    })).ToList() ?? new List<SitePictures> // Default to an empty list with default values if null
                    {
                        new SitePictures
                        {
                            SitePicID = 0,
                            Description = string.Empty,
                            PicPath = string.Empty,
                            SitePicCategoryData = new SitePicCategory
                            {
                                PicCatID = 0,
                                Description = string.Empty
                            }
                        }
                    }
                };

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/CreateSiteDetails";
                var jsonContent = JsonSerializer.Serialize(_siteDetail);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    // Successfully created the site, now upload images
                    var siteDetailResponse = await response.Content.ReadAsStringAsync();
                    var createdSite = JsonSerializer.Deserialize<SiteDetail>(siteDetailResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Upload images after the site is created
                    if (SitePicCategoryList != null && SitePicCategoryList.Any())
                    {
                        foreach (var category in SitePicCategoryList)
                        {
                            // Ensure the category description exists
                            if (string.IsNullOrWhiteSpace(category.Description))
                                continue;

                            // Create the folder path based on category description
                            string categoryFolder = Path.Combine(
                                _webHostEnvironment.WebRootPath,
                                _configuration["SiteDetailImgPath"],
                                category.Description
                            );

                            // Create the folder if it doesn't exist
                            if (!Directory.Exists(categoryFolder))
                            {
                                Directory.CreateDirectory(categoryFolder);
                            }

                            foreach (var image in category.Images)
                            {
                                if (image != null && image.Length > 0)
                                {
                                    // Extract file name from the uploaded image
                                    string uploadedFileName = Path.GetFileName(image.FileName);

                                    // Find the matching SitePicture entry based on PicPath containing the uploaded file name
                                    var sitePicture = createdSite.SitePicturesLst
                                        .FirstOrDefault(sp => sp.PicPath.Contains(uploadedFileName));

                                    if (sitePicture != null)
                                    {
                                        // Build the complete file path
                                        string filePath = Path.Combine(categoryFolder, sitePicture.PicPath);

                                        // Copy the file to the appropriate path on the server
                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await image.CopyToAsync(fileStream);
                                        }
                                    }
                                    else
                                    {
                                        // Handle the case where the picture is not found (optional logging or error handling)
                                        Console.WriteLine($"No matching entry found for file: {uploadedFileName}");
                                    }
                                }
                            }
                        }
                    }

                    TempData["success"] = $"{SiteDetailVM.SiteName} - Created Successfully with Images Uploaded";
                    return RedirectToPage("/ManageSiteDetails");
                }
                else
                {
                    await LoadDropdownDataAsync();
                    TempData["error"] = $"{SiteDetailVM.SiteName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
        private IActionResult HandleError(Exception ex, string errorMessage)
        {
            TempData["error"] = $"Error Message - " + errorMessage + ". Error details: " + ex.Message;
            return Page();
        }
    }
}
