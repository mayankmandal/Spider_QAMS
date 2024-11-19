using Microsoft.AspNetCore.Authorization;
using Spider_QAMS.Models.ViewModels;
using Spider_QAMS.Repositories.Domain;
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
            try
            {
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

                var user = await _applicationUserBusinessLogic.GetCurrentUserAsync();

                if (user.IsActive == null || user.IsActive == false || user.IsDeleted == true)
                {
                    context.Fail();
                    HandleAccessDenied(context);
                    return;
                }

                // Fetch user pages from session or API
                var pages = await FetchUserPagesAsync();

                if (pages != null && pages.Any(page => page.PageUrl.Equals(currentPage, StringComparison.OrdinalIgnoreCase)))
                {
                    context.Succeed(requirement);
                }
                else
                {
                    HandleAccessDenied(context, currentPage);
                }
            }
            catch (Exception ex)
            {
                HandleAccessDenied(context, message: "Error occurred while processing your request.");
            }
        }
        private async Task<List<PageSiteVM>> FetchUserPagesAsync()
        {
            var pagesJson = _httpContextAccessor.HttpContext.Session.GetString(SessionKeys.CurrentUserPagesKey);
            List<PageSiteVM> pages = !string.IsNullOrEmpty(pagesJson)
                ? JsonSerializer.Deserialize<List<PageSiteVM>>(pagesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                : null;

            if (pages == null)
            {
                var accessToken = _applicationUserBusinessLogic.GetJWTCookie(JwtCookieName);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    await _applicationUserBusinessLogic.FetchAndCacheUserPermissions(accessToken);
                    pagesJson = _httpContextAccessor.HttpContext.Session.GetString(SessionKeys.CurrentUserPagesKey);
                    pages = !string.IsNullOrEmpty(pagesJson)
                        ? JsonSerializer.Deserialize<List<PageSiteVM>>(pagesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        : null;
                }
            }
            return pages;
        }
        private void HandleAccessDenied(AuthorizationHandlerContext context, string currentPage = null, string message = null)
        { 
            context.Fail();

            // Decide the redirect page based on authentication status
            var isAuthenticated = _applicationUserBusinessLogic.UserContext.User.Identity.IsAuthenticated;
            var accessDeniedUrl = isAuthenticated ? "/Account/AccessDenied" : "/Account/Login";

            if (!string.IsNullOrEmpty(currentPage) && isAuthenticated)
            {
                accessDeniedUrl += $"?returnUrl={currentPage}";
            }

            // Set custom message if provided
            if (!string.IsNullOrEmpty(message))
            {
                _httpContextAccessor.HttpContext.Response.Headers.Add("Error-Message", message);
            }

            _httpContextAccessor.HttpContext.Response.Redirect(accessDeniedUrl);
        }
    }
}
