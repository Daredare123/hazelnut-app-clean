using Microsoft.AspNetCore.Authentication.Cookies;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using HazelnutVeb.Data;
using Microsoft.Extensions.Logging;
using HazelnutVeb.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using HazelnutVeb.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http; // Ensure this is present for Cookie options

var builder = WebApplication.CreateBuilder(args);

// Ensure configuration includes environment variables (default behavior is usually enough, but explicitly preserving it)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddRazorPages();

// Add controllers with views
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<PushNotificationService>();

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });

builder.Services.AddAuthorization();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear known proxies so it accepts headers from Render's load balancer
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

var supportedCultures = new[] { "mk", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Enable Production Exception handling to seamlessly route HTTP 500 errors to /Home/Error
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // db.Database.Migrate(); // Temporarily disabled
    
    // Seed Inventory if empty ensuring at least one record (Preserved business logic)
    if (!db.Inventory.Any())
    {
        db.Inventory.Add(new Inventory { TotalKg = 0 });
        db.SaveChanges();
    }
}

app.UseForwardedHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


try
{
    var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
    if (!string.IsNullOrEmpty(firebaseJson) && FirebaseApp.DefaultInstance == null)
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromJson(firebaseJson)
        });
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Firebase initialization failed: {ex.Message}");
}
app.Run();