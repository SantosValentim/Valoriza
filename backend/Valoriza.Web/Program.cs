/* Painel MVC | Identity (cookie) | SQL Server
   Segurança: HTTPS, HSTS, CSRF, cookies seguros, hash de senha (Identity) */

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Valoriza.API.Data;
using Valoriza.API.Models;
using Valoriza.API.Services;

var builder = WebApplication.CreateBuilder(args);

// BANCO (SQL Server)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY (hash de senha PBKDF2)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    // Produção com HTTPS: CookieSecurePolicy.Always
    // Dev local: SameAsRequest
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// CSRF (antiforgery)
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.HeaderName = "X-CSRF-TOKEN";
});

// MVC + token CSRF obrigatório em POST/PUT/DELETE
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// SERVIÇOS DE DOMÍNIO
builder.Services.AddScoped<ProgressoService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

var app = builder.Build();

// PIPELINE 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // navegador passa a exigir HTTPS
}

app.UseHttpsRedirection(); // HTTP -> HTTPS
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();