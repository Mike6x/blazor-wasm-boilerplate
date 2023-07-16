namespace FSH.WebApi.Shared.Notifications;

public class ReceiveChatNotification : INotificationMessage
{
    public string Message { get; set; } = default!;
    public string ReceiverUserId { get; set; } = default!;
    public string SenderUserId { get; set; } = default!;
}
