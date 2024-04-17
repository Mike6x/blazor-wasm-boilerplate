using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Place;

public partial class Channels
{
    protected EntityServerTableContext<ChannelDto, Guid, UpdateChannelRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            fields:
            [

                // new(Channel => Channel.Id, L["Id"], "Id"),
                new(Channel => Channel.Order, L["Order"], "Order"),
                new(Channel => Channel.Code, L["Code"], "Code"),
                new(Channel => Channel.Name, L["Name"], "Name"),
                new(Channel => Channel.Description, L["Description"], "Description"),
                new(Channel => Channel.IsActive, L["Active"], Type: typeof(bool)),
            ],
            searchFunc: async filter => (await ChannelsClient
                .SearchAsync(filter.Adapt<SearchChannelsRequest>()))
                .Adapt<PaginationResponse<ChannelDto>>(),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportChannelsRequest>();
                return await ChannelsClient.ExportAsync(exportFilter);
            },
            idFunc: Channel => Channel.Id,
            createFunc: async Channel => await ChannelsClient.CreateAsync(Channel.Adapt<CreateChannelRequest>()),
            updateFunc: async (id, Channel) => await ChannelsClient.UpdateAsync(id, Channel),
            deleteFunc: async id => await ChannelsClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportChannelsRequest() { ExcelFile = FileUploadRequest };
                await ChannelsClient.ImportAsync(request);
            },
            entityName: L["Channel"],
            entityNamePlural: L["Channels"],
            entityResource: FSHResource.Channels);
}