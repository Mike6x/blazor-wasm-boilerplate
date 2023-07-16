using FSH.WebApi.Shared.Notifications;

namespace FSH.BlazorWebAssembly.Client.Infrastructure.Notifications;

public record ChatMessageReceived(string? Message, string? ReceiverUserId, string? SenderUserId) : INotificationMessage;