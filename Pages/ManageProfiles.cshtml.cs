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
    public class ManageProfilesModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private ProfilePagesAccess _profilePagesAccess;
        public ManageProfilesModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public IList<ProfileSiteVM> AllProfiles { get; set; }
        public IList<PageSiteVM> AllPages { get; set; }
        public IList<ProfilePagesAccessDTO> AllProfilePages { get; set; }
        [BindProperty]
        public ProfileSiteVM profileSiteVM { get; set; }
        [BindProperty]
        public string? SelectedPagesJson { get; set; }
        private async Task LoadAllProfilesData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllProfiles");
            var profiles = string.IsNullOrEmpty(response) ? new List<ProfileSite>() : JsonSerializer.Deserialize<List<ProfileSite>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            AllProfiles = profiles.Select(profile => new ProfileSiteVM
            {
                ProfileId = profile.ProfileId,
                ProfileName = profile.ProfileName
            }).ToList();
        }
        private async Task LoadAllPagesData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllPages");
            var pages = string.IsNullOrEmpty(response) ? new List<PageSite>() : JsonSerializer.Deserialize<List<PageSite>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            AllPages = pages.Select(page => new PageSiteVM
            {
                PageId = page.PageId,
                IsSelected = page.isSelected,
                PageDesc = page.PageDesc,
                PageUrl = page.PageUrl
            }).ToList();
        }
        private async Task LoadAllProfilePagesData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllProfilePages");
            var profilepages = string.IsNullOrEmpty(response) ? new List<ProfilePagesAccess>() : JsonSerializer.Deserialize<List<ProfilePagesAccess>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            AllProfilePages = profilepages.Select(eachprofile => new ProfilePagesAccessDTO
            {
                Profile = new ProfileSiteVM
                {
                    ProfileId = eachprofile.Profile.ProfileId,
                    ProfileName = eachprofile.Profile.ProfileName
                },
                Pages = eachprofile.Pages.Select(eachpage => new PageSiteVM
                {
                    PageId = eachpage.PageId,
                    PageUrl = eachpage.PageUrl,
                    PageDesc = eachpage.PageDesc,
                    IsSelected = eachpage.isSelected,
                }).ToList(),
            }).ToList();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAllProfilesData();
            await LoadAllPagesData();
            await LoadAllProfilePagesData();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["error"] = "Model State Validation Failed: " + string.Join("; ", errorMessages);
                await LoadAllProfilesData();
                await LoadAllPagesData();
                await LoadAllProfilePagesData();
                return Page();
            }
            try
            {
                // Deserialize the Json string into a list of PageSite objects
                List<PageSiteVM> selectedPages = JsonSerializer.Deserialize<List<PageSiteVM>>(SelectedPagesJson);

                _profilePagesAccess = new ProfilePagesAccess
                {
                    Profile = new ProfileSite
                    {
                        ProfileName = profileSiteVM.ProfileName,
                        ProfileId = profileSiteVM.ProfileId
                    },
                    // Filter only the pages where IsSelected is true
                    Pages = selectedPages
                    .Where(page => page.IsSelected) // Only select pages where IsSelected is true
                    .Select(page => new PageSite
                    {
                        isSelected = page.IsSelected,
                        PageDesc = page.PageDesc,
                        PageId = page.PageId,
                        PageUrl = page.PageUrl,
                        PageCatId = -1,
                        PageImgUrl = string.Empty,
                        PageName = string.Empty,
                        PageSeq = -1
                    }).ToList()
                };

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/CreateProfile";
                var jsonContent = JsonSerializer.Serialize(_profilePagesAccess);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{profileSiteVM.ProfileName} - Created Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllProfilesData();
                    await LoadAllPagesData();
                    await LoadAllProfilePagesData();
                    TempData["error"] = $"{profileSiteVM.ProfileName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
            if (profileSiteVM.ProfileId != null)
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
                var requestBody = new Record { RecordId = profileSiteVM.ProfileId, RecordType = (int)FetchRecordByIdOrTextEnum.GetProfilePagesData };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _profilePagesAccess = JsonSerializer.Deserialize<ProfilePagesAccess>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    await LoadAllProfilesData();
                    await LoadAllPagesData();
                    await LoadAllProfilePagesData();

                    ModelState.AddModelError("ProfileUsersData.UserId", $"Error fetching user record for {profileSiteVM.ProfileName}. Please ensure the UserId is correct.");
                    TempData["error"] = $"Model State Validation Failed. Response status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }

                if (_profilePagesAccess.Profile.ProfileName == profileSiteVM.ProfileName)
                {
                    ModelState.Remove("profileSiteVM.ProfileName");
                }
            }
            if (!ModelState.IsValid)
            {
                await LoadAllProfilesData();
                await LoadAllPagesData();
                await LoadAllProfilePagesData();
                TempData["error"] = "Model State Validation Failed.";
                return Page();
            }
            try
            {
                // Deserialize the Json string into a list of PageSite objects
                List<PageSiteVM> selectedPages = JsonSerializer.Deserialize<List<PageSiteVM>>(SelectedPagesJson);

                _profilePagesAccess = new ProfilePagesAccess
                {
                    Profile = new ProfileSite
                    {
                        ProfileName = profileSiteVM.ProfileName,
                        ProfileId = _profilePagesAccess.Profile.ProfileId
                    },
                    // Filter only the pages where IsSelected is true
                    Pages = selectedPages
                    .Where(page => page.IsSelected) // Only select pages where IsSelected is true
                    .Select(page => new PageSite
                    {
                        isSelected = true,
                        PageDesc = page.PageDesc,
                        PageId = page.PageId,
                        PageUrl = page.PageUrl,
                        PageCatId = -1,
                        PageImgUrl = string.Empty,
                        PageName = string.Empty,
                        PageSeq = -1
                    }).ToList()
                };

                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateProfile";
                var jsonContent = JsonSerializer.Serialize(_profilePagesAccess);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{profileSiteVM.ProfileName} - Profile Updated Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllProfilesData();
                    await LoadAllPagesData();
                    await LoadAllProfilePagesData();
                    TempData["error"] = $"{profileSiteVM.ProfileName} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
        public async Task<IActionResult> OnPostDeleteAsync(int ProfileId)
        {
            if (ProfileId <= 0)
            {
                TempData["error"] = "User ID is required.";
                return RedirectToPage("/Error");
            }
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/DeleteEntity?deleteId={ProfileId}&deleteType={DeleteEntityType.Profile}";
                HttpResponseMessage response;
                response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"Profile - {ProfileId} deleted Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllProfilesData();
                    await LoadAllPagesData();
                    await LoadAllProfilePagesData();
                    TempData["error"] = $"Profile - {ProfileId} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
