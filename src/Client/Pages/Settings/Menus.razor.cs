using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components;

namespace FSH.BlazorWebAssembly.Client.Pages.Settings;

public partial class Menus
{
    [Inject]
    protected IMenusClient MenusClient { get; set; } = default!;

    protected EntityServerTableContext<MenuDto, Guid, UpdateMenuRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            fields:
            [
                new(Menu => Menu.Order, L["Order"], "Order"),
                new(Menu => Menu.Code, L["Code"], "Code"),
                new(Menu => Menu.Name, L["Name"], "Name"),
                new(Menu => Menu.Href, L["Href"], "Href"),
                new(Menu => Menu.Icon, L["Icon"], "Icon"),
                new(Menu => Menu.Parent, L["Parent"], "Parent"),
                new(Menu => Menu.Description, L["Description"], "Description"),
                new(Menu => Menu.IsActive, L["IsActive"], Type: typeof(bool)),
            ],
            searchFunc: async filter => (await MenusClient
                .SearchAsync(filter.Adapt<SearchMenusRequest>()))
                .Adapt<PaginationResponse<MenuDto>>(),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportMenusRequest>();
                return await MenusClient.ExportAsync(exportFilter);
            },
            idFunc: Menu => Menu.Id,
            createFunc: async Menu => await MenusClient.CreateAsync(Menu.Adapt<CreateMenuRequest>()),
            updateFunc: async (id, Menu) => await MenusClient.UpdateAsync(id, Menu),
            deleteFunc: async id => await MenusClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportMenusRequest() { ExcelFile = FileUploadRequest };
                await MenusClient.ImportAsync(request);
            },
            entityName: L["Menu"],
            entityNamePlural: L["Menus"],
            entityResource: FSHResource.Menus

            // exportAction: string.Empty
            );
}