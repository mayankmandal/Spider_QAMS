using Microsoft.AspNetCore.Mvc.Rendering;

namespace Spider_QAMS.Utilities
{
    public static class Constants
    {
        public static string SuperUserName = "SuperUser";
        public static string SiteInspectorName = "SiteInspector";
        public static string SiteAuditorName = "SiteAuditor";

        public static string SP_RegisterNewUser = "dbo.uspRegisterNewUser";

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
        public const string JwtAMRTokenName = "_next-amr-value";

        public static class JWTCookieHelper
        {
            public static string GetJWTCookie(HttpContext httpContext)
            {
                return httpContext.Request.Cookies[Constants.JwtCookieName];
            }
            public static string GetJWTAMRToken(HttpContext httpContext)
            {
                return httpContext.Request.Cookies[Constants.JwtAMRTokenName];
            }
        }

    }
}
