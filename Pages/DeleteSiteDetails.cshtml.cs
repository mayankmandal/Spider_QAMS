using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Spider_QAMS.Pages
{
    [Authorize(Policy = "PageAccess")]
    public class DeleteSiteDetailsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private SiteDetail _siteDetail;
        public DeleteSiteDetailsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        [BindProperty]
        public SiteDetailVM SiteDetailVM { get; set; }
        
        public async Task<IActionResult> OnGetAsync(string siteId)
        {
            if (string.IsNullOrEmpty(siteId))
            {
                return RedirectToPage("/Error");
            }
            await LoadSiteDetailsDataAsync(siteId);
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
                    SponsorName = _siteDetail.SponsorName != null ? _siteDetail.SponsorName : "Sponsor's not present",
                    RegionID = _siteDetail.RegionID,
                    RegionName = _siteDetail.RegionName != null ? _siteDetail.RegionName : "Region's Data not present",
                    CityID = _siteDetail.CityID,
                    CityName = _siteDetail.CityName != null ? _siteDetail.CityName : "City's Data not present",
                    LocationID = _siteDetail.LocationID,
                    LocationName = _siteDetail.LocationName != null ? _siteDetail.LocationName : "Location's Data not present",
                    ContactID = _siteDetail.ContactID,
                    ContactName = _siteDetail.ContactName != null ? _siteDetail.ContactName : "Contact's Data not present",
                    SiteTypeID = _siteDetail.SiteTypeID,
                    SiteTypeDescription = _siteDetail.SiteTypeDescription != null ? _siteDetail.SiteTypeDescription : "Site Type's not present",
                    GPSLong = _siteDetail.GPSLong ?? string.Empty,
                    GPSLatt = _siteDetail.GPSLatt ?? string.Empty,
                    VisitUserID = _siteDetail.VisitUserID,
                    VisitedDate = _siteDetail.VisitedDate,
                    ApprovedUserID = _siteDetail.ApprovedUserID,
                    ApprovalDate = _siteDetail.ApprovalDate,
                    VisitStatusID = _siteDetail.VisitStatusID,
                    IsActive = _siteDetail.IsActive,
                    BranchNo = _siteDetail.BranchNo != null ? _siteDetail.BranchNo : "Branch Number's data not present",
                    BranchTypeId = _siteDetail.BranchTypeId != null ? _siteDetail.BranchTypeId : -1,
                    BranchTypeDescription = _siteDetail.BranchTypeDescription != string.Empty? _siteDetail.BranchTypeDescription : "Branch Type's Data not present",
                    AtmClass = _siteDetail.AtmClass != null ? _siteDetail.AtmClass : "ATM Class's data not present",

                    // Populate Site Pictures List for rendering
                    SitePicturesLst = _siteDetail.SitePicturesLst?.Select(p => new SitePicturesVM
                    {
                        SitePicID = p.SitePicID,
                        Description = p.Description ?? string.Empty,
                        PicPath = Path.Combine(
                            _configuration["BaseUrl"],
                            _configuration["SiteDetailImgPath"],
                            p.SitePicCategoryData.PicCatID.ToString(),
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

        public async Task<IActionResult> OnPostAsync(string siteId)
        {
            if (string.IsNullOrEmpty(siteId))
            {
                TempData["error"] = "Site ID is required.";
                return RedirectToPage("/Error");
            }
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/DeleteEntity?deleteId={siteId}&deleteType={DeleteEntityType.SiteDetail}";
                HttpResponseMessage response;
                response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"Site - {siteId} deleted Successfully";
                    return RedirectToPage("/ManageSiteDetails");
                }
                else
                {
                    TempData["error"] = $"Site - {siteId} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
