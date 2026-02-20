using MudBlazor.Services;
using CienEstudiantesDijeron.Components;
using Microsoft.EntityFrameworkCore;
using CienEstudiantesDijeron.Data;
using CienEstudiantesDijeron.Services;
using CienEstudiantesDijeron;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Se agrega el servicio de Juego Service como un Singleton
builder.Services.AddSingleton<JuegoService>();
//Se agrega el servicio de Importador Service como un Scoped
builder.Services.AddScoped<ImportadorService>();

// Para utilizar una base de datos SQL Server
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Se agrega el servicio para usar una base de datos SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
