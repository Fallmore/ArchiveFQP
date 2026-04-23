using ArchiveFqp.Client.Pages;
using ArchiveFqp.Components;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.Applications;
using ArchiveFqp.Services.DatabaseNotification;
using ArchiveFqp.Services.ExpirationCheck;
using ArchiveFqp.Services.FileUpload;
using ArchiveFqp.Services.Hash;
using ArchiveFqp.Services.ReferenceData;
using ArchiveFqp.Services.User;
using ArchiveFqp.Services.Work;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSingleton<SettingsArchive>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDatabaseNotificationService, DatabaseNotificationService>();
builder.Services.AddSingleton<ReferenceDataService>();
builder.Services.AddSingleton<IReferenceDataService>(sp => sp.GetRequiredService<ReferenceDataService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<ReferenceDataService>());
builder.Services.AddHostedService<ExpirationCheckService>();

builder.Services.AddScoped<IWorkService, WorkService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApplicationsService, ApplicationsService>();
//builder.Services.AddTransient<StateContainer>();
//builder.Services.AddScoped<StateContainer>();

// Подключение к БД
var conString = builder.Configuration.GetConnectionString("ArchiveFqpContext") ??
     throw new InvalidOperationException("Connection string 'ArchiveFqpContext'" +
    " not found.");
builder.Services.AddDbContextFactory<ArchiveFqpContext>(options =>
    options.UseNpgsql(conString));

// Загрузка файлов
builder.Services.AddScoped<IHashService, Sha256HashService>();
builder.Services.AddScoped<WorkFileUploadService>();
builder.Services.AddScoped<IFileUploadService, WorkFileUploadService>(sp =>
    sp.GetRequiredService<WorkFileUploadService>());
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
    options.MemoryBufferThreshold = int.MaxValue;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
    // Таймауты
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});

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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ArchiveFqp.Client._Imports).Assembly);

app.Run();
