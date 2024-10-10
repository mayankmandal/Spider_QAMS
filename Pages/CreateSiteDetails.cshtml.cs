using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace Spider_QAMS.Pages
{
    public class CreateSiteDetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private SiteDetail _siteDetail;
        public CreateSiteDetailsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
            _webHostEnvironment = webHostEnvironment;
        }
        // Dropdown Data Lists
        public IList<SponsorGroup> SponsorsList { get; set; }
        public IList<VisitStatusModel> VisitStatuses { get; set; }
        public IList<SitePicCategory> SitePicCategories { get; set; }
        public IList<string> ATMClass {  get; set; }
        [BindProperty]
        public SiteDetailVM SiteDetailVM { get; set; }
        [BindProperty]
        public List<SitePicCategoryVMAssociation> SitePicCategoryList { get; set; }
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

            var responsePicCategories = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllPicCategories");
            SitePicCategories = string.IsNullOrEmpty(responsePicCategories) ? new List<SitePicCategory>() : JsonSerializer.Deserialize<List<SitePicCategory>>(responsePicCategories, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
                    category.Images.Select(image => new SitePictures
                    {
                        SitePicID = image.SitePicID,
                        Description = image.Description ?? string.Empty,
                        PicPath = image.PicPath ?? string.Empty,
                        SitePicCategoryData = new SitePicCategory
                        {
                            PicCatID = category.PicCatID ?? 0,
                            Description = category.Description ?? string.Empty
                        }
                    })
                    ).ToList() ?? new List<SitePictures> // Default to an empty list with default values if null
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
                        string filePath = null;
                        foreach (var category in SitePicCategoryList)
                        {
                            // Append category description as folder name
                            string categoryFolder = Path.Combine(_webHostEnvironment.WebRootPath, _configuration["SiteDetailImgPath"], category.Description);

                            // Create the folder if it doesn't exist
                            if(!Directory.Exists(categoryFolder))
                            {
                                Directory.CreateDirectory(categoryFolder);
                            }

                            foreach (var image in category.Images)
                            {
                                if (image.PicPathFile != null && image.PicPathFile.Length > 0) // Assuming you are using IFormFile for image upload
                                {
                                    // Fetching the unique generated file name using SiteCode, SiteTypeId, SiteId, and PicName
                                    var sitePicture = createdSite.SitePicturesLst.Where(si => si.PicPath.Contains(image.PicPath)).FirstOrDefault();
                                    if(sitePicture != null)
                                    {
                                        // Build the complete file path using category folder and unique file name
                                        filePath = Path.Combine(categoryFolder, sitePicture.PicPath);
                                    }

                                    // Copy the file to the server using FileStream
                                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await image.PicPathFile.CopyToAsync(fileStream);
                                    }
                                }
                            }
                        }
                    }

                    TempData["success"] = $"{SiteDetailVM.SiteName} - Created Successfully with Images Uploaded";
                    return RedirectToPage();
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
