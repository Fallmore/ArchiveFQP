using ArchiveFqp;
using ArchiveFqp.Components;
using ArchiveFqp.Hubs;
using ArchiveFqp.Interfaces.Applications;
using ArchiveFqp.Interfaces.Attributes;
using ArchiveFqp.Interfaces.Auth;
using ArchiveFqp.Interfaces.DatabaseNotification;
using ArchiveFqp.Interfaces.FileUpload;
using ArchiveFqp.Interfaces.Hash;
using ArchiveFqp.Interfaces.PdfRender;
using ArchiveFqp.Interfaces.ReferenceData;
using ArchiveFqp.Interfaces.Schedule;
using ArchiveFqp.Interfaces.Settings;
using ArchiveFqp.Interfaces.Student;
using ArchiveFqp.Interfaces.Teacher;
using ArchiveFqp.Interfaces.User;
using ArchiveFqp.Interfaces.Work;
using ArchiveFqp.Jobs;
using ArchiveFqp.Models.Auth;
using ArchiveFqp.Models.Database;
using ArchiveFqp.Models.Settings.SettingsArchive;
using ArchiveFqp.Services.AiExtractor;
using ArchiveFqp.Services.Applications;
using ArchiveFqp.Services.Attributes;
using ArchiveFqp.Services.Auth;
using ArchiveFqp.Services.Auth.ThreeKL;
using ArchiveFqp.Services.Database;
using ArchiveFqp.Services.Email;
using ArchiveFqp.Services.ExpirationCheck;
using ArchiveFqp.Services.FileUpload;
using ArchiveFqp.Services.Hash;
using ArchiveFqp.Services.Notifications;
using ArchiveFqp.Services.PdfRender;
using ArchiveFqp.Services.ReferenceData;
using ArchiveFqp.Services.Report;
using ArchiveFqp.Services.Schedule;
using ArchiveFqp.Services.Settings;
using ArchiveFqp.Services.Student;
using ArchiveFqp.Services.Teacher;
using ArchiveFqp.Services.User;
using ArchiveFqp.Services.Work;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddSignalR();
builder.Services.AddControllers();
//builder.Services.AddHsts(options =>
//{
//    options.Preload = true;
//    options.IncludeSubDomains = true;
//    options.MaxAge = TimeSpan.FromDays(1);
//    options.ExcludedHosts.Add("example.com");
//    options.ExcludedHosts.Add("www.example.com");
//});

// Для докера
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"));

#warning Впишите свой пароль в appsettings.json
#warning Впишите свои параметры авторизации 3KL в appsettings.json
#warning Впишите свои настройки почты в appsettings.json
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    // Переменные окружения ПЕРЕОПРЕДЕЛЯЮТ appsettings.json
    .AddEnvironmentVariables();

// Подключение к БД
string conString = builder.Configuration.GetConnectionString("ArchiveFqpContext") ??
     throw new InvalidOperationException("Connection string 'ArchiveFqpContext'" +
    " not found.");
builder.Services.AddDbContextFactory<ArchiveFqpContext>(options =>
    options.UseNpgsql(conString));

builder.Services.AddSingleton<SettingsArchive>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDatabaseNotificationService, DatabaseNotificationService>();
builder.Services.AddSingleton<DatabaseBackupService>();
builder.Services.AddSingleton<ReferenceDataService>();
builder.Services.AddSingleton<IReferenceDataService>(sp => sp.GetRequiredService<ReferenceDataService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<ReferenceDataService>());
builder.Services.AddHostedService<ExpirationCheckService>();

builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IWorkService, WorkService>();
builder.Services.AddScoped<IAttributeService, AttributeService>();
builder.Services.AddScoped<IApplicationsService, ApplicationsService>();
builder.Services.AddScoped<EmailService>();

builder.Services.AddScoped<IPdfRenderService, PdfSciaRenderService>();
builder.Services.AddScoped<ReportService>();

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

#region Настройка аутентификации
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ReturnUrlParameter = "returnUrl";
        options.Cookie.MaxAge = TimeSpan.FromHours(8);
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();    
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddHttpClient<ThreeKlAuthService>();
builder.Services.AddScoped<ThreeKlAuthService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
#endregion

builder.Services.AddSingleton<UserConnectionManager>();
builder.Services.AddScoped<NotificationService>();

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
    options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500 MB
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
});
#endregion

#region Настройка Quartz.NET

builder.Services.AddQuartz(q =>
{
    JobKey jobKey = new(nameof(PostgresBackupJob));
    q.AddJob<PostgresBackupJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity(nameof(PostgresBackupJob) + "Trigger")
        // Каждый день в 00:00
        .WithCronSchedule("0 0 0 * * ?")
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddScoped<IScheduleService, ScheduleService>();

#endregion

// Раскомментируйте, если приложение работает за прокси-сервером и в компоненте Components/Components/InspectUser.razor
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Uncomment this if your proxy is on the same machine (e.g., localhost)  
    options.KnownProxies.Add(System.Net.IPAddress.Parse("127.0.0.1"));
});

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.Configuration["AiExtractor:BaseUrl"]
        ?? "http://localhost:8000")
});
builder.Services.AddScoped<AiExtractorService>();

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
app.UseStaticFiles();
app.UseRouting();

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapHub<NotificationHub>("/notificationHub");
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ArchiveFqp.Client._Imports).Assembly);

app.Run();
