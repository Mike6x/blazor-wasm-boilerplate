using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Communication;

public partial class ChatMessages
{
    protected EntityServerTableContext<ChatMessageDto, Guid, UpdateChatMessageRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["ChatMessage"],
            entityNamePlural: L["ChatMessages"],
            entityResource: FSHResource.ChatMessages,
            fields: new()
            {
                new(ChatMessage => ChatMessage.IsRead, L["Read"], Type: typeof(bool)),

                // new(ChatMessage => ChatMessage.FromUserId, L["FromUserId"], "FromUserId"),
                // new(ChatMessage => ChatMessage.ToUserId, L["ToUserId"], "ToUserId"),
                // new(ChatMessage => ChatMessage.FromUserUserName, L["FromUser"], "FromUser.UserName"),
                // new(ChatMessage => ChatMessage.ToUserUserName, L["ToUser"], "ToUser.UserName"),
                new(ChatMessage => ChatMessage.FromUserEmail, L["FromUser"], "FromUser.Email"),
                new(ChatMessage => ChatMessage.ToUserEmail, L["ToUser"], "ToUser.Email"),
                new(ChatMessage => ChatMessage.Message, L["Message"], "Message"),
                new(ChatMessage => ChatMessage.CreatedDate, L["CreatedDate"], "CreatedDate"),

                new(ChatMessage => ChatMessage.FromUserFirstName, L["FromUser F.Name"], "FromUser.FirstName"),
                new(ChatMessage => ChatMessage.FromUserLastName, L["FromUser L.Name"], "FromUser.LastName"),
                new(ChatMessage => ChatMessage.ToUserFirstName, L["ToUser F.Name"], "ToUser.FirstName"),
                new(ChatMessage => ChatMessage.ToUserLastName, L["ToUser L.Name"], "ToUser.LastName"),
            },
            idFunc: ChatMessage => ChatMessage.Id,
            searchFunc: async filter => (await ChatMessagesClient
                .SearchAsync(filter.Adapt<SearchChatMessagesRequest>()))
                .Adapt<PaginationResponse<ChatMessageDto>>(),
            createFunc: async ChatMessage => await ChatMessagesClient.CreateAsync(ChatMessage.Adapt<CreateChatMessageRequest>()),
            updateFunc: async (id, ChatMessage) => await ChatMessagesClient.UpdateAsync(id, ChatMessage),
            deleteFunc: async id => await ChatMessagesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportChatMessagesRequest>();
                return await ChatMessagesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportChatMessagesRequest() { ExcelFile = FileUploadRequest };
                await ChatMessagesClient.ImportAsync(request);
            }
            );
}