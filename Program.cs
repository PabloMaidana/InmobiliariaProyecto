using Inmobiliaria.Web.Data;
using Inmobiliaria.Web.Security;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", opt => {
        opt.Cookie.Name = builder.Configuration["Auth:CookieName"] ?? "InmoAuth";
        opt.LoginPath = builder.Configuration["Auth:LoginPath"] ?? "/Auth/Login";
        opt.AccessDeniedPath = "/Auth/Denied";
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

builder.Services.AddSingleton(new Db(builder.Configuration.GetConnectionString("MySql")!));
builder.Services.AddScoped<AuthService>();

// Repositorios
builder.Services.AddScoped<PropietarioRepository>();
builder.Services.AddScoped<InquilinoRepository>();
builder.Services.AddScoped<TipoInmuebleRepository>();
builder.Services.AddScoped<InmuebleRepository>();
builder.Services.AddScoped<ContratoRepository>();
builder.Services.AddScoped<PagoRepository>();
builder.Services.AddScoped<UsuarioRepository>();


var app = builder.Build();
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

// Seed admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Db>();
    await db.SeedAdminAsync("admin@demo.local", "Admin", "Principal", "Admin123!");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
