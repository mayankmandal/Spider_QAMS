using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Spider_QAMS.Models;
using static Spider_QAMS.Utilities.Constants;
using System.Net.Http.Headers;
using System.Text.Json;
using Spider_QAMS.Models.ViewModels;
using System.Text;

namespace Spider_QAMS.Pages
{
    public class ManageContactsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;
        private Contact _contact;
        public ManageContactsModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _clientFactory = httpClientFactory;
        }
        public IList<ContactVM> contactVMList { get; set; }
        public IList<Sponsor> Sponsors { get; set; }
        [BindProperty]
        public ContactVM ContactViewModel { get; set; }
        private async Task LoadAllContactsData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllContacts");
            contactVMList = JsonSerializer.Deserialize<List<ContactVM>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        private async Task LoadAllSponsorsData()
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
            var response = await client.GetStringAsync($"{_configuration["ApiBaseUrl"]}/Navigation/GetAllSponsors");
            Sponsors = JsonSerializer.Deserialize<List<Sponsor>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadAllContactsData();
            await LoadAllSponsorsData();
            return Page();
        }
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["error"] = "Model State Validation Failed: " + string.Join("; ", errorMessages);
                await LoadAllContactsData();
                await LoadAllSponsorsData();
                return Page();
            }
            try
            {
                _contact = new Contact
                {
                    ContactId = 0,
                    Sponsor = new Sponsor
                    {
                        SponsorId = ContactViewModel.SponsorId,
                        SponsorName = ""
                    },
                    Designation = ContactViewModel.Designation,
                    BranchName = ContactViewModel.BranchName,
                    EmailID = ContactViewModel.EmailID,
                    Name = ContactViewModel.Name,
                    Fax = ContactViewModel.Fax,
                    Mobile = ContactViewModel.Mobile,
                    OfficePhone = ContactViewModel.OfficePhone,
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/CreateContact";
                var jsonContent = JsonSerializer.Serialize(_contact);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{ContactViewModel.Name} - Created Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllContactsData();
                    await LoadAllSponsorsData();
                    TempData["error"] = $"{ContactViewModel.Name} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
            if (ContactViewModel.ContactId != null)
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/FetchRecord";
                var requestBody = new Record { RecordId = ContactViewModel.ContactId, RecordType = (int)FetchRecordByIdEnum.GetContactData };
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _contact = JsonSerializer.Deserialize<Contact>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    await LoadAllContactsData();
                    await LoadAllSponsorsData();
                    // ModelState.AddModelError("ProfileUsersData.UserId", $"Error fetching user record for {ContactViewModel.Name}. Please ensure the UserId is correct.");
                    TempData["error"] = $"Model State Validation Failed. Response status: {response.StatusCode} - {response.ReasonPhrase}";
                    return Page();
                }
            }

            if (!ModelState.IsValid)
            {
                await LoadAllSponsorsData();
                TempData["error"] = "Model State Validation Failed.";
                return Page();
            }
            try
            {
                _contact = new Contact
                {
                    ContactId = ContactViewModel.ContactId,
                    Sponsor = new Sponsor
                    {
                        SponsorId = ContactViewModel.SponsorId,
                        SponsorName = ""
                    },
                    Designation = ContactViewModel.Designation,
                    BranchName = ContactViewModel.BranchName,
                    EmailID = ContactViewModel.EmailID,
                    Name = ContactViewModel.Name,
                    Fax = ContactViewModel.Fax,
                    Mobile = ContactViewModel.Mobile,
                    OfficePhone = ContactViewModel.OfficePhone,
                };
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/UpdateContact";
                var jsonContent = JsonSerializer.Serialize(_contact);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(apiUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"{ContactViewModel.Name} - Contact Updated Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllContactsData();
                    await LoadAllSponsorsData();
                    TempData["error"] = $"{ContactViewModel.Name} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
        public async Task<IActionResult> OnPostDeleteAsync(int ContactId)
        {
            if (ContactId <= 0)
            {
                TempData["error"] = "Contact ID is required.";
                await LoadAllContactsData();
                await LoadAllSponsorsData();
                return RedirectToPage();
            }
            try
            {
                var client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JWTCookieHelper.GetJWTCookie(HttpContext));
                var apiUrl = $"{_configuration["ApiBaseUrl"]}/Navigation/DeleteEntity?deleteId={ContactId}&deleteType={DeleteEntityType.Contact}";
                HttpResponseMessage response;
                response = await client.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    TempData["success"] = $"Contact - {ContactId} deleted Successfully";
                    return RedirectToPage();
                }
                else
                {
                    await LoadAllContactsData();
                    await LoadAllSponsorsData();
                    TempData["error"] = $"Contact - {ContactId} - Error occurred in response with status: {response.StatusCode} - {response.ReasonPhrase}";
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
