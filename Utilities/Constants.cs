using Microsoft.AspNetCore.Mvc.Rendering;
using Spider_QAMS.Models;
using Spider_QAMS.Models.ViewModels;

namespace Spider_QAMS.Utilities
{
    public static class Constants
    {
        public const string BaseUserRoleName = "Base User Access"; // Needs Attention for Change if required for string value

        public const string CategoryType_UncategorizedPages = "Uncategorized Pages";

        public static string SuperUserName = "SuperUser";
        public static string SiteInspectorName = "SiteInspector";
        public static string SiteAuditorName = "SiteAuditor";

        public static string SP_RegisterNewUser = "dbo.uspRegisterNewUser";
        public static string SP_UpdateUserFlags = "dbo.uspUpdateUserFlags";
        public const string SP_AddUserPermission = "dbo.uspAddUserPermission";
        public const string SP_AddNewProfile = "dbo.uspAddNewProfile";
        public const string SP_DeleteUserPermission = "dbo.uspDeleteUserPermission";
        public const string SP_AddNewUser = "dbo.uspAddNewUser";
        public const string SP_UpdateUser = "dbo.uspUpdateUser";
        public const string SP_UpdateUserSettings = "dbo.uspUpdateUserSettings";

        public const string SP_CheckUniqueness = "dbo.uspCheckUniqueness";
        public const string SP_GetUserData = "dbo.uspGetUserData";
        public const string SP_GetTableAllData = "dbo.uspGetTableAllData";
        public const string SP_uspFetchRecordById = "dbo.uspFetchRecordById";

        public const string SP_DeleteEntityRecord = "dbo.uspDeleteEntityRecord";
        public const string SP_UpdateEntityRecord = "dbo.uspUpdateEntityRecord";
        public const string SP_CreateEntityRecord = "dbo.uspCreateEntityRecord";

        public static List<SelectListItem> GetTimeDropDown()
        {
            int minute = 60;
            List<SelectListItem > duration = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                duration.Add(new SelectListItem { Value = minute.ToString(), Text = i + " Hr" });
                minute = minute + 30;
                duration.Add(new SelectListItem { Value = minute.ToString(), Text = i + " Hr 30 min" });
                minute = minute + 30;
            }
            return duration;
        }

        public const string JwtCookieName = "_next-session-value";
        public const string JwtRefreshTokenName = "_next-session-token";

        public enum UserFlagsProperty
        {
            None = 0,
            IsADUser = 1,
            IsActive = 2,
            UserVerificationSetupEnabled = 3,
            RoleAssignmentEnabled = 4,
            EmailConfirmed = 5,
            PhoneNumberConfirmed = 6
        };

        public enum UserDataRetrievalCriteria
        {
            None = 0,
            GetUserByEmail = 1,     // Fetch user by email
            GetUserById = 2,        // Fetch user by ID
            GetUserRoles = 3,       // Fetch user roles by ID
        }

        public enum FetchRecordByIdEnum
        {
            None = 0,
            GetCurrentUserDetails = 1,      // Fetch user data
            GetCurrentUserProfile = 2,      // Fetch user's profile
            GetCurrentUserPages = 3,        // Fetch user pages list
            GetCurrentUserCategories = 4,   // Fetch user categories list
            GetSettingsData = 5,            // Fetch settings data
            GetProfileData = 6,             // Fetch profile data
            GetCategoryData = 7,            // Fetch category data
            GetRegionData = 8,              // Fetch region data
            GetCityData = 9,                // Fetch city data
            GetLocationData = 10,           // Fetch location data
            GetContactData = 11,           // Fetch location data
        }

