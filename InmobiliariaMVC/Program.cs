using InmobiliariaDTO.Constants;
using InmobiliariaMVC.Handlers;
using InmobiliariaMVC.Services.Implementations;
using InmobiliariaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//Politicas de autorizacion
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole(RoleNames.Administrador));
    options.AddPolicy("Empleado", policy => policy.RequireRole(RoleNames.Empleado));
    options.AddPolicy("Propietario", policy => policy.RequireRole(RoleNames.Propietario));
    options.AddPolicy("Inquilino", policy => policy.RequireRole(RoleNames.Inquilino));
    options.AddPolicy("PropietarioOrAdmin", policy => policy.RequireRole(RoleNames.Propietario, RoleNames.Administrador));
});

// Session y acceso al HttpContext para obtener TOKEN
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>(); // Registrar el TokenHandler que inyecta el Authorization header desde el HttpContext/Session

//Inyeccion de dependencias - Repositorios y Servicios
builder.Services.AddScoped<IAuthService, AuthService>();



// Leer configuración API
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
        options.LoginPath = "/iniciar-sesion";
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

// Ruta personalizada para login amigable
app.MapControllerRoute(name: "login", pattern: "iniciar-sesion/{**accion}", defaults: new { controller = "Auth", action = "Login" });
app.MapControllerRoute(name: "login", pattern: "login/{**accion}", defaults: new { controller = "Auth", action = "Login" });
app.MapControllerRoute(name: "login", pattern: "entrar/{**accion}", defaults: new { controller = "Auth", action = "Login" });
app.MapControllerRoute(name: "login", pattern: "inicio/{**accion}", defaults: new { controller = "Auth", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
