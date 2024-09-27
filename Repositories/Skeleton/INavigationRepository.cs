using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;

namespace Spider_QAMS.Repositories.Skeleton
{
    public interface INavigationRepository
    {
        Task<object> FetchRecordByTypeAsync(Record record);
        Task<bool> CheckUniquenessAsync(string field, string value);
        Task<bool> DeleteEntityAsync(int deleteId, string deleteType);

        Task<bool> CreateUserProfileAsync(ProfileUser profileUsersData, int CurrentUserId);
        Task<string> UpdateUserProfileAsync(ProfileUser profileUsersData, int CurrentUserId);

        Task<string> UpdateSettingsDataAsync(ProfileUser userSettings, int CurrentUserId);

        Task<bool> UpdateRegionAsync(Region region);
        Task<bool> CreateRegionAsync(Region region);

        Task<bool> UpdateCityAsync(City city);
        Task<bool> CreateCityAsync(City city);

        Task<bool> UpdateLocationAsync(SiteLocation siteLocation);
        Task<bool> CreateLocationAsync(SiteLocation siteLocation);

        Task<bool> UpdateContactAsync(Contact contact);
        Task<bool> CreateContactAsync(Contact contact);

        Task<ProfileUserAPIVM> GetUserRecordAsync(int newUserId);
        Task<ProfileSite> GetCurrentUserProfileAsync(int CurrentUserId);
        Task<List<PageSiteVM>> GetCurrentUserPagesAsync(int CurrentUserId);
        Task<List<CategoriesSetDTO>> GetCurrentUserCategoriesAsync(int CurrentUserId);
        Task<ProfileUserAPIVM> GetSettingsDataAsync(int CurrentUserId);
        Task<ProfileSite> GetProfileDataAsync(int newUserId);
        Task<PageCategory> GetCategoryDataAsync(int newUserId);
        Task<Region> GetRegionDataAsync(int newUserId);
        Task<City> GetCityDataAsync(int newUserId);
        Task<SiteLocation> GetLocationDataAsync(int newUserId);
        Task<Contact> GetContactDataAsync(int newUserId);
        

        Task<List<ProfileUserAPIVM>> GetAllUsersDataAsync();
        Task<List<ProfileSite>> GetAllProfilesAsync();
        Task<List<PageSite>> GetAllPagesAsync();
        Task<List<PageCategory>> GetAllCategoriesAsync();
        Task<List<Region>> GetAllRegionsAsync();
        Task<List<City>> GetAllCitiesAsync();
        Task<List<SiteLocation>> GetAllLocationsAsync();
        Task<List<CityRegionViewModel>> GetRegionListOfCitiesAsync();
        Task<List<Sponsor>> GetAllSponsorsAsync();
        Task<List<ContactVM>> GetAllContactsAsync();
    }
}