        public static class FetchRecordTypeMapper
        {
            private static readonly Dictionary<FetchRecordByIdEnum, Type> TypeMappings = new()
            {
                { FetchRecordByIdEnum.GetCurrentUserDetails, typeof(ProfileUserAPIVM) },
                { FetchRecordByIdEnum.GetCurrentUserProfile, typeof(ProfileSite) },
                { FetchRecordByIdEnum.GetCurrentUserPages, typeof(List<PageSiteVM>) },
                { FetchRecordByIdEnum.GetCurrentUserCategories, typeof(List<CategoriesSetDTO>) },
                { FetchRecordByIdEnum.GetSettingsData, typeof(ProfileUserAPIVM) },
                { FetchRecordByIdEnum.GetProfileData, typeof(ProfileSite) },
                { FetchRecordByIdEnum.GetCategoryData, typeof(PageCategory) },
                { FetchRecordByIdEnum.GetRegionData, typeof(Region) },
                { FetchRecordByIdEnum.GetCityData, typeof(City) },
                { FetchRecordByIdEnum.GetLocationData, typeof(SiteLocation) },
                { FetchRecordByIdEnum.GetContactData, typeof(Contact) }
            };
            public static Type GetTypeByEnum(FetchRecordByIdEnum recordType)
            {
                if(TypeMappings.TryGetValue(recordType, out var type))
                {
                    return type;
                }
                throw new InvalidOperationException("Invalid Record Type.");
            }
        }

        public enum GetTableData
        {
            None = 0,
            GetAllUsersData = 1,            // Fetch all users
            GetAllProfiles = 2,             // Fetch al profiles
            GetAllPages = 3,                // Fetch all pages
            GetAllCategories = 4,           // Fetch all categories
            GetAllRegions = 5,              // Fetch all regions
            GetAllCities = 6,               // Fetch all cities
            GetAllLocations = 7,            // Fetch all cities
            GetRegionListOfCities = 8,      // Fetch all cities
            GetAllSponsors = 9,             // Fetch all sponsors
            GetAllContacts = 10,            // Fetch all contacts
            GetAllSiteTypes = 11,           // Fetch all site types
            GetAllBranchTypes = 12,         // Fetch all branch types
            GetAllVisitStatuses = 13,       // Fetch all visit statuses
            GetAllATMClasses = 14,          // Fetch all atm classes
        }

        public enum TableNameCheckUniqueness
        {
            User = 1,
            Profile = 2,
            PageCategory = 3,
            Region = 4,
            City = 5,
            Location = 6,
            Sponsor = 7,
            Contact = 8
        };

        public static class DeleteEntityType
        {
            public static string User = "User";
            public static string Profile = "Profile";
            public static string Category = "Category";
            public static string Region = "Region";
            public static string City = "City";
            public static string Location = "Location";
            public static string Sponsor = "Sponsor";
            public static string Contact = "Contact";
        }

        public static class TableNameClassForUniqueness
        {
            public static string[] User = { "emailid", "phonenumber", "username", "profilepicname" };
            public static string[] Profile = { "profilename" };
            public static string[] PageCategory = { "categoryname" };
            public static string[] Region = { "regionname" };
            public static string[] City = { "cityname" };
        };

        public static class UserPermissionStates
        {
            public static int PageIdOnly = 1;
            public static int PageCategoryIdOnly = 2;
            public static int BothPageIdAndPageCategoryId = 3;
        };

        public static class JWTCookieHelper
        {
            public static string GetJWTCookie(HttpContext httpContext)
            {
                return httpContext.Request.Cookies[JwtCookieName];
            }
        }
        public static class SessionKeys
        {
            public static string CurrentUserKey = "CurrentUser";
            public static string CurrentUserProfileKey = "CurrentUserProfile";
            public static string CurrentUserPagesKey = "CurrentUserPages";
            public static string CurrentUserCategoriesKey = "CurrentUserCategories";
            public static string CurrentUserClaimsKey = "CurrentUserClaims";
        }

        public static class BaseUserScreenAccess // Needs Attention for Change if required for string value
        {
            public const string AccessDenied = "/Account/AccessDenied";
            public const string AuthenticatorWithMFASetup = "/Account/AuthenticatorWithMFASetup";
            public const string ConfirmEmail = "/Account/ConfirmEmail";
            public const string Login = "/Account/Login";
            public const string LoginTwoFactorWithAuthenticator = "/Account/LoginTwoFactorWithAuthenticator";
            public const string Logout = "/Account/Logout";
            public const string Register = "/Account/Register";
            public const string Dashboard = "/Dashboard";
            public const string ViewUserProfile = "/ViewUserProfile";
            public const string EditSettings = "/EditSettings";
            public const string Error = "/Error";
        };
    }
}
