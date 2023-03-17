using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Property;

public partial class AssetStatuses
{
    protected EntityServerTableContext<AssetStatusDto, Guid, UpdateAssetStatusRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["AssetStatus"],
            entityNamePlural: L["AssetStatuses"],
            entityResource: FSHResource.AssetStatuses,
            fields: new()
            {
                // new(AssetStatus => AssetStatus.Id, L["Id"], "Id"),
                new(AssetStatus => AssetStatus.Code, L["Code"], "Code"),
                new(AssetStatus => AssetStatus.Name, L["Name"], "Name"),
                new(AssetStatus => AssetStatus.Type, L["Type"], "Type" ),
                new(AssetStatus => AssetStatus.Description, L["Description"], "Description"),

            },
            idFunc: AssetStatus => AssetStatus.Id,
            searchFunc: async filter => (await AssetStatusesClient
                .SearchAsync(filter.Adapt<SearchAssetStatusesRequest>()))
                .Adapt<PaginationResponse<AssetStatusDto>>(),
            createFunc: async AssetStatus => await AssetStatusesClient.CreateAsync(AssetStatus.Adapt<CreateAssetStatusRequest>()),
            updateFunc: async (id, AssetStatus) => await AssetStatusesClient.UpdateAsync(id, AssetStatus),
            deleteFunc: async id => await AssetStatusesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportAssetStatusesRequest>();
                return await AssetStatusesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportAssetStatusesRequest() { ExcelFile = FileUploadRequest };
                await AssetStatusesClient.ImportAsync(request);
            }
            );
}