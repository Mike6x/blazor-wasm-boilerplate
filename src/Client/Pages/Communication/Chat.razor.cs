using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Notifications;
using MediatR.Courier;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace FSH.BlazorWebAssembly.Client.Pages.Communication;
public partial class Chat
{
    [Parameter]
    public required string CurrentMessage { get; set; }
    [Parameter]
    public required string CurrentUserId { get; set; }
    [Parameter]
    public required string CurrentUserEmail { get; set; }
    [Parameter]
    public string ContactEmail { get; set; } = default!;
    [Parameter]
    public string ContactId { get; set; } = default!;

    public List<UserDetailsDto> ChatUsers = new();

    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject]
    private ICourier Courier { get; set; } = default!;

    [Inject]
    private IChatMessagesClient ChatMessagesClient { get; set; } = default!;
    [Inject]
    private IUsersClient UsersClient { get; set; } = default!;

    private bool _loaded;
    private bool ContactSelected { get; set; }
    private List<ChatMessageDto> _messages = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await _jsRuntime.InvokeAsync<string>("ScrollToBottom", "chatContainer");
    }

    protected override async Task OnInitializedAsync()
    {
        Courier.SubscribeWeak<NotificationWrapper<ReceiveChatNotification>>(async _ =>
        {
            await ReLoadDataAsync();
            StateHasChanged();
        });

        await LoadDataAsync();

        _loaded = true;
    }

    private async Task LoadDataAsync()
    {
        if ((await AuthState).User is { } user)
        {
            CurrentUserId = user.GetUserId();
            CurrentUserEmail = user.GetEmail();
        }

        if (await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.GetChatUsersAsync(CurrentUserId), Snackbar)
            is List<UserDetailsDto> chatUsers)
        {
            ChatUsers = chatUsers;
        }

        if (!string.IsNullOrEmpty(ContactId))
        {
            await LoadUserChat(ContactId);
        }
    }

    private async Task ReLoadDataAsync()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(() => UsersClient.GetChatUsersAsync(CurrentUserId), Snackbar)
            is List<UserDetailsDto> chatUsers)
        {
            ChatUsers = chatUsers;
        }

        if (!string.IsNullOrEmpty(ContactId))
        {
            await LoadUserChat(ContactId);
        }
    }

    private async Task LoadUserChat(string userId)
    {
        ContactSelected = true;

        var contact = await UsersClient.GetChatUserAsync(userId);
        if (contact != null)
        {
            ContactId = contact.Id.ToString();
            ContactEmail = contact.Email ?? contact.UserName ?? contact.Id.ToString();
            Navigation.NavigateTo($"Communication/chat/{ContactId}");
            _messages = (List<ChatMessageDto>)await ChatMessagesClient.GetConversationAsync(CurrentUserId, ContactId);
        }
    }

    private async Task SubmitAsync()
    {
        if (!string.IsNullOrEmpty(CurrentMessage) && !string.IsNullOrEmpty(ContactId))
        {
            // Save Message to DB
            var chatHistory = new CreateChatMessageRequest()
            {
                FromUserId = CurrentUserId,
                ToUserId = ContactId,
                Message = CurrentMessage,
                CreatedDate = DateTime.UtcNow,
                IsRead = false
            };
            await ChatMessagesClient.CreateAsync(chatHistory);

            CurrentMessage = string.Empty;
            await LoadUserChat(ContactId);
        }
    }
}