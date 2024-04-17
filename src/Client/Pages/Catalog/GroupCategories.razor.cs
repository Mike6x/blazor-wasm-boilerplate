using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class GroupCategories
{
    protected EntityServerTableContext<GroupCategorieDto, Guid, UpdateGroupCategorieRequest> Context { get; set; } = default!;
    private EntityTable<GroupCategorieDto, Guid, UpdateGroupCategorieRequest>? _table;
    protected override void OnInitialized() =>
        Context = new(
            fields:
            [

                // new(GroupCategorie => GroupCategorie.Id, L["Id"], "Id"),
                new(GroupCategorie => GroupCategorie.Order, L["Order"], "Order"),
                new(GroupCategorie => GroupCategorie.Code, L["Code"], "Code"),
                new(GroupCategorie => GroupCategorie.Name, L["Name"], "Name"),
                new(GroupCategorie => GroupCategorie.Description, L["Description"], "Description"),

                new(GroupCategorie => GroupCategorie.BusinessLineName, L["BusinessLine"], "BusinessLine.Name"),
                new(GroupCategorie => GroupCategorie.Type, L["Type"], "Type" ),
                new(GroupCategorie => GroupCategorie.IsActive, L["Active"], Type: typeof(bool)),
            ],
            searchFunc: async filter =>
            {
                var searchFilter = filter.Adapt<SearchGroupCategoriesRequest>();
                searchFilter.BusinessLineId = SearchBusinessLineId == default ? null : SearchBusinessLineId;
                searchFilter.Type = SearchCatalogType == default ? null : SearchCatalogType;

                var result = await GroupCategoriesClient.SearchAsync(searchFilter);
                return result.Adapt<PaginationResponse<GroupCategorieDto>>();
            },
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportGroupCategoriesRequest>();
                exportFilter.BusinessLineId = SearchBusinessLineId == default ? null : SearchBusinessLineId;
                exportFilter.Type = SearchCatalogType == default ? null : SearchCatalogType;

                return await GroupCategoriesClient.ExportAsync(exportFilter);
            },
            enableAdvancedSearch: true,
            idFunc: GroupCategorie => GroupCategorie.Id,
            createFunc: async GroupCategorie => await GroupCategoriesClient.CreateAsync(GroupCategorie.Adapt<CreateGroupCategorieRequest>()),
            updateFunc: async (id, GroupCategorie) => await GroupCategoriesClient.UpdateAsync(id, GroupCategorie),
            deleteFunc: async id => await GroupCategoriesClient.DeleteAsync(id),
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportGroupCategoriesRequest() { ExcelFile = FileUploadRequest };
                await GroupCategoriesClient.ImportAsync(request);
            },
            entityName: L["GroupCategorie"],
            entityNamePlural: L["GroupCategories"],
            entityResource: FSHResource.GroupCategories);

    #region Advanced Search

    private Guid _searchBusinessLineId;
    private Guid SearchBusinessLineId
    {
        get => _searchBusinessLineId;
        set
        {
            _searchBusinessLineId = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    private CatalogType _searchCatalogType = CatalogType.All;
    private CatalogType SearchCatalogType
    {
        get => _searchCatalogType;
        set
        {
            _searchCatalogType = value;
            _ = _table?.ReloadDataAsync();
        }
    }

    #endregion
}