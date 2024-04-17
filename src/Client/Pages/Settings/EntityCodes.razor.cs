using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Settings;

public partial class EntityCodes
{
    protected EntityServerTableContext<EntityCodeDto, Guid, UpdateEntityCodeRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            fields:
            [

                // new(EntityCode => EntityCode.Id, L["Id"], "Id"),
                new(EntityCode => EntityCode.Order, L["Order"], "Order"),
                new(EntityCode => EntityCode.Code, L["Code"], "Code"),
                new(EntityCode => EntityCode.Name, L["Name"], "Name"),
                new(EntityCode => EntityCode.Type, L["Type"], "Type" ),
                new(EntityCode => EntityCode.Seperator, L["Seperator"], "Seperator"),
                new(EntityCode => EntityCode.Value, L["Value"], "Value"),
                new(EntityCode => EntityCode.Description, L["Description"], "Description"),
                new(EntityCode => EntityCode.IsActive, L["Active"], Type: typeof(bool)),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchEntityCodesRequest>();
                searchFilter.Type = SearchCodeType == default ? null : SearchCodeType;

                var result = await EntityCodesClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<EntityCodeDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportEntityCodesRequest>();
                exportFilter.Type = SearchCodeType == default ? null : SearchCodeType;

                return await EntityCodesClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: EntityCode => EntityCode.Id,
            createFunc: async EntityCode => await EntityCodesClient.CreateAsync(EntityCode.Adapt<CreateEntityCodeRequest>()),
            updateFunc: async (id, EntityCode) => await EntityCodesClient.UpdateAsync(id, EntityCode),
            deleteFunc: async id => await EntityCodesClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportEntityCodesRequest() { ExcelFile = FileUploadRequest };
                await EntityCodesClient.ImportAsync(request);
            },
            entityName: L["EntityCode"],
            entityNamePlural: L["EntityCodes"],
            entityResource: FSHResource.EntityCodes);

    #region Advanced Search
    private EntityTable<EntityCodeDto, Guid, UpdateEntityCodeRequest>? _table;

    private CodeType _searchCodeType = CodeType.All;
    private CodeType SearchCodeType
    {
        get => _searchCodeType;
        set
        {
            _searchCodeType = value;
            _ = _table?.ReloadDataAsync();
        }
    }
    #endregion
}