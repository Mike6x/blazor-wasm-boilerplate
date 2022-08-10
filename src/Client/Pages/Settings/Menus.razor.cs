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
            entityName: L["Menu"],
            entityNamePlural: L["Menus"],
            entityResource: FSHResource.Menus,
            fields: new()
            {
                new(Menu => Menu.Name, L["Name"], "Name"),
                new(Menu => Menu.Code, L["Code"], "Code"),
                new(Menu => Menu.Parent, L["Parent"], "Parent"),
                new(Menu => Menu.Order, L["Order"], "Order"),
                new(Menu => Menu.Href, L["Href"], "Href"),
                new(Menu => Menu.Icon, L["Icon"], "Icon"),
                new(Menu => Menu.Description, L["Description"], "Description"),
            },
            idFunc: Menu => Menu.Id,
            searchFunc: async filter => (await MenusClient
                .SearchAsync(filter.Adapt<SearchMenusRequest>()))
                .Adapt<PaginationResponse<MenuDto>>(),
            createFunc: async Menu => await MenusClient.CreateAsync(Menu.Adapt<CreateMenuRequest>()),
            updateFunc: async (id, Menu) => await MenusClient.UpdateAsync(id, Menu),
            deleteFunc: async id => await MenusClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportMenusRequest>();
                return await MenusClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportMenusRequest() { ExcelFile = FileUploadRequest };
                await MenusClient.ImportAsync(request);
            }

            // exportAction: string.Empty
            );

}