using Spider_QAMS.DAL;
using Spider_QAMS.Controllers;
using Spider_QAMS.Repositories.Domain;
using Spider_QAMS.Repositories.Skeleton;
using Spider_QAMS.Models;
using Spider_QAMS.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Configure the connection string for SQL Database Helper
SqlDBHelper.CONNECTION_STRING = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
Configure(app, builder.Configuration);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{

    // Configure application cookies
    services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

    // Add cookie-based authentication
    services.AddAuthentication(
        options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JwtSettings_SecretKey"])),
            };
        });

    // Configure SMTP settings
    services.Configure<SmtpSetting>(configuration.GetSection("SMTP"));

    // Add authorization policies
    services.AddAuthorization(options =>
    {
        options.AddPolicy("PageAccess", policy => policy.Requirements.Add(new PageAccessRequirement()));
    });

    // Add services to the container.
    services.AddRazorPages().AddRazorPagesOptions(options =>
    {
        options.Conventions.AddPageRoute("/Account/Login", "");
    });

    services.AddControllers();

    services.AddHttpClient("WebAPI", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]);
    });

    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    // Register the PageAccessHandler
    services.AddScoped<IAuthorizationHandler, PageAccessHandlerMiddleware>();

    // Register the HTTP context accessor
    services.AddHttpContextAccessor();

    services.AddSingleton<SqlDBHelper>();

    services.AddScoped<IUniquenessCheckService, UniquenessCheckService>();

    // Register the UserRepository
    services.AddScoped<IUserRepository, UserRepository>();

    // Register the NavigationRepository
    services.AddScoped<INavigationRepository, NavigationRepository>();

    // Register the ApplicationUserBusinessLogic
    services.AddScoped<ApplicationUserBusinessLogic>();

    // Apply CORS specifically for image requests
    /*services.AddCors(options =>
    {
        options.AddPolicy("ImagesOnlyPolicy", builder =>
        {
            builder.WithOrigins(configuration["BaseUrl"]) // Allow specific origin
                   .WithMethods("GET") // Allow only GET requests
                   .AllowCredentials(); // Allow cookies and authorization
        });
    });*/
}

void Configure(WebApplication app, IConfiguration configuration)
{
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseSession();

    app.UseRouting();

    // Use authentication and authorization
    app.UseAuthentication();

    // app.UseCors("ImagesOnlyPolicy");

    app.UseAuthorization();

    /*app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapRazorPages();
        endpoints.MapFallbackToFile("");
        endpoints.MapGet(configuration["SiteDetailImgPath"] + "/{*filepath}", async context =>
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", configuration["BaseUrl"]);
            await context.Response.SendFileAsync(context.Request.Path);
        });
    });*/

    app.MapRazorPages();
    app.MapControllers();
}