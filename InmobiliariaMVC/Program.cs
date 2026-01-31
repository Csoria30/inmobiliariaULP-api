using InmobiliariaDTO.Constants;
using InmobiliariaMVC.Handlers;
using InmobiliariaMVC.Services.Implementations;
using InmobiliariaMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// 1) Configuración básica y MVC
//*******************************************************************************   

// Añadir soporte MVC / Views
builder.Services.AddControllersWithViews();


// 2) Configuración de autorización (políticas de roles)
//*******************************************************************************   

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole(RoleNames.Administrador));
    options.AddPolicy("Empleado", policy => policy.RequireRole(RoleNames.Empleado));
    options.AddPolicy("Propietario", policy => policy.RequireRole(RoleNames.Propietario));
    options.AddPolicy("Inquilino", policy => policy.RequireRole(RoleNames.Inquilino));
    options.AddPolicy("PropietarioOrAdmin", policy => policy.RequireRole(RoleNames.Propietario, RoleNames.Administrador));
});


// 3) Session, HttpContext y TokenHandler
//*******************************************************************************   

// - Session se usa para guardar el token de la API
// - TokenHandler lee la sesión y pone Authorization: Bearer <token> en cada petición
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Para que TokenHandler pueda acceder al HttpContext
builder.Services.AddHttpContextAccessor();
// Registrar TokenHandler como DelegatingHandler (se añade a HttpClient)
builder.Services.AddTransient<TokenHandler>();


// 4) Lectura de configuración de la API
//*******************************************************************************   

var apiSection = builder.Configuration.GetSection("ApiClient");
if (!apiSection.Exists())
    apiSection = builder.Configuration.GetSection("ApiSettings");

var baseAddressString = apiSection.GetValue<string>("BaseAddress") ?? "http://localhost:5258/";
if (!Uri.TryCreate(baseAddressString, UriKind.Absolute, out var baseAddress))
    baseAddress = new Uri("http://localhost:5258/");

// 5) Registrar HttpClients: cliente nombrado y/o typed clients (usar uno u otro)
//*******************************************************************************   

// HttpClient para consumir la API, usando TokenHandler
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = baseAddress;
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<TokenHandler>();

// Registrar servicios como typed clients — el framework inyectará un HttpClient configurado
builder.Services.AddHttpClient<IPersonaService, PersonaService>(client =>
{
    client.BaseAddress = baseAddress;
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<TokenHandler>();

builder.Services.AddHttpClient<IRoleService, RoleService>(client =>
{
    client.BaseAddress = baseAddress;
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<TokenHandler>();


// 6) Otros servicios que no dependen de HttpClient
//*******************************************************************************   

builder.Services.AddScoped<IAuthService, AuthService>();


// 7) Authentication cookies
//*******************************************************************************   

// - Usa cookies para mantener la sesión de usuario en la app
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/iniciar-sesion";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });


// 8) Construir app
//*******************************************************************************   

var app = builder.Build();

// 9) Pipeline HTTP
//*******************************************************************************   

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

// 10) Ruta personalizada para login amigable
//*******************************************************************************   

app.MapControllerRoute(name: "login", pattern: "iniciar-sesion/{**accion}", defaults: new { controller = "Auth", action = "Login" });
app.MapControllerRoute(name: "login_alt1", pattern: "login/{**accion}", defaults: new { controller = "Auth", action = "Login" });
app.MapControllerRoute(name: "login_alt2", pattern: "entrar/{**accion}", defaults: new { controller = "Auth", action = "Login" });
app.MapControllerRoute(name: "login_alt3", pattern: "inicio/{**accion}", defaults: new { controller = "Auth", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
