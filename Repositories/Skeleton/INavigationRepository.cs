﻿using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;

namespace Spider_QAMS.Repositories.Skeleton
{
    public interface INavigationRepository
    {
        Task<List<ProfileUserAPIVM>> GetAllUsersDataAsync();
        Task<List<ProfileSite>> GetAllProfilesAsync();
        Task<List<PageSite>> GetAllPagesAsync();
        Task<List<PageCategory>> GetAllCategoriesAsync();

        Task<ProfileUserAPIVM> GetCurrentUserDetailsAsync(int CurrentUserId);
        Task<ProfileSite> GetCurrentUserProfileAsync(int CurrentUserId);
        Task<List<PageSiteVM>> GetCurrentUserPagesAsync(int CurrentUserId);
        Task<List<CategoriesSetDTO>> GetCurrentUserCategoriesAsync(int CurrentUserId);
        
        Task<ProfileUserAPIVM> GetUserRecordAsync(string userId);
        Task<bool> CreateUserProfileAsync(ProfileUser profileUsersData, int CurrentUserId);
        Task<string> UpdateUserProfileAsync(ProfileUser profileUsersData, int CurrentUserId);
        Task<bool> CheckUniquenessAsync(string field, string value);
        Task<bool> DeleteEntityAsync(int deleteId, string deleteType);
        Task<ProfileUserAPIVM> GetSettingsDataAsync(int CurrentUserId);
        Task<string> UpdateSettingsDataAsync(ProfileUser userSettings, int CurrentUserId);

        Task<List<Region>> GetAllRegionsAsync();
        Task<bool> UpdateRegionAsync(Region region);
        Task<bool> CreateRegionAsync(Region region);

        Task<List<City>> GetAllCitiesAsync();
        Task<bool> UpdateCityAsync(City city);
        Task<bool> CreateCityAsync(City city);

        Task<List<SiteLocation>> GetAllLocationsAsync();
        // Task<bool> UpdateLocationAsync(City city);
        // Task<bool> CreateLocationAsync(City city);

        Task <List<CityRegionViewModel>> GetRegionListOfCitiesAsync();
    }
}
