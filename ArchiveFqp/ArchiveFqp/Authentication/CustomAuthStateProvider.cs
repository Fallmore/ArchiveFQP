using ArchiveFqp.Interfaces.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace ArchiveFqp.Authentication
{
    public class CustomAuthStateProvider //: AuthenticationStateProvider
    {
        //private AuthenticationState authenticationState;

        //public CustomAuthStateProvider(IAuthService service)
        //{
        //    authenticationState = new AuthenticationState(service.CurrentUser);

        //    service.UserChanged += (newUser) =>
        //    {
        //        authenticationState = new AuthenticationState(newUser);
        //        NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        //    };
        //}

        //public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        //    Task.FromResult(authenticationState);



        //private readonly ProtectedLocalStorage _localStorage;
        //private readonly ILogger<CustomAuthStateProvider> _logger;
        //private readonly IJSRuntime _jsRuntime;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        //public CustomAuthStateProvider(IJSRuntime jsRuntime, IHttpContextAccessor httpContextAccessor)
        //{
        //    _jsRuntime = jsRuntime;
        //    _httpContextAccessor = httpContextAccessor;
        //}

        ////public CustomAuthStateProvider(ProtectedLocalStorage localStorage, ILogger<CustomAuthStateProvider> logger)
        ////{
        ////    _localStorage = localStorage;
        ////    _logger = logger;
        ////}

        //public override  Task<AuthenticationState> GetAuthenticationStateAsync()
        //{
        //    IEnumerable<Claim> claims = ParseClaimsFromJwt(token);

        //    var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        //    if (expClaim != null)
        //    {
        //        var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
        //        if (expTime <= DateTimeOffset.UtcNow)
        //        {
        //            //await RemoveTokenAsync();
        //            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        //        }
        //    }

        //    ClaimsIdentity identity = new (claims, "jwt");
        //    ClaimsPrincipal user = new (identity);

        //    return Task.FromResult(new AuthenticationState(user));
        //}

        //public async Task SetTokenAsync(string? token)
        //{
        //    if (string.IsNullOrEmpty(token))
        //    {
        //        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        //    }
        //    else
        //    {
        //        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        //        string? f = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        //    }

        //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        //    //try
        //    //{
        //    //    await _localStorage.SetAsync("authToken", token);
        //    //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    _logger.LogError(ex, "Ошибка при сохранении токена");
        //    //}
        //}

        //public async Task<string?> GetTokenFromSession()
        //{
        //    try
        //    {
        //        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
        //    }
        //    catch
        //    {
        //        return null;
        //    }

        //    //try
        //    //{
        //    //    var result = await _localStorage.GetAsync<string>("authToken");
        //    //    return result.Success ? result.Value : null;
        //    //}
        //    //catch (InvalidOperationException)
        //    //{
        //    //    // Это нормально во время пререндеринга
        //    //    return null;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    _logger.LogError(ex, "Ошибка при получении токена");
        //    //    return null;
        //    //}
        //}

        ////public async Task RemoveTokenAsync()
        ////{
        ////    try
        ////    {
        ////        await _localStorage.DeleteAsync("authToken");
        ////        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        _logger.LogError(ex, "Ошибка при удалении токена");
        ////    }
        ////}

        //public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        //{
        //    List<Claim> claims = new();
        //    string payload = jwt.Split('.')[1];
        //    byte[] jsonBytes = ParseBase64WithoutPadding(payload);
        //    Dictionary<string, object>? keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        //    if (keyValuePairs != null)
        //    {
        //        foreach (KeyValuePair<string, object> kvp in keyValuePairs)
        //        {
        //            if (kvp.Value is JsonElement element)
        //            {
        //                if (element.ValueKind == JsonValueKind.Array)
        //                {
        //                    foreach (JsonElement item in element.EnumerateArray())
        //                    {
        //                        claims.Add(new Claim(kvp.Key, item.ToString()));
        //                    }
        //                }
        //                else
        //                {
        //                    claims.Add(new Claim(kvp.Key, element.ToString()));
        //                }
        //            }
        //        }
        //    }

        //    return claims;
        //}

        //private static byte[] ParseBase64WithoutPadding(string base64)
        //{
        //    switch (base64.Length % 4)
        //    {
        //        case 2: base64 += "=="; break;
        //        case 3: base64 += "="; break;
        //    }
        //    return Convert.FromBase64String(base64);
        //}
    }
}
