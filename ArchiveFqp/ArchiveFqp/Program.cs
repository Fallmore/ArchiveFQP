using ArchiveFqp.Authentication;
using ArchiveFqp.Client.Pages;
using ArchiveFqp.Components;
using ArchiveFqp.Interfaces.Applications;
using ArchiveFqp.Interfaces.Auth;
using ArchiveFqp.Interfaces.DatabaseNotification;
using ArchiveFqp.Interfaces.FileUpload;
using ArchiveFqp.Interfaces.Hash;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Interfaces.Work;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.Applications;
using ArchiveFqp.Services.Auth;
using ArchiveFqp.Services.DatabaseNotification;
using ArchiveFqp.Services.ExpirationCheck;
using ArchiveFqp.Services.FileUpload;
using ArchiveFqp.Services.Hash;
using ArchiveFqp.Services.ReferenceData;
using ArchiveFqp.Services.User;
using ArchiveFqp.Services.Work;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
//builder.ServicesdScoped<StateContainer>();

// Подключение к БД
string conString = builder.Configuration.GetConnectionString("ArchiveFqpContext") ??
     throw new InvalidOperationException("Connection string 'ArchiveFqpContext'" +
    " not found.");
builder.Services.AddDbContextFactory<ArchiveFqpContext>(options =>
    options.UseNpgsql(conString));

#region Настройка аутентификации
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.MaxAge = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthService, AuthService>();
#endregion

#region Загрузка файлов
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
#endregion

// Раскомментируйте, если приложение работает за прокси-сервером и в компоненте InspectUser.razor
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Uncomment this if your proxy is on the same machine (e.g., localhost)  
    options.KnownProxies.Add(System.Net.IPAddress.Parse("127.0.0.1"));
});

var app = builder.Build();

// Раскомментируйте, если приложение работает за прокси-сервером
app.UseForwardedHeaders();

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
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ArchiveFqp.Client._Imports).Assembly);

app.Run();
