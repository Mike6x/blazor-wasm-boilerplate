using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace FSH.BlazorWebAssembly.Client.Infrastructure.Auth.AzureAd;

internal class AzureAdAuthenticationService(NavigationManager navigation) : IAuthenticationService
{
    private readonly NavigationManager _navigation = navigation;

    public AuthProvider ProviderType => AuthProvider.AzureAd;

    public void NavigateToExternalLogin(string returnUrl) =>
        _navigation.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(returnUrl)}");

    public Task<bool> LoginAsync(string tenantId, TokenRequest request) =>
        throw new NotImplementedException();

    public async Task LogoutAsync()
    {
        _navigation.NavigateToLogout("authentication/logout");
        await Task.CompletedTask;
    }

    public Task ReLoginAsync(string returnUrl)
    {
        NavigateToExternalLogin(returnUrl);
        return Task.CompletedTask;
    }
}

// internal class AzureAdAuthenticationService : IAuthenticationService
// {
//    private readonly SignOutSessionStateManager _signOut;
//    public AzureAdAuthenticationService(SignOutSessionStateManager signOut, NavigationManager navigation) =>
//    (_signOut, _navigation) = (signOut, navigation);
//    private readonly NavigationManager _navigation;
//    public AuthProvider ProviderType => AuthProvider.AzureAd;
//    public void NavigateToExternalLogin(string returnUrl) =>
//        _navigation.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
//    public Task<bool> LoginAsync(string tenantId, TokenRequest request) =>
//        throw new NotImplementedException();
//    public async Task LogoutAsync()
//    {
//        await _signOut.SetSignOutState();
//        _navigation.NavigateTo("authentication/logout");
//    }
//    public Task ReLoginAsync(string returnUrl)
//    {
//        NavigateToExternalLogin(returnUrl);
//        return Task.CompletedTask;
//    }
// }