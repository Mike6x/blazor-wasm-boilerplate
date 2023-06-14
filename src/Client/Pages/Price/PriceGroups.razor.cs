using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Price;

public partial class PriceGroups
{
    protected EntityServerTableContext<PriceGroupDto, Guid, UpdatePriceGroupRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["PriceGroup"],
            entityNamePlural: L["PriceGroups"],
            entityResource: FSHResource.PriceGroups,
            fields: new()
            {
                // new(PriceGroup => PriceGroup.Id, L["Id"], "Id"),
                new(PriceGroup => PriceGroup.Order, L["Order"], "Order"),
                new(PriceGroup => PriceGroup.Code, L["Code"], "Code"),
                new(PriceGroup => PriceGroup.Name, L["Name"], "Name"),
                new(PriceGroup => PriceGroup.Description, L["Description"], "Description"),
                new(PriceGroup => PriceGroup.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: PriceGroup => PriceGroup.Id,
            searchFunc: async filter => (await PriceGroupsClient
                .SearchAsync(filter.Adapt<SearchPriceGroupsRequest>()))
                .Adapt<PaginationResponse<PriceGroupDto>>(),
            createFunc: async PriceGroup => await PriceGroupsClient.CreateAsync(PriceGroup.Adapt<CreatePriceGroupRequest>()),
            updateFunc: async (id, PriceGroup) => await PriceGroupsClient.UpdateAsync(id, PriceGroup),
            deleteFunc: async id => await PriceGroupsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportPriceGroupsRequest>();
                return await PriceGroupsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportPriceGroupsRequest() { ExcelFile = FileUploadRequest };
                await PriceGroupsClient.ImportAsync(request);
            }
            );
}