using Archive.Client.Pages;
using Archive.Components;
using Archive.Models.Database;
using Archive.Models.StateContainer;
using Archive.Properties;
using Archive.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<IWorkService, WorkService>();
//builder.Services.AddTransient<IApplicationService<Работа>, ApplicationService<Работа>>();
//builder.Services.AddScoped<ApplicationService<Работа>>();
builder.Services.AddTransient<StateContainer>();
builder.Services.AddScoped<StateContainer>();

// Подключение к БД
var conString = builder.Configuration.GetConnectionString("ArchiveFqpContext") ??
     throw new InvalidOperationException("Connection string 'ArchiveFqpContext'" +
    " not found.");
builder.Services.AddDbContextFactory<ArchiveFqpContext>(options =>
    options.UseNpgsql(conString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Archive.Client._Imports).Assembly);

app.Run();
