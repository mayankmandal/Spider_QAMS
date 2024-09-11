using Spider_QAMS.DAL;
using Spider_QAMS.Controllers;
using Spider_QAMS.Repositories.Domain;
using Spider_QAMS.Repositories.Skeleton;
using Spider_QAMS.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure the connection string for SQL Database Helper
SqlDBHelper.CONNECTION_STRING = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
Configure(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddSingleton<SqlDBHelper>();

    // Register the HTTP context accessor
    services.AddHttpContextAccessor();

    // Register the email service
    services.AddScoped<IEmailService, EmailService>();

    // Register the UserRepository
    services.AddScoped<IUserRepository, UserRepository>();

    // Register the ApplicationUserBusinessLogic
    services.AddScoped<ApplicationUserBusinessLogic>();

    // Configure SMTP settings
    services.Configure<SmtpSetting>(configuration.GetSection("SMTP"));

    // Add services to the container.
    services.AddRazorPages().AddRazorPagesOptions(options =>
    {
        options.Conventions.AddPageRoute("/Account/Login","");
    });

    services.AddControllers();

    services.AddHttpClient("WebAPI", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]);
    });

    services.AddScoped<IAppointmentService, AppointmentService>();

    services.AddSession();
}

void Configure(WebApplication app)
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

    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();

    app.Run();
}