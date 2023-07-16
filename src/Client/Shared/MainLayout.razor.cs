using FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;
using FSH.BlazorWebAssembly.Client.Infrastructure.Preferences;
using FSH.WebApi.Shared.Notifications;
using MediatR.Courier;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System.Security.Claims;

namespace FSH.BlazorWebAssembly.Client.Shared;

public partial class MainLayout
{
    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;
    [Parameter]
    public EventCallback OnDarkModeToggle { get; set; }
    [Parameter]
    public EventCallback<bool> OnRightToLeftToggle { get; set; }

    private bool _drawerOpen;
    private bool _rightToLeft;

    // Chat Section
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    private ICourier Courier { get; set; } = default!;
    private string CurrentUserId { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (await ClientPreferences.GetPreference() is ClientPreference preference)
        {
            _rightToLeft = preference.IsRTL;
            _drawerOpen = preference.IsDrawerOpen;
        }

        Courier.SubscribeWeak<NotificationWrapper<ReceiveChatNotification>>(async wrapper =>
        {
            await LoadDataAsync(
                wrapper.Notification.Message,
                wrapper.Notification.ReceiverUserId,
                wrapper.Notification.SenderUserId);
        });
    }

    private async Task LoadDataAsync(string message, string receiverUserId, string senderUserId)
    {
        if ((await AuthState).User is { } user)
        {
            CurrentUserId = user.GetUserId()!;
        }

        if (CurrentUserId == receiverUserId)
        {
            _ = _jsRuntime.InvokeAsync<string>("PlayAudio", "notification");
            Snackbar.Add(message, Severity.Info, config =>
            {
                config.VisibleStateDuration = 10000;
                config.HideTransitionDuration = 500;
                config.ShowTransitionDuration = 500;
                config.Action = "Chat?";
                config.ActionColor = Color.Info;
                config.Onclick = snackbar =>
                {
                    Navigation.NavigateTo($"Communication/chat/{senderUserId}");
                    return Task.CompletedTask;
                };
            });
        }
    }

    private async Task RightToLeftToggle()
    {
        bool isRtl = await ClientPreferences.ToggleLayoutDirectionAsync();
        _rightToLeft = isRtl;

        await OnRightToLeftToggle.InvokeAsync(isRtl);
    }

    public async Task ToggleDarkMode()
    {
        await OnDarkModeToggle.InvokeAsync();
    }

    private async Task DrawerToggle()
    {
        _drawerOpen = await ClientPreferences.ToggleDrawerAsync();
    }

    private void Logout()
    {
        var parameters = new DialogParameters
            {
                { nameof(Dialogs.Logout.ContentText), $"{L["Logout Confirmation"]}"},
                { nameof(Dialogs.Logout.ButtonText), $"{L["Logout"]}"},
                { nameof(Dialogs.Logout.Color), Color.Error}
            };

        var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.Show<Dialogs.Logout>(L["Logout"], parameters, options);
    }

    private void Profile()
    {
        Navigation.NavigateTo("/account");
    }
}