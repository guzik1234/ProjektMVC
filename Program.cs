using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ogloszenia.Data;
using ogloszenia.Services;
using ogloszenia.Models;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register AuthService
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IModerationService, ModerationService>();
builder.Services.AddScoped<IRssService, RssService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

var app = builder.Build();

// Seed database with initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    
    // Seed users if none exist
    if (!context.Users.Any())
    {
        var testUser = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
            IsAdmin = false,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        
        var adminUser = new User
        {
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            IsAdmin = true,
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        
        context.Users.Add(testUser);
        context.Users.Add(adminUser);
        context.SaveChanges();
    }
    
    // Seed categories if none exist
    if (!context.Categories.Any())
    {
        var categories = new[]
        {
            new Category { Name = "Motoryzacja" },
            new Category { Name = "Elektronika" },
            new Category { Name = "Dom i Ogród" },
            new Category { Name = "Moda" },
            new Category { Name = "Praca" }
        };
        context.Categories.AddRange(categories);
        context.SaveChanges();
    }
    
    // Initialize SystemSettings if it doesn't exist or fix NULL values
    var systemSettings = context.SystemSettings.FirstOrDefault();
    if (systemSettings == null)
    {
        systemSettings = new SystemSettings
        {
            ForbiddenWords = new List<string>(),
            AllowedHtmlTags = new List<string> { "p", "br", "strong", "em", "u", "a", "ul", "ol", "li", "h1", "h2", "h3" },
            MaxFileSize = 10 * 1024 * 1024,
            MaxFilesPerAdvertisement = 5,
            MaxMediaPerAdvertisement = 10,
            AdminMessage = string.Empty,
            LastUpdated = DateTime.Now
        };
        context.SystemSettings.Add(systemSettings);
        context.SaveChanges();
    }
    else
    {
        // Fix NULL values in existing record
        bool needsUpdate = false;
        if (systemSettings.ForbiddenWords == null)
        {
            systemSettings.ForbiddenWords = new List<string>();
            needsUpdate = true;
        }
        if (systemSettings.AllowedHtmlTags == null || !systemSettings.AllowedHtmlTags.Any())
        {
            systemSettings.AllowedHtmlTags = new List<string> { "p", "br", "strong", "em", "u", "a", "ul", "ol", "li", "h1", "h2", "h3" };
            needsUpdate = true;
        }
        if (needsUpdate)
        {
            context.SaveChanges();
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession(); // Add session middleware

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
