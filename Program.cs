using FFMpegCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Cantare.Database;
using Cantare.Database.Services;

var builder = WebApplication.CreateBuilder(args);

GlobalFFOptions.Configure(new FFOptions { 
    BinaryFolder = builder.Configuration.GetValue<string>("FFMPEGBinaryLocation"), 
    TemporaryFilesFolder = builder.Configuration.GetValue<string>("FFMPEGTempLocation")
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<SongSelectionService>();

builder.Services.AddTransient<Cantare.Helpers.HTMLGenerator>(); 

var connectionString = builder.Configuration.GetConnectionString("BackendDatabase");
builder.Services.AddDbContext<BackendDatabase>(dbContextOptions =>
    dbContextOptions.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
}).AddCookie()
    .AddGoogle(options =>
{
    options.ClientId = builder.Configuration.GetValue<string>("client_id");
    options.ClientSecret = builder.Configuration.GetValue<string>("client_secret");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
