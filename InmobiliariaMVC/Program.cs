using InmobiliariaMVC.Handlers;
using InmobiliariaMVC.Services.Implementations;
using InmobiliariaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session y acceso al HttpContext para obtener TOKEN
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// Registrar el TokenHandler que inyecta el Authorization header desde el HttpContext/Session
builder.Services.AddTransient<TokenHandler>();

//Inyeccion de dependencias - Repositorios y Servicios
builder.Services.AddScoped<IAuthService, AuthService>();



// Leer configuración del cliente API (compatibilizar "ApiClient" o "ApiSettings")
var apiSection = builder.Configuration.GetSection("ApiClient");
if (!apiSection.Exists())
    apiSection = builder.Configuration.GetSection("ApiSettings");


var baseAddressString = apiSection.GetValue<string>("BaseAddress") ?? "http://localhost:5258/";
if (!Uri.TryCreate(baseAddressString, UriKind.Absolute, out var baseAddress))
    baseAddress = new Uri("http://localhost:5258/");


// HttpClient para consumir la API, usando TokenHandler
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = baseAddress;
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<TokenHandler>();

// Cookie auth para la app cliente (usamos claims del JWT)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
