using Microsoft.AspNetCore.Mvc.Rendering;

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
        public static string SP_UpdateUserVerificationInitialSetup = "dbo.uspUpdateUserVerificationInitialSetup";
        public const string SP_AddUserPermission = "dbo.uspAddUserPermission";
        public const string SP_AddNewProfile = "dbo.uspAddNewProfile";
        public const string SP_DeleteUserPermission = "dbo.uspDeleteUserPermission";
        public const string SP_AddNewUser = "dbo.uspAddNewUser";
        public const string SP_UpdateUser = "dbo.uspUpdateUser";

        public const string SP_CheckUniqueness = "dbo.uspCheckUniqueness";

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

        public enum TableNameCheckUniqueness
        {
            User = 1,
            Profile = 2,
            PageCategory = 3
        };

        public static class TableNameClassForUniqueness
        {
            public static string[] User = { "emailid", "phonenumber", "username", "profilepicname" };
            public static string[] Profile = { "profilename" };
            public static string[] PageCategory = { "categoryname" };
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
            public const string ReadUserProfile = "/ReadUserProfile";
            public const string EditSettings = "/EditSettings";
            public const string Error = "/Error";
        };
    }
}
