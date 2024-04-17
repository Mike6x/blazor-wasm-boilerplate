using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Settings;

public partial class Dimensions
{
    protected EntityServerTableContext<DimensionDto, Guid, UpdateDimensionRequest> Context { get; set; } = default!;
    protected override void OnInitialized() =>
        Context = new(
            fields:
            [
                new(Dimension => Dimension.Order, L["Order"], "Order"),
                new(Dimension => Dimension.Code, L["Code"], "Code"),
                new(Dimension => Dimension.Name, L["Name"], "Name"),
                new(Dimension => Dimension.Value, L["Value"], "Value" ),
                new(Dimension => Dimension.Type, L["Type"], "Type" ),
                new(Dimension => Dimension.FatherName, L["Father Dimension"], "FatherName"),

                new(Dimension => Dimension.FullName, L["Full Name"], "FullName"),
                new(Dimension => Dimension.NativeName, L["Navtive Name"], "NativeName"),

                // new(Dimension => Dimension.FullNativeName, L["Full Native"], "FullNativeName"),
                // new(Dimension => Dimension.Description, L["Description"], "Description"),
                new(Dimension => Dimension.IsActive,  L["Active"], Type: typeof(bool)),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchDimensionsRequest>();
                searchFilter.Type = SearchTypeString;

                var result = await DimensionsClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<DimensionDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportDimensionsRequest>();
                exportFilter.Type = SearchTypeString;
                return await DimensionsClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: Dimension => Dimension.Id,
            createFunc: async Dimension => await DimensionsClient.CreateAsync(Dimension.Adapt<CreateDimensionRequest>()),
            updateFunc: async (id, Dimension) => await DimensionsClient.UpdateAsync(id, Dimension),
            deleteFunc: async id => await DimensionsClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportDimensionsRequest() { ExcelFile = FileUploadRequest };
                await DimensionsClient.ImportAsync(request);
            },
            entityName: L["Dimension"],
            entityNamePlural: L["Dimensions"],
            entityResource: FSHResource.Dimensions);

    #region Advanced Search
    private EntityTable<DimensionDto, Guid, UpdateDimensionRequest>? _table;
    private string? SearchTypeString { get; set; }
    private void OnTypeStringChanged()
    {
        _ = _table?.ReloadDataAsync();
    }
    #endregion
}