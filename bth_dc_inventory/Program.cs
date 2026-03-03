using bth_dc_inventory.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using System.Text;


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
// CONFIGURE JWT AUTHENTICATION
// =========================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "YourIssuer",      // Ubah dengan issuer valid
            ValidAudience = "YourAudience", // Ubah dengan audience valid
            IssuerSigningKey = new SymmetricSecurityKey(key) // Secret key untuk token
        };
    });

// =========================
// SERVICES
// =========================
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =========================
// BUILD THE APPLICATION
// =========================
var app = builder.Build();

app.MapControllers(); // Memetakan controller otomatis
// =========================
// MIDDLEWARE
// =========================
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Middleware untuk autentikasi
app.UseAuthorization();  // Middleware untuk otorisasi

// =========================
// ROUTING
// =========================
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

//using bth_dc_inventory.Data;
//using Microsoft.EntityFrameworkCore;
//using QuestPDF.Infrastructure;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;


//var builder = WebApplication.CreateBuilder(args);


//// =========================
//// DATABASE CONNECTION
//// =========================
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection")
//    )
//);

//// =========================
//// SERVICES
//// =========================
//builder.Services.AddControllersWithViews();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//var app = builder.Build();

//// =========================
//// MIDDLEWARE
//// =========================
//app.UseSwagger();
//app.UseSwaggerUI();

//var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("BTH_DC_INVENTORY")); // Ganti dengan key yang kuat
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = "YourIssuer",
//            ValidAudience = "YourAudience",
//            IssuerSigningKey = key
//        };
//    });

//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//// =========================
//// ROUTING
//// =========================
//app.MapControllers();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();