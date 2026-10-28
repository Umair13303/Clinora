using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using PulsePoint.Areas.Dashboard.Helpers;
using PulsePoint.Areas.Security.Helpers;
using PulsePoint.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers & Views
builder.Services.AddControllersWithViews();

// 2. EF Core Database Context (Points to HMS_DB on SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Register Helpers & Services
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IDashboardHelper, DashboardHelper>();

// 4. JWT & Cookie Authentication
var secretKey = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "PulsePointCookieAuth";
    options.DefaultChallengeScheme = "PulsePointCookieAuth";
})
.AddCookie("PulsePointCookieAuth", options =>
{
    options.Cookie.Name = "PulsePoint_AuthSession";
    options.LoginPath = "/Security/Auth/Login";
    options.AccessDeniedPath = "/Security/Auth/Login";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. CORS for Angular 21 Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve standard wwwroot static files
app.UseStaticFiles();

// Serve Area-specific static files (e.g., Areas/Dashboard/Js)
var dashboardJsDir = Path.Combine(app.Environment.ContentRootPath, "Areas", "Dashboard", "Js");
if (Directory.Exists(dashboardJsDir))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(dashboardJsDir),
        RequestPath = "/areas/dashboard/js"
    });
}

app.UseRouting();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();



// Map Area routes (e.g. /{area}/{controller}/{action})
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route (points to Dashboard area, which redirects to Login if unauthenticated)
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Dashboard}/{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
