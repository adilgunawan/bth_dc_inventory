using bth_dc_inventory.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// =========================
// DATABASE CONNECTION
// =========================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// =========================
// CONFIGURE JWT AUTHENTICATION - 
// =========================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

// ✅ Debug: Log JWT configuration
Console.WriteLine($"JWT Configuration:");
Console.WriteLine($"  Key: {(!string.IsNullOrEmpty(jwtKey) ? "✅ Configured" : "❌ Missing")}");
Console.WriteLine($"  Issuer: {jwtIssuer ?? "❌ Missing"}");
Console.WriteLine($"  Audience: {jwtAudience ?? "❌ Missing"}");

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is incomplete. Please check appsettings.json");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer, 
        ValidAudience = jwtAudience, 
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero 
    };

    // ✅ Event handlers untuk debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ JWT Authentication Failed: {context.Exception.Message}");
            Console.WriteLine($"   Token: {context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "")?.Substring(0, 20)}...");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var userName = context.Principal?.FindFirst(ClaimTypes.Name)?.Value;
            var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"✅ JWT Token Validated - User: {userName} (ID: {userId})");
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring(7);
                Console.WriteLine($"🔑 JWT Token Received: {token.Substring(0, Math.Min(30, token.Length))}...");
            }
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"🚫 JWT Challenge: {context.Error} - {context.ErrorDescription}");
            Console.WriteLine($"   Path: {context.Request.Path}");
            Console.WriteLine($"   Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
            return Task.CompletedTask;
        }
    };
});

//  Authorization
builder.Services.AddAuthorization();

// =========================
// SERVICES
// =========================
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 52428800; // 50MB
});

//  EPPlus License
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

//  QuestPDF License
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

// =========================
// BUILD THE APPLICATION
// =========================
var app = builder.Build();

// =========================
// MIDDLEWARE CONFIGURATION
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//  PENTING: Urutan middleware harus benar
app.UseAuthentication(); // Harus sebelum UseAuthorization
app.UseAuthorization();

// =========================
// ROUTING
// =========================
app.MapControllers(); //  Untuk API endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("🚀 Application started successfully!");
Console.WriteLine($"📍 Environment: {app.Environment.EnvironmentName}");

app.Run();