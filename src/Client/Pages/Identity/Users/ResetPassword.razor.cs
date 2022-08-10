using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Multitenancy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Text;

namespace FSH.BlazorWebAssembly.Client.Pages.Identity.Users;

public partial class ResetPassword
{
    private readonly ResetPasswordRequest _resetPasswordRequest = new();

    private CustomValidation? _customValidation;
    private bool BusySubmitting { get; set; }

    [Inject]
    private IUsersClient UsersClient { get; set; } = default!;
    private string Tenant { get; set; } = MultitenancyConstants.Root.Id;

    protected override void OnInitialized()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("Token", out var param))
        {
            string? queryToken = param[0];
            _resetPasswordRequest.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(queryToken));
        }
        else
        {
            Navigation.NavigateTo("/login");
        }

    }

    private async Task SubmitAsync()
    {
        BusySubmitting = true;

        string? sucessMessage = await ApiHelper.ExecuteCallGuardedAsync(
            () => UsersClient.ResetPasswordAsync(Tenant, _resetPasswordRequest),
            Snackbar,
            _customValidation);

        if (sucessMessage != null)
        {
            Snackbar.Add(sucessMessage, Severity.Info);
        }

        BusySubmitting = false;
    }

    private bool _passwordVisibility;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
    private void TogglePasswordVisibility()
    {
        if (_passwordVisibility)
        {
            _passwordVisibility = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _passwordVisibility = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }
}
