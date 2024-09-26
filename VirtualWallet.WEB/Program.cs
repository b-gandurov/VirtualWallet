using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using VirtualWallet.WEB.Middlewares;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using VirtualWallet.BUSINESS.Services.Contracts;
using VirtualWallet.DATA.Repositories.Contracts;
using VirtualWallet.DATA.Repositories;
using VirtualWallet.DATA.Services.Contracts;
using VirtualWallet.DATA.Services;
using VirtualWallet.BUSINESS.Services;
using VirtualWallet.DATA.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Twilio.Jwt.Taskrouter;
using Polly;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(80);
//});

builder.Services.AddControllersWithViews();

// EF Core - Database Context
// Register EF Core with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // Use PostgreSQL as the database provider
    options.UseNpgsql(connectionString);

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

});

// Define the retry policy
var retryPolicy = Polly.Policy
    .Handle<NpgsqlException>()
    .WaitAndRetry(
        retryCount: 5,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} encountered an error: {exception.Message}. Waiting {timeSpan} before next retry.");
        });

// Google OAuth Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:client_id"];
    options.ClientSecret = builder.Configuration["Authentication:client_secret"];
    options.CallbackPath = new PathString("/signin-google");
});

// Action Filters
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<HandleServiceResultAttribute>();
});

// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Dependency Injection: Repositories and Services
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
builder.Services.AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>();

// Registering Repositories
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICardTransactionRepository, CardTransactionRepository>();
builder.Services.AddScoped<IRealCardRepository, RealCardRepository>();
builder.Services.AddScoped<IUserWalletRepository, UserWalletRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

// Registering Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ICardTransactionService, CardTransactionService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IPaymentProcessorService, PaymentProcessorService>();
builder.Services.AddScoped<ITransactionHandlingService, TransactionHandlingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IWalletTransactionService, WalletTransactionService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// Helpers
builder.Services.AddScoped<IViewModelMapper, ViewModelMapper>();
builder.Services.AddScoped<IDtoMapper, DtoMapper>();

// Attributes
builder.Services.AddScoped<RequireAuthorizationAttribute>();

// Registering HttpClient
builder.Services.AddHttpClient<CurrencyService>();

// Swagger Configuration
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ForumProject API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer abcdef12345\""
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});
var app = builder.Build();

retryPolicy.Execute(() =>
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            context.Database.Migrate();
            InitializeData.Initialize(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred applying migrations: {ex}");
            throw;
        }
    }
});




app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// JWT authentication
app.UseAuthentication();
app.UseAuthorization();

// Custom middlewares
app.UseMiddleware<CurrentUserMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCoreDemo API V1");
    options.RoutePrefix = "api/swagger";
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapControllerRoute(
        name: "error",
        pattern: "Error",
        defaults: new { controller = "Error", action = "Index" });
});

app.Run();
