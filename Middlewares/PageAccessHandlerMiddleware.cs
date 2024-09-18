using Microsoft.AspNetCore.Authorization;
using Spider_QAMS.Controllers;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Utilities;
using System.Text.Json;
using static Spider_QAMS.Utilities.Constants;

namespace Spider_QAMS.Middlewares
{
    public class PageAccessRequirement : IAuthorizationRequirement { }
    public class PageAccessHandlerMiddleware : AuthorizationHandler<PageAccessRequirement>
    {
        private readonly ApplicationUserBusinessLogic _applicationUserBusinessLogic;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PageAccessHandlerMiddleware(ApplicationUserBusinessLogic applicationUserBusinessLogic, IHttpContextAccessor httpContextAccessor)
        {
            _applicationUserBusinessLogic = applicationUserBusinessLogic;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PageAccessRequirement requirement)
        {
            var user = await _applicationUserBusinessLogic.GetCurrentUserAsync();

            var currentPage = _httpContextAccessor.HttpContext.Request.Path.Value;

            var jwtToken = _applicationUserBusinessLogic.GetJWTCookie(JwtCookieName);
            var principal = _applicationUserBusinessLogic.GetPrincipalFromToken(jwtToken);

            // Check if user is not authenticated
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                context.Fail();
                HandleAccessDenied(context);
                return;
            }

            user = await _applicationUserBusinessLogic.GetCurrentUserAsync();
            
            if (user.IsActive == null || user.IsActive == false)
            {
                context.Fail();
                HandleAccessDenied(context);
                return;
            }

            // Try to get pages from session
            var pagesJson = _httpContextAccessor.HttpContext.Session.GetString(SessionKeys.CurrentUserPagesKey);
            List<PageSiteVM> pages = !string.IsNullOrEmpty(pagesJson) ? JsonSerializer.Deserialize<List<PageSiteVM>>(pagesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) : null;

            if (pages == null)
            {
                // If pages are not in session, fetch and store them in session
                var accessToken = _applicationUserBusinessLogic.GetJWTCookie(Constants.JwtCookieName);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await _applicationUserBusinessLogic.FetchAndCacheUserPermissions(accessToken);
                    pagesJson = _httpContextAccessor.HttpContext.Session.GetString(SessionKeys.CurrentUserPagesKey);
                    if (!string.IsNullOrEmpty(pagesJson))
                    {
                        pages = JsonSerializer.Deserialize<List<PageSiteVM>>(pagesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
            }

            if (pages != null && pages.Any(page => page.PageUrl.Equals(currentPage, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
            else
            {
                HandleAccessDenied(context, currentPage);
            }
        }

        private void HandleAccessDenied(AuthorizationHandlerContext context, string currentPage = null)
        {
            context.Fail();
            var isAuthenticated = _applicationUserBusinessLogic.UserContext.User.Identity.IsAuthenticated;
            var accessDeniedUrl = isAuthenticated ? "/Account/AccessDenied" : "/Account/Login";


            if (!string.IsNullOrEmpty(currentPage) && isAuthenticated)
            {
                accessDeniedUrl += $"?returnUrl={currentPage}";
            }
            _applicationUserBusinessLogic.UserContext.Response.Redirect(accessDeniedUrl);
        }
    }
}
