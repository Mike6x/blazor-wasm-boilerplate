using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;
using FSH.BlazorWebAssembly.Client.Shared;
using FSH.WebApi.Shared.Notifications;
using MediatR.Courier;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Claims;

namespace FSH.BlazorWebAssembly.Client.Pages.Communication;
public partial class Chat
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;

    [CascadingParameter]
    public required HubConnection hubConnection { get; set; }

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

    public List<ApplicationUser> ChatUsers = new List<ApplicationUser>();

    [Inject]
    private ICourier Courier { get; set; } = default!;
    [Inject]
    private IMessagesClient MessagesClient { get; set; } = default!;

    private bool _loaded;
    private bool ContactSelected { get; set; }
    private List<ChatMessage> _messages = new();

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    await _jsRuntime.InvokeAsync<string>("ScrollToBottom", "chatContainer");
    //}

    protected override async Task OnInitializedAsync()
    {
        Courier.SubscribeWeak<NotificationWrapper<StatsChangedNotification>>(async _ =>
        {
            await LoadDataAsync();
            StateHasChanged();
        });

        await LoadDataAsync();

        _loaded = true;
    }

    private async Task LoadDataAsync()
    {
        if ((await AuthState).User is { } user)
        {
            CurrentUserId = user.GetUserId() ?? string.Empty;
            CurrentUserEmail = user.GetEmail() ?? string.Empty;
        }

        if (await ApiHelper.ExecuteCallGuardedAsync(() => MessagesClient.GetUsersAsync(), Snackbar)
            is List<ApplicationUser> chatUsers)
        {
            ChatUsers = chatUsers;
            var state = await AuthState;
        }
    }

    private async Task LoadUserChat(string userId)
    {
        ContactSelected = true;

        var contact = await MessagesClient.GetUserDetailsAsync(userId);
        ContactId = contact.Id;
        ContactEmail = contact.Email ?? contact.Id;
        Navigation.NavigateTo($"Communication/chat/{ContactId}");
        _messages = new List<ChatMessage>();
        _messages = (List<ChatMessage>)await MessagesClient.GetConversationAsync(ContactId);
    }

    private async Task GetUsersAsync()
    {
        ChatUsers = (List<ApplicationUser>)await MessagesClient.GetUsersAsync();
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
            await MessagesClient.CreateAsync(chatHistory);

            // await MessagesClient.SaveMessageAsync(chatHistory);
            chatHistory.FromUserId = CurrentUserId;

            // await hubConnection.SendAsync("SendMessageAsync", chatHistory, CurrentUserEmail);
            // StateHasChanged();
            CurrentMessage = string.Empty;
        }
    }
}


//protected override async Task OnInitializedAsync()
//{
//    await GetUsersAsync();
//    var state = await AuthState;
//    var user = state.User;
//    CurrentUserId = user.Claims.Where(a => a.Type == "sub").Select(a => a.Value).FirstOrDefault();
//    CurrentUserEmail = user.Claims.Where(a => a.Type == "name").Select(a => a.Value).FirstOrDefault();
//    StateHasChanged();

//    if (!string.IsNullOrEmpty(ContactId))
//    {
//        await LoadUserChat(ContactId);
//    }
//    if (hubConnection == null)
//    {
//        hubConnection = new HubConnectionBuilder().WithUrl(Navigation.ToAbsoluteUri("/signalRHub")).Build();
//    }

//    if (hubConnection.State == HubConnectionState.Disconnected)
//    {
//        await hubConnection.StartAsync();
//    }

//    hubConnection.On<Message, string>("ReceiveMessage", async (message, userName) =>
//    {
//        if ((ContactId == message.ToUserId && CurrentUserId == message.FromUserId) || (ContactId == message.FromUserId && CurrentUserId == message.ToUserId))
//        {

//            if ((ContactId == message.ToUserId && CurrentUserId == message.FromUserId))
//            {
//                _messages.Add(new Message { MessageText = message.MessageText, CreatedDate = message.CreatedDate, FromUser = new ApplicationUser() { Email = CurrentUserEmail } });
//                await hubConnection.SendAsync("ChatNotificationAsync", $"New Message From {userName}", ContactId, CurrentUserId);
//            }
//            else if ((ContactId == message.FromUserId && CurrentUserId == message.ToUserId))
//            {
//                _messages.Add(new Message { MessageText = message.MessageText, CreatedDate = message.CreatedDate, FromUser = new ApplicationUser() { Email = ContactEmail } });
//            }

//            await _jsRuntime.InvokeAsync<string>("ScrollToBottom", "chatContainer");
//           StateHasChanged();
//        }
//    });

//    //await GetUsersAsync();

//    //// var state = await _stateProvider.GetAuthenticationStateAsync();
//    //var state = await AuthState;
//    //var user = state.User;

//    //CurrentUserId = user.Claims.Where(a => a.Type == "sub").Select(a => a.Value).FirstOrDefault();
//    //CurrentUserEmail = user.Claims.Where(a => a.Type == "name").Select(a => a.Value).FirstOrDefault();
//    //if (!string.IsNullOrEmpty(ContactId))
//    //{
//    //    await LoadUserChat(ContactId);
//    //}

//    // StateHasChanged();
//}

